using CommunityToolkit.Mvvm.ComponentModel;
using NAudio.Effects;
using NAudio.Wave;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// Base class for an effect unit.
    /// It collects the parameters and the audio effects the unit owns, forwards the
    /// enable switch to the ramped bypass of those effects, and keeps a unit that
    /// cannot work on the loaded format bypassed instead of letting it fail.
    /// </summary>
    public abstract class AudioEffectUnit : ObservableObject, IAudioEffectUnit
    {
        private readonly List<IAudioEffectParameter> _parameters = new List<IAudioEffectParameter>();
        private readonly List<AudioEffect> _effects = new List<AudioEffect>();
        private bool _isEnabled = true;
        private bool _isSupported = true;

        /// <inheritdoc />
        public abstract string Id { get; }

        /// <inheritdoc />
        public abstract string DisplayName { get; }

        /// <inheritdoc />
        public IReadOnlyList<IAudioEffectParameter> Parameters => _parameters;

        /// <inheritdoc />
        public IReadOnlyList<AudioEffect> Effects => _effects;

        /// <inheritdoc />
        public bool IsSupported => _isSupported;

        /// <inheritdoc />
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (SetProperty(ref _isEnabled, value))
                    ApplyBypass();
            }
        }

        /// <inheritdoc />
        public void Configure(WaveFormat format)
        {
            _isSupported = SupportsFormat(format);

            if (_isSupported)
            {
                foreach (AudioEffect effect in _effects)
                    effect.Configure(format);

                OnConfigured();
            }

            ApplyBypass();
        }

        /// <summary>
        /// Tells whether this unit can process the given format.
        /// </summary>
        /// <param name="format">The format of the loaded file.</param>
        /// <returns><c>true</c> when the unit should process the audio.</returns>
        protected virtual bool SupportsFormat(WaveFormat format)
        {
            return true;
        }

        /// <summary>
        /// Runs after the audio format is known, for units that must rebuild filters.
        /// </summary>
        protected virtual void OnConfigured()
        {
        }

        /// <summary>
        /// Adds an effect to the unit, in processing order.
        /// </summary>
        /// <param name="effect">The effect to append.</param>
        protected void AddEffect(AudioEffect effect)
        {
            _effects.Add(effect);
        }

        /// <summary>
        /// Registers a parameter as owned by this unit.
        /// </summary>
        /// <param name="parameter">The parameter owned by the unit.</param>
        protected void AddParameter(IAudioEffectParameter parameter)
        {
            _parameters.Add(parameter);
        }

        private void ApplyBypass()
        {
            foreach (AudioEffect effect in _effects)
                effect.Bypass = !_isEnabled;
        }
    }
}
