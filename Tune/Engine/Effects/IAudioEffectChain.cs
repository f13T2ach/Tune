using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.Wave;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// The sound effect processor of the player.
    /// It owns the fixed equalizer, the output level and a rack of effects the user
    /// stacks up. The equalizer is deliberately not part of the rack, so the rack can
    /// never reshape the knobs the main page binds to.
    /// </summary>
    public interface IAudioEffectChain : INotifyPropertyChanged
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
        /// Clears the state of every effect. Called when playback jumps, so a filter
        /// never carries samples across a seek.
        /// </summary>
        void Reset();
    }
}
