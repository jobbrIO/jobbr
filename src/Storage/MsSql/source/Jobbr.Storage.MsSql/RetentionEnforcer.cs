using System;
using System.Threading;

namespace Jobbr.Storage.MsSql
{
    public class RetentionEnforcer
    {
        private readonly Timer _timer;

        public RetentionEnforcer(MsSqlStorageProvider storage, TimeSpan retention, TimeSpan interval)
        {
            _timer = new Timer(state =>
            {
                var deadline = DateTimeOffset.UtcNow.Subtract(retention);

                storage.ApplyRetention(deadline);
            });

            _timer.Change(TimeSpan.Zero, interval);
        }
    }
}
