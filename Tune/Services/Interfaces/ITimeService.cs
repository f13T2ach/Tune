namespace Tune.Services.Interfaces
{
    public interface ITimeService
    {
        DateTime CurrentTime { get; }

        event EventHandler? TimeChanged;
    }
}
