using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Tune.Helpers.Windows
{
    /// <summary>
    /// Provides methods to apply blur effects to WPF windows, supporting both Windows 11 Mica/Acrylic and Windows 10 Acrylic blur.
    /// </summary>
    public static class WindowBlurHelper
    {
        // ==== Windows 11 Backdrop Types ====
        // Reference: DWM_SYSTEMBACKDROP_TYPE
        // https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type
        private enum DwmSystemBackdropType
        {
            DWMSBT_AUTO = 0,
            DWMSBT_NONE = 1,
            DWMSBT_MAINWINDOW = 2,      // Mica
            DWMSBT_TRANSIENTWINDOW = 3, // Acrylic
            DWMSBT_TABBEDWINDOW = 4     // Mica Alt
        }

        // DwmSetWindowAttribute attribute ID for system backdrop type.
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

        // ==== Windows 10 Accent States ====
        private enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4, // Acrylic (Windows 10 1803+)
            ACCENT_ENABLE_HOSTBACKDROP = 5,
            ACCENT_INVALID_STATE = 6
        }

        // Attribute ID for SetWindowCompositionAttribute.
        private const int WCA_ACCENT_POLICY = 19;

        // ==== Native Interop Structures ====

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor; // Format: AABBGGRR
            public int AnimationId;
        }

        // ==== P/Invoke Declarations ====

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize);

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(
            IntPtr hwnd,
            ref WindowCompositionAttributeData data);

        // ==== Public API ====

        /// <summary>
        /// Applies a blur effect to the specified window.
        /// Tries Windows 11 Mica/Acrylic first; falls back to Windows 10 Acrylic blur.
        /// </summary>
        /// <param name="window">The WPF Window to apply the effect to.</param>
        /// <param name="useAcrylic">
        /// If true, uses Acrylic (transient window) on Windows 11.
        /// If false, uses Mica (main window) on Windows 11.
        /// </param>
        /// <param name="gradientColor">
        /// Optional gradient color for Windows 10 fallback (format: AABBGGRR).
        /// Default is a semi-transparent dark gray.
        /// </param>
        public static void ApplyBlur(Window window, bool useAcrylic = true, int? gradientColor = null)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            // Ensure the window handle is created.
            var helper = new WindowInteropHelper(window);
            IntPtr hwnd = helper.Handle;

            // If the handle isn't ready yet, hook into SourceInitialized.
            if (hwnd == IntPtr.Zero)
            {
                window.SourceInitialized += (_, _) => ApplyBlur(window, useAcrylic, gradientColor);
                return;
            }

            // Make the window background transparent so the blur shows through.
            window.Background = Brushes.Transparent;

            // Try Windows 11 native backdrop first.
            if (TryApplyWindows11Backdrop(hwnd, useAcrylic))
                return;

            // Fall back to Windows 10 Acrylic blur.
            ApplyWindows10Blur(hwnd, gradientColor ?? unchecked((int)0xCC000000));
        }

        /// <summary>
        /// Removes any blur effect from the window.
        /// </summary>
        public static void RemoveBlur(Window window)
        {
            if (window == null) return;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;

            // Try to disable Windows 11 backdrop.
            int none = (int)DwmSystemBackdropType.DWMSBT_NONE;
            DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref none, sizeof(int));

            // Also disable Windows 10 accent policy.
            var accent = new AccentPolicy { AccentState = AccentState.ACCENT_DISABLED };
            var data = new WindowCompositionAttributeData
            {
                Attribute = WCA_ACCENT_POLICY,
                Data = Marshal.AllocHGlobal(Marshal.SizeOf(accent)),
                SizeOfData = Marshal.SizeOf(accent)
            };
            Marshal.StructureToPtr(accent, data.Data, false);
            SetWindowCompositionAttribute(hwnd, ref data);
            Marshal.FreeHGlobal(data.Data);

            window.Background = null; // Restore default background.
        }

        // ==== Private Implementations ====

        /// <summary>
        /// Attempts to apply Windows 11 Mica or Acrylic backdrop.
        /// Returns true if successful.
        /// </summary>
        private static bool TryApplyWindows11Backdrop(IntPtr hwnd, bool useAcrylic)
        {
            // Check OS version: Windows 11 build 22621+ is required for DWMWA_SYSTEMBACKDROP_TYPE.
            if (!IsWindows11OrGreater())
                return false;

            int backdrop = useAcrylic
                ? (int)DwmSystemBackdropType.DWMSBT_TRANSIENTWINDOW // Acrylic
                : (int)DwmSystemBackdropType.DWMSBT_MAINWINDOW;     // Mica

            int hr = DwmSetWindowAttribute(
                hwnd,
                DWMWA_SYSTEMBACKDROP_TYPE,
                ref backdrop,
                sizeof(int));

            return hr == 0; // S_OK
        }

        /// <summary>
        /// Applies Windows 10 Acrylic blur using SetWindowCompositionAttribute.
        /// </summary>
        private static void ApplyWindows10Blur(IntPtr hwnd, int gradientColor)
        {
            var accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                AccentFlags = 2, // Draw all borders
                GradientColor = gradientColor,
                AnimationId = 0
            };

            var data = new WindowCompositionAttributeData
            {
                Attribute = WCA_ACCENT_POLICY,
                Data = Marshal.AllocHGlobal(Marshal.SizeOf(accent)),
                SizeOfData = Marshal.SizeOf(accent)
            };

            Marshal.StructureToPtr(accent, data.Data, false);
            SetWindowCompositionAttribute(hwnd, ref data);
            Marshal.FreeHGlobal(data.Data);
        }

        /// <summary>
        /// Checks whether the current OS is Windows 11 (build 22621) or later.
        /// </summary>
        private static bool IsWindows11OrGreater()
        {
            var version = Environment.OSVersion.Version;
            // Windows 11 reports major version 10, but build >= 22000.
            return version.Major >= 10 && version.Build >= 22000;
        }
    }
}