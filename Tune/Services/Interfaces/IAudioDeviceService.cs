namespace Tune.Services.Interfaces
{
    public interface IAudioDeviceService : IDisposable
    {
        string DeviceName { get; }

        event EventHandler? DeviceChanged;
    }
}