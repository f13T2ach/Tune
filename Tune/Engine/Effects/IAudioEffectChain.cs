using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.Wave;
using Tune.Engine.Audio;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// The sound effect processor of the player.
    /// It owns the fixed equalizer, the output level and a rack of effects the user
    /// stacks up. The equalizer is deliberately not part of the rack, so the rack can
    /// never reshape the knobs the main page binds to. It also reports the output level,
    /// so the meters have one place to read from.
    /// </summary>
    public interface IAudioEffectChain : INotifyPropertyChanged, IAudioLevelSource
    {
        /// <summary>
        /// Gets the output level of the player, on the 0..100 UI scale.
        /// </summary>
        IAudioEffectParameter Volume { get; }

        /// <summary>
        /// Gets the three band equalizer, a fixed processor of the chain.
        /// </summary>
        EqualizerUnit Equalizer { get; }

        /// <summary>
        /// Gets the ten band graphic equalizer, which the equalizer page drives from
        /// its presets. It is a fixed processor too, and stays flat until a preset is
        /// loaded, so it colours nothing on its own.
        /// </summary>
        TenBandEqualizerUnit TenBand { get; }

        /// <summary>
        /// Gets the rack, in the order the audio flows through it.
        /// </summary>
        ObservableCollection<EffectSlot> Slots { get; }

        /// <summary>
        /// Gets or sets the node the parameter area shows.
        /// </summary>
        EffectSlot? SelectedSlot { get; set; }

        /// <summary>
        /// Gets the labels of every effect that can be added to the rack.
        /// </summary>
        IReadOnlyList<string> AvailableEffects { get; }

        /// <summary>
        /// Gets or sets the index of the catalog entry the add command uses.
        /// </summary>
        int SelectedEffectIndex { get; set; }

        /// <summary>
        /// Gets the command that appends the selected catalog entry to the rack.
        /// </summary>
        IRelayCommand AddEffectCommand { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the effects are applied.
        /// The level is applied either way.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Wraps a sample source so that reading from it runs the whole chain.
        /// A new processor is created for every loaded file, and later edits to the
        /// rack are published to it so they are heard while audio plays.
        /// </summary>
        /// <param name="source">The sample source to process.</param>
        /// <returns>The processing node, which carries the source wave format.</returns>
        ISampleProvider CreateProcessor(ISampleProvider source);

        /// <summary>
        /// Attaches an effect at the end of the user layer. This is the only way outside code may
        /// extend the chain: the order of the built-in nodes, and everything the system layer does
        /// afterwards, stays out of reach.
        /// </summary>
        /// <param name="attach">Builds the effect on top of the given source.</param>
        void AttachEffect(Func<ISampleProvider, ISampleProvider> attach);

        /// <summary>Gets or sets the output ceiling in decibels; zero leaves the sound untouched.</summary>
        double OutputLimitDecibels { get; set; }

        /// <summary>Gets or sets a value indicating whether the output is limited.</summary>
        bool IsOutputLimited { get; set; }

        /// <summary>
        /// Clears the state of every effect. Called when playback jumps, so a filter
        /// never carries samples across a seek.
        /// </summary>
        void Reset();
    }
}
