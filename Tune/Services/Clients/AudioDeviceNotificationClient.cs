using NAudio.CoreAudioApi;

// To Update to NAudio 3.0, we used AI to update our code.
namespace Tune.Services.Clients
{
    public class AudioDeviceManager
    {
        private MMDeviceEnumerator? _deviceEnumerator;
        private MMDeviceNotificationClient? _notificationClient;

        public AudioDeviceManager()
        {
            _deviceEnumerator = new MMDeviceEnumerator();

            // 1. 创建通知客户端
            _notificationClient = _deviceEnumerator.CreateNotificationClient();

            // 2. 订阅事件
            _notificationClient.DefaultDeviceChanged += OnDefaultDeviceChanged;
            _notificationClient.DeviceStateChanged += OnDeviceStateChanged;
            // 其他事件也可以订阅，例如：
            // _notificationClient.DeviceAdded += OnDeviceAdded;
            // _notificationClient.DeviceRemoved += OnDeviceRemoved;
        }

        private void OnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
        {
            // e.DeviceId 和 e.Flow / e.Role 提供了详细信息
            Console.WriteLine($"默认设备变更: {e.DeviceId}");
        }

        private void OnDeviceStateChanged(object? sender, DeviceStateChangedEventArgs e)
        {
            // e.DeviceId 和 e.DeviceState
            Console.WriteLine($"设备 '{e.DeviceId}' 状态变更: {e.NewState}");
        }

        // 记得在合适的时候取消订阅和释放资源
        public void Dispose()
        {
            if (_notificationClient != null)
            {
                _notificationClient.DefaultDeviceChanged -= OnDefaultDeviceChanged;
                _notificationClient.DeviceStateChanged -= OnDeviceStateChanged;
                // 如果客户端实现了 IDisposable，应调用 Dispose
                // _notificationClient.Dispose();
            }
            _deviceEnumerator?.Dispose();
        }
    }
}
