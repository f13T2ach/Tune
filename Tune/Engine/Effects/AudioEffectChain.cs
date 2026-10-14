using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.Effects;
using NAudio.Wave;
using Tune.Helpers.FormatConverters;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// Default effect chain of the player.
    /// The audio flows through the fixed equalizer, then the rack the user builds,
    /// then the level fader.
    /// </summary>
    public partial class AudioEffectChain : ObservableObject, IAudioEffectChain
    {
        private readonly VolumeFader _fader;

        private EffectChainSampleProvider? _processor;
        private WaveFormat? _format;

        [ObservableProperty]
        private bool _isEnabled = true;

        [ObservableProperty]
        private EffectSlot? _selectedSlot;

        [ObservableProperty]
        private int _selectedEffectIndex;

        /// <summary>
        /// Initializes the chain and restores every parameter from the store.
        /// </summary>
        /// <param name="store">The store the parameters are read from and written to.</param>
        public AudioEffectChain(IAudioEffectParameterStore store)
        {
            Volume = new AudioEffectParameter(store, "PlayerVolume", "VOL", 0, 100, 50);
            Equalizer = new EqualizerUnit(store);
            TenBand = new TenBandEqualizerUnit();
            Slots = new ObservableCollection<EffectSlot>();
            AvailableEffects = EffectCatalog.Entries.Select(entry => entry.Name).ToList();

            _fader = new VolumeFader(Volume);
            AddEffectCommand = new RelayCommand(AddEffect);
        }

        /// <inheritdoc />
        public IAudioEffectParameter Volume { get; }

        /// <inheritdoc />
        public EqualizerUnit Equalizer { get; }

        /// <inheritdoc />
        public TenBandEqualizerUnit TenBand { get; }

        /// <inheritdoc />
        public ObservableCollection<EffectSlot> Slots { get; }

        /// <inheritdoc />
        public IReadOnlyList<string> AvailableEffects { get; }

        /// <inheritdoc />
        public IRelayCommand AddEffectCommand { get; }

        /// <inheritdoc />
        public ISampleProvider CreateProcessor(ISampleProvider source)
        {
            _format = source.WaveFormat;

            Equalizer.Configure(_format);
            TenBand.Configure(_format);
            _fader.Configure(_format);

            foreach (EffectSlot slot in Slots)
                slot.Configure(_format);

            _processor = new EffectChainSampleProvider(source, BuildEffects());

            return _processor;
        }

        /// <inheritdoc />
        public void Reset()
        {
            Equalizer.Reset();
            TenBand.Reset();

            foreach (EffectSlot slot in Slots)
            {
                if (slot.IsSupported && slot.Effect is AudioEffect effect)
                    effect.Reset();
            }

            _fader.Gain.Reset();
        }

        /// <summary>
        /// The master switch drives the equalizer and suppresses every node of the
        /// rack, without dropping the user's own bypass choice for a node.
        /// </summary>
        partial void OnIsEnabledChanged(bool value)
        {
            ApplyBypass();
        }

        private void AddEffect()
        {
            if (SelectedEffectIndex < 0 || SelectedEffectIndex >= EffectCatalog.Entries.Count)
                return;

            EffectCatalog.Entry entry = EffectCatalog.Entries[SelectedEffectIndex];
            var slot = new EffectSlot(entry.Name, entry.Create(), Remove, MoveUp, MoveDown);

            if (_format != null)
                slot.Configure(_format);

            Slots.Add(slot);
            SelectedSlot = slot;

            ApplyBypass();
            PublishEffects();
        }

        private void Remove(EffectSlot slot)
        {
            if (!Slots.Remove(slot))
                return;

            if (ReferenceEquals(SelectedSlot, slot))
                SelectedSlot = Slots.FirstOrDefault();

            PublishEffects();
        }

        private void MoveUp(EffectSlot slot)
        {
            int index = Slots.IndexOf(slot);
            if (index <= 0)
                return;

            Slots.Move(index, index - 1);
            PublishEffects();
        }

        private void MoveDown(EffectSlot slot)
        {
            int index = Slots.IndexOf(slot);
            if (index < 0 || index >= Slots.Count - 1)
                return;

            Slots.Move(index, index + 1);
            PublishEffects();
        }

        /// <summary>
        /// Flattens the chain into the order the audio flows through it.
        /// The equalizer comes first, the level fader always comes last.
        /// </summary>
        private AudioEffect[] BuildEffects()
        {
            var effects = new List<AudioEffect>();

            if (Equalizer.IsSupported)
                effects.AddRange(Equalizer.Effects);

            if (TenBand.IsSupported)
                effects.AddRange(TenBand.Effects);

            foreach (EffectSlot slot in Slots)
            {
                if (slot.IsSupported && slot.Effect is AudioEffect effect)
                    effects.Add(effect);
            }

            effects.Add(_fader.Gain);

            return effects.ToArray();
        }

        /// <summary>
        /// Hands the rebuilt effect list to the running processor, so a rack edit is
        /// heard on the next audio block instead of the next file.
        /// </summary>
        private void PublishEffects()
        {
            _processor?.SetEffects(BuildEffects());
        }

        private void ApplyBypass()
        {
            Equalizer.IsEnabled = IsEnabled;
            TenBand.IsEnabled = IsEnabled;

            foreach (EffectSlot slot in Slots)
                slot.ApplyBypass(suppressed: !IsEnabled);
        }
    }

    /// <summary>
    /// The output level of the chain.
    /// It is deliberately not an effect: it always runs last and is never bypassed,
    /// so switching the effects off keeps the volume control working.
    /// </summary>
    internal sealed class VolumeFader
    {
        private readonly IAudioEffectParameter _volume;

        /// <summary>
        /// Initializes the fader and applies the stored volume.
        /// </summary>
        /// <param name="volume">The volume parameter on the 0..100 UI scale.</param>
        public VolumeFader(IAudioEffectParameter volume)
        {
            _volume = volume;
            Gain = new GainEffect();

            volume.PropertyChanged += OnVolumeChanged;

            Apply();
        }

        /// <summary>
        /// Gets the effect that applies the level.
        /// </summary>
        public GainEffect Gain { get; }

        /// <summary>
        /// Tells the fader which format the audio has.
        /// The framework refuses to process an effect that was never configured.
        /// </summary>
        /// <param name="format">The format of the loaded file.</param>
        public void Configure(WaveFormat format)
        {
            Gain.Configure(format);
        }

        private void OnVolumeChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAudioEffectParameter.Value))
                Apply();
        }

        private void Apply()
        {
            Gain.LinearGain = (float)UIVolumeToDoubleVolume.Convert(_volume.Value);
        }
    }

    /// <summary>
    /// Sample source that pushes every read block through the effects of the chain,
    /// in order. One instance belongs to one loaded file.
    /// </summary>
    internal sealed class EffectChainSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private volatile AudioEffect[] _effects;

        /// <summary>
        /// Initializes the node for one loaded file.
        /// </summary>
        /// <param name="source">The sample source to read from.</param>
        /// <param name="effects">The effects to apply, already in processing order.</param>
        public EffectChainSampleProvider(ISampleProvider source, AudioEffect[] effects)
        {
            _source = source;
            _effects = effects;
        }

        /// <inheritdoc />
        public WaveFormat WaveFormat => _source.WaveFormat;

        /// <summary>
        /// Swaps the effect list in one assignment, so the reader always sees a
        /// complete chain and the rack can be edited while audio plays.
        /// </summary>
        /// <param name="effects">The effects to apply, in processing order.</param>
        public void SetEffects(AudioEffect[] effects)
        {
            _effects = effects;
        }

        /// <inheritdoc />
        public int Read(Span<float> buffer)
        {
            int read = _source.Read(buffer);

            if (read <= 0)
                return read;

            Span<float> block = buffer[..read];
            AudioEffect[] effects = _effects;

            // Indexed loop on purpose: this runs on the audio thread and must not allocate.
            for (int i = 0; i < effects.Length; i++)
                effects[i].Process(block);

            return read;
        }
    }
}
