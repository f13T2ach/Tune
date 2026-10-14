using CommunityToolkit.Mvvm.ComponentModel;
using Tune.Assets.Localization;
using Tune.Helpers.UiState;

namespace Tune.Engine.Effects
{
    /// <summary>Which equalizer the player is running.</summary>
    public enum EqualizerSchemeKind
    {
        /// <summary>No equalizer is in use.</summary>
        None,

        /// <summary>The ten band equalizer.</summary>
        TenBand,

        /// <summary>The graphic equalizer.</summary>
        Graphic
    }

    /// <summary>
    /// Tells the rest of the application which equalizer is in use and what its scheme is
    /// called. The main page shows this and never talks to an equalizer page of its own, so
    /// the graphic equalizer can take over later without the main page changing. The two
    /// kinds are alternatives: whichever one is set here is the one the player is running.
    /// </summary>
    public sealed class EqualizerScheme : ObservableObject
    {
        /// <summary>Interface state key the name of the active scheme is remembered under.</summary>
        public const string SchemeKey = "equalizer.scheme";

        /// <summary>Interface state key the kind of the active scheme is remembered under.</summary>
        public const string KindKey = "equalizer.kind";

        /// <summary>Stored name of the ten band equalizer.</summary>
        public const string TenBandKind = "tenband";

        /// <summary>Stored name of the graphic equalizer.</summary>
        public const string GraphicKind = "graphic";

        private EqualizerSchemeKind _kind = EqualizerSchemeKind.None;
        private string _name = string.Empty;

        /// <summary>Gets the scheme of the running player; one player, one scheme.</summary>
        public static EqualizerScheme Current { get; } = new EqualizerScheme();

        /// <summary>Gets which equalizer is in use.</summary>
        public EqualizerSchemeKind Kind => _kind;

        /// <summary>Gets the name of the scheme, empty while it has none.</summary>
        public string Name => _name;

        /// <summary>
        /// Gets what the main page shows: the kind of equalizer with the name of its
        /// scheme, or the plain "no preset" answer when there is none yet.
        /// </summary>
        public string DisplayText => _kind switch
        {
            EqualizerSchemeKind.TenBand when _name.Length > 0 => LangStrings.SchemeTenBand + _name,
            EqualizerSchemeKind.Graphic when _name.Length > 0 => LangStrings.SchemeGraphic + _name,
            _ => LangStrings.PresetNone
        };

        /// <summary>
        /// Remembers the scheme in use, tells whoever shows it, and writes the choice down
        /// so the player comes back on the same one.
        /// </summary>
        /// <param name="kind">The equalizer that is in use.</param>
        /// <param name="name">Name of its scheme, empty while it has none.</param>
        public void Set(EqualizerSchemeKind kind, string? name)
        {
            _kind = kind;
            _name = name ?? string.Empty;

            UiStateStore.Set(KindKey, StoredKind(kind));
            UiStateStore.Set(SchemeKey, _name);

            OnPropertyChanged(nameof(Kind));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayText));
        }

        /// <summary>Goes back to no scheme at all.</summary>
        public void Clear() => Set(EqualizerSchemeKind.None, null);

        /// <summary>Reads back which kind an earlier run left the player on.</summary>
        /// <returns>The stored kind, or none when nothing was stored.</returns>
        public static EqualizerSchemeKind StoredKind()
        {
            string stored = UiStateStore.Get(KindKey);

            if (stored == TenBandKind) return EqualizerSchemeKind.TenBand;
            if (stored == GraphicKind) return EqualizerSchemeKind.Graphic;

            return EqualizerSchemeKind.None;
        }

        private static string StoredKind(EqualizerSchemeKind kind) => kind switch
        {
            EqualizerSchemeKind.TenBand => TenBandKind,
            EqualizerSchemeKind.Graphic => GraphicKind,
            _ => string.Empty
        };
    }
}
