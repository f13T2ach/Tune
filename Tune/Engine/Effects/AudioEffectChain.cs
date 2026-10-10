using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.Effects;
using NAudio.Wave;

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
            _fader.Configure(_format);

            foreach (EffectSlot slot in Slots)
                slot.Configure(_format);

            _processor = new EffectChainSampleProvider(source, BuildEffects());

            return _processor;
        }

        /// <inheritdoc />
        public void Reset()
        {
            if (Equalizer.IsSupported)
            {
                foreach (AudioEffect effect in Equalizer.Effects)
                    effect.Reset();
            }

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

            foreach (EffectSlot slot in Slots)
                slot.ApplyBypass(suppressed: !IsEnabled);
        }
    }
}
