using NAudio.CoreAudioApi;
using Tune.Helpers;
using Tune.Services.Clients;
using Tune.Services.Interfaces;
using Tune.Values.Static;

namespace Tune.Services
{
    public class AudioDeviceService : IAudioDeviceService
    {
        private readonly MMDeviceEnumerator _enumerator;
        private readonly AudioDeviceNotificationClient _notificationClient;

        private string _deviceName = "Unknown";


        public string DeviceName => _deviceName;


        public event EventHandler? DeviceChanged;


        public AudioDeviceService()
        {
            _enumerator = new MMDeviceEnumerator();

            _notificationClient = new AudioDeviceNotificationClient();

            _notificationClient.DefaultDeviceChanged
                += OnDefaultDeviceChanged;

            _notificationClient.DeviceStateChanged
                += OnDeviceStateChanged;

            _enumerator.RegisterEndpointNotificationCallback(
                _notificationClient);


            UpdateDefaultDeviceName();
        }

        private void UpdateDefaultDeviceName()
        {
            try
            {
                using var device =
                    _enumerator.GetDefaultAudioEndpoint(
                        DataFlow.Render,
                        Role.Multimedia);
                _deviceName =
                    device?.FriendlyName
                    ?? "No Default Device";
            }
            catch(Exception ex)
            {
                    LoggerHelper.LogError("Unable to get device name", ex);
                    _deviceName = Strings.NO_CONTENT;
            }

            DeviceChanged?.Invoke(this, EventArgs.Empty);
        }


        private void OnDefaultDeviceChanged(
            object? sender,
            string deviceId)
        {
            UpdateDefaultDeviceName();
        }
        private void OnDeviceStateChanged(
            object? sender,
            (string DeviceId, DeviceState NewState) e)
        {
            UpdateDefaultDeviceName();
        }

        public void Dispose()
        {
            _notificationClient.DefaultDeviceChanged
                -= OnDefaultDeviceChanged;

            _notificationClient.DeviceStateChanged
                -= OnDeviceStateChanged;


            _enumerator.UnregisterEndpointNotificationCallback(
                _notificationClient);


            _enumerator.Dispose();
        }
    }
}