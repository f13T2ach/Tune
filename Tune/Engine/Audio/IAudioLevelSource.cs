using System;

namespace Tune.Engine.Audio
{
    /// <summary>
    /// Where the meter's numbers come from. Everything above this interface only ever sees numbers,
    /// which keeps each control from reaching into the player.
    /// </summary>
    public interface IAudioLevelSource
    {
        /// <summary>
        /// Raised on the audio thread after every block that was read, and once more with a closed
        /// window when a sampling window runs out.
        /// </summary>
        event EventHandler<AudioLevelEventArgs>? LevelReported;
    }
}
