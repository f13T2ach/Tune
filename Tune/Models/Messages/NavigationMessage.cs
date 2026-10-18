namespace Tune.Models.Messages
{
    /// <summary>
    /// The pages the shell can show. Views are resolved by the shell, so a view model can ask to
    /// go somewhere without knowing which type draws it.
    /// </summary>
    public enum PageKind
    {
        Main,
        Playlists,
        CurrentPlaylist,
        Settings,
        SoundEffect
    }

    /// <summary>A request to show one of the pages.</summary>
    public class NavigationMessage
    {
        public NavigationMessage(PageKind kind)
        {
            Kind = kind;
        }

        /// <summary>Gets the page to show.</summary>
        public PageKind Kind { get; }
    }
}