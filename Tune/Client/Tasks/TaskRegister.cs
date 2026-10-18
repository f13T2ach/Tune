using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;

namespace Tune.Client.Tasks
{
    /// <summary>Something that has to run once per rendered frame.</summary>
    public interface IFrameTask
    {
        /// <summary>Does one step for the time that passed since the previous frame.</summary>
        /// <param name="elapsed">Time since the previous frame.</param>
        void Tick(TimeSpan elapsed);
    }

    /// <summary>
    /// Runs frame tasks from one place. A component attaches itself here instead of owning a timer,
    /// so it keeps running while its panel is not on screen and WPF still has exactly one subscription
    /// to the render frame.
    /// </summary>
    public sealed class TaskRegister
    {
        private readonly List<IFrameTask> _tasks = new();
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private TimeSpan _last;
        private bool _running;

        /// <summary>Starts calling the task once per frame.</summary>
        /// <param name="task">The task to run.</param>
        public void Attach(IFrameTask task)
        {
            if (task == null || _tasks.Contains(task))
                return;

            _tasks.Add(task);

            if (!_running)
            {
                _last = _clock.Elapsed;
                CompositionTarget.Rendering += OnRendering;
                _running = true;
            }
        }

        /// <summary>Stops calling the task once per frame.</summary>
        /// <param name="task">The task to stop.</param>
        public void Deattach(IFrameTask task)
        {
            if (task == null || !_tasks.Remove(task))
                return;

            if (_tasks.Count == 0 && _running)
            {
                CompositionTarget.Rendering -= OnRendering;
                _running = false;
            }
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            TimeSpan now = _clock.Elapsed;
            TimeSpan elapsed = now - _last;
            _last = now;

            // A snapshot, because a task may attach or detach while it is being ticked.
            IFrameTask[] tasks = _tasks.ToArray();

            foreach (IFrameTask task in tasks)
            {
                try
                {
                    task.Tick(elapsed);
                }
                catch
                {
                    // One failing task must not take the frame, or the other tasks, down with it.
                }
            }
        }
    }
}