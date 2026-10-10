using CommunityToolkit.Mvvm.ComponentModel;
using NAudio.Effects;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// One parameter of a rack effect, bound by the generic editor.
    /// The editor shows one control per kind and the Show flags pick which one,
    /// exactly like the NAudio demo this rack follows.
    /// </summary>
    public sealed class EffectParameterSlot : ObservableObject
    {
        private readonly EffectParameter _parameter;

        /// <summary>
        /// Initializes the editor for one effect parameter.
        /// </summary>
        /// <param name="parameter">The parameter exposed by the effect.</param>
        public EffectParameterSlot(EffectParameter parameter)
        {
            _parameter = parameter;
        }

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string Name => _parameter.Name;

        /// <summary>
        /// Gets the unit shown after the value.
        /// </summary>
        public string Unit => _parameter.Unit;

        /// <summary>
        /// Gets the lowest accepted value.
        /// </summary>
        public double Minimum => _parameter.Minimum;

        /// <summary>
        /// Gets the highest accepted value.
        /// </summary>
        public double Maximum => _parameter.Maximum;

        /// <summary>
        /// Gets the arrow key step. A slider default of 0.1 is meaningless across
        /// data driven ranges, so it is scaled to one percent of the range.
        /// </summary>
        public double SmallChange => (Maximum - Minimum) / 100.0;

        /// <summary>
        /// Gets the page step, scaled to ten percent of the range.
        /// </summary>
        public double LargeChange => (Maximum - Minimum) / 10.0;

        /// <summary>
        /// Gets a value indicating whether the parameter is a continuous value.
        /// </summary>
        public bool ShowSlider => _parameter.Kind == EffectParameterKind.Continuous;

        /// <summary>
        /// Gets a value indicating whether the parameter is on or off.
        /// </summary>
        public bool ShowToggle => _parameter.Kind == EffectParameterKind.Toggle;

        /// <summary>
        /// Gets a value indicating whether the parameter picks from a list.
        /// </summary>
        public bool ShowChoice => _parameter.Kind == EffectParameterKind.Choice;

        /// <summary>
        /// Gets a value indicating whether the parameter is a read only meter.
        /// </summary>
        public bool ShowMeter => _parameter.Kind == EffectParameterKind.Meter;

        /// <summary>
        /// Gets the choices of a list parameter.
        /// </summary>
        public IReadOnlyList<string> Choices => _parameter.Choices;

        /// <summary>
        /// Gets or sets the value of a continuous or meter parameter.
        /// </summary>
        public double Value
        {
            get => _parameter.Value;
            set
            {
                _parameter.Value = (float)value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayValue));
            }
        }

        /// <summary>
        /// Gets or sets the value of an on/off parameter.
        /// </summary>
        public bool BoolValue
        {
            get => _parameter.Value >= 0.5f;
            set
            {
                _parameter.Value = value ? 1f : 0f;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayValue));
            }
        }

        /// <summary>
        /// Gets or sets the selected index of a list parameter.
        /// </summary>
        public int ChoiceIndex
        {
            get => (int)_parameter.Value;
            set
            {
                _parameter.Value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayValue));
            }
        }

        /// <summary>
        /// Gets the value as it is shown next to the editor.
        /// </summary>
        public string DisplayValue => $"{_parameter.Value:0.##} {Unit}".Trim();

        /// <summary>
        /// Pushes a change made outside the editor, such as a meter, to the UI.
        /// </summary>
        public void Refresh()
        {
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(BoolValue));
            OnPropertyChanged(nameof(ChoiceIndex));
            OnPropertyChanged(nameof(DisplayValue));
        }
    }
}
