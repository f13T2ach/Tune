using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;


namespace Tune.Services.Clients
{
    public class AudioDeviceNotificationClient : IMMNotificationClient
    {
        // 自定义事件，用于通知外部
        public event EventHandler<string>? DefaultDeviceChanged;
        public event EventHandler<(string DeviceId, DeviceState NewState)>? DeviceStateChanged;

        // 必须实现的接口方法
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            DefaultDeviceChanged?.Invoke(this, defaultDeviceId);
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            DeviceStateChanged?.Invoke(this, (deviceId, newState));
        }

        // 以下为空实现
        public void OnDeviceAdded(string pwstrDeviceId) { }
        public void OnDeviceRemoved(string deviceId) { }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
    }
}
