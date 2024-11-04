using System;
using System.Threading;

namespace Jobbr.Server.ForkedExecution.Execution
{
    /// <summary>
    /// Fixed interval timer.
    /// </summary>
    internal class FixedIntervalTimer : IPeriodicTimer, IDisposable
    {
        private Action _callback;

        private Timer _timer;
        private TimeSpan _interval;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedIntervalTimer"/> class.
        /// </summary>
        public FixedIntervalTimer()
        {
            _timer = new Timer(state => _callback());
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FixedIntervalTimer"/> class.
        /// </summary>
        ~FixedIntervalTimer()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Setup(Action value, long intervalInSeconds)
        {
            _callback = value;
            _interval = TimeSpan.FromSeconds(intervalInSeconds);
        }

        /// <inheritdoc/>
        public void Start()
        {
            _timer.Change(_interval, _interval);
        }

        /// <inheritdoc/>
        public void Stop()
        {
            _timer.Change(int.MaxValue, int.MaxValue);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Conditional dispose.
        /// </summary>
        /// <param name="disposing">Condition for disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }
    }
}