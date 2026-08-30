using NAudio.CoreAudioApi;
using Tune.Helpers;
using Tune.Services.Interfaces;
using Tune.Values.Static;

// To Update to NAudio 3.0, we used AI to update our code.
namespace Tune.Services
{
    public class AudioDeviceService : IAudioDeviceService
    {
        private readonly MMDeviceEnumerator _enumerator;
        private MMDeviceNotificationClient? _notificationClient; // 改为使用官方类型

        private string _deviceName = Strings.UNKNOWN;

        public string DeviceName => _deviceName;

        public event EventHandler? DeviceChanged;

        public AudioDeviceService()
        {
            _enumerator = new MMDeviceEnumerator();

            // 1. 创建通知客户端
            _notificationClient = _enumerator.CreateNotificationClient();

            // 2. 订阅事件（注意新的事件参数类型）
            _notificationClient.DefaultDeviceChanged += OnDefaultDeviceChanged;
            _notificationClient.DeviceStateChanged += OnDeviceStateChanged;

            // 不再需要 RegisterEndpointNotificationCallback

            UpdateDefaultDeviceName();
        }

        private void UpdateDefaultDeviceName()
        {
            try
            {
                using var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                _deviceName = device?.FriendlyName ?? "No Default Device";
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError("Unable to get device name", ex);
                _deviceName = Strings.NO_CONTENT;
            }

            DeviceChanged?.Invoke(this, EventArgs.Empty);
        }

        // 3. 更新事件处理方法签名
        private void OnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
        {
            // e.DeviceId, e.Flow, e.Role 可用
            UpdateDefaultDeviceName();
        }

        private void OnDeviceStateChanged(object? sender, DeviceStateChangedEventArgs e)
        {
            // e.DeviceId, e.DeviceState
            UpdateDefaultDeviceName();
        }

        public void Dispose()
        {
            if (_notificationClient != null)
            {
                // 4. 取消订阅
                _notificationClient.DefaultDeviceChanged -= OnDefaultDeviceChanged;
                _notificationClient.DeviceStateChanged -= OnDeviceStateChanged;

                // 如果客户端实现了 IDisposable（通常是的），释放它
                (_notificationClient as IDisposable)?.Dispose();
                _notificationClient = null;
            }

            _enumerator?.Dispose();
        }
    }
}