using System;
using Jobbr.Server.ForkedExecution.Execution;

namespace Jobbr.Server.ForkedExecution.Tests.Infrastructure
{
    public class PeriodicTimerMock : IPeriodicTimer
    {
        private Action _callback;

        public void Setup(Action value, long intervalInSeconds)
        {
            _callback = value;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void CallbackOnce()
        {
            _callback();
        }
    }
}