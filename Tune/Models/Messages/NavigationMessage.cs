namespace Tune.Models.Messages
{
    public class NavigationMessage
    {
        public Type PageType { get; }

        public NavigationMessage(Type pageType)
        {
            PageType = pageType;
        }
    }
}
