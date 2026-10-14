using System.ComponentModel;
using System.Windows;
using Tune.Components.TabFrame;
using Tune.Helpers.UiState;

namespace Tune.Helpers.UiState
{
    /// <summary>
    /// Remembers which tab a tab bar was on. Give a tab bar a key and it comes back on the
    /// tab the user left it on, without the tab bar itself having to know about storage.
    /// </summary>
    public static class TabFramePersistence
    {
        /// <summary>Identifies the tab bar; an empty key remembers nothing.</summary>
        public static readonly DependencyProperty PersistKeyProperty =
            DependencyProperty.RegisterAttached("PersistKey", typeof(string), typeof(TabFramePersistence),
                new PropertyMetadata(string.Empty, OnPersistKeyChanged));

        /// <summary>Sets the key a tab bar is remembered under.</summary>
        /// <param name="element">The tab bar.</param>
        /// <param name="value">The key, which has to be unique in the application.</param>
        public static void SetPersistKey(DependencyObject element, string value) =>
            element.SetValue(PersistKeyProperty, value);

        /// <summary>Reads the key a tab bar is remembered under.</summary>
        /// <param name="element">The tab bar.</param>
        public static string GetPersistKey(DependencyObject element) =>
            (string)element.GetValue(PersistKeyProperty);

        private static void OnPersistKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TabFrame tabs) return;
            if (e.NewValue is not string key || string.IsNullOrWhiteSpace(key)) return;

            string storageKey = $"tab.{key}";

            tabs.Loaded += (_, _) =>
            {
                if (int.TryParse(UiStateStore.Get(storageKey), out int index) &&
                    index >= 0 && index < tabs.Items.Count)
                {
                    tabs.SelectedIndex = index;
                }

                DependencyPropertyDescriptor
                    .FromProperty(TabFrame.SelectedIndexProperty, typeof(TabFrame))
                    .AddValueChanged(tabs, (_, _) => UiStateStore.Set(storageKey, tabs.SelectedIndex.ToString()));
            };
        }
    }
}
