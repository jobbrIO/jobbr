using System;
using Jobbr.Server.ForkedExecution.Execution;

namespace Jobbr.Server.ForkedExecution.Tests.Infrastructure
{
    public class ManualTimeProvider : IDateTimeProvider
    {
        private DateTime _currentTime;

        public ManualTimeProvider()
        {
            _currentTime = DateTime.UtcNow;
        }

        public DateTime GetUtcNow()
        {
            return _currentTime;
        }

        public void AddMinute()
        {
            _currentTime = _currentTime.AddMinutes(1);
        }

        public void AddSecond()
        {
            _currentTime = _currentTime.AddSeconds(1);
        }

        public void Set(DateTime dateTimeUtc)
        {
            _currentTime = dateTimeUtc;
        }
    }
}