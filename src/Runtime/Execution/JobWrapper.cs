using System;
using System.Security.Principal;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Jobbr.Runtime.Execution
{
    internal class JobWrapper
    {
        private readonly ILogger<JobWrapper> _logger;

        private readonly Thread _thread;

        internal JobWrapper(ILoggerFactory loggerFactory, Action action, UserContext runtimeContext)
        {
            _logger = loggerFactory.CreateLogger<JobWrapper>();
            
            _thread = new Thread(() =>
            {
                var previousPrincipal = Thread.CurrentPrincipal;

                try
                {
                    if (!string.IsNullOrWhiteSpace(runtimeContext.UserId))
                    {
                        Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(runtimeContext.UserId, "JobbrIdentity"), Array.Empty<string>());
                    }

                    action();
                }
                catch (Exception e)
                {
                    Exception = e;
                }
                finally
                {
                    Thread.CurrentPrincipal = previousPrincipal;
                }
            });
        }

        internal void Start()
        {
            _thread.Start();
        }

        public Exception Exception { get; private set; }

        internal bool WaitForCompletion()
        {
            try
            {
                _thread.Join();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while waiting for completion of job");
                Exception = e;
                return false;
            }

            if (Exception != null)
            {
                _logger.LogError(Exception, "The execution of the job has faulted. See Exception for details.");
                return false;
            }

            return true;
        }
    }
}