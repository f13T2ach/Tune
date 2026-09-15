using System;
using System.Collections.Generic;
using System.Text;
using Tune.Assets.Localization;
using Tune.Models.Playlist;

namespace Tune.Models.DisplayItems
{
    public class PlaylistDisplayItem
    {
        // Singleton instance representing the special "None" entry.
        public static readonly PlaylistDisplayItem None = new PlaylistDisplayItem((PlaylistBase?)null);

        public PlaylistBase? Playlist { get; }

        public bool IsNone => Playlist == null;

        public PlaylistDisplayItem(PlaylistBase? playlist) => Playlist = playlist;

        // Display text; "None" is shown as a literal string when Playlist is null.
        public override string ToString() => Playlist?.Name ?? LangStrings.None;
    }
}
