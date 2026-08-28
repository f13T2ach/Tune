namespace Tune.Models.Results
{
    public class PlayerControllerLoadResult
    {
        public bool IsSucceeded { get; set; }
        public Exception? Ex { get; set; }
        public PlayerControllerLoadResult(bool isSuccessed, Exception? ex)
        {
            IsSucceeded = isSuccessed;
            Ex = ex;
        }

        public PlayerControllerLoadResult(bool isSuccessed)
        {
            IsSucceeded = isSuccessed;
        }
    }
}
