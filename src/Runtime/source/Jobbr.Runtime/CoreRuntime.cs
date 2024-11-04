using System;
using Jobbr.Runtime.Activation;
using Jobbr.Runtime.Execution;
using Microsoft.Extensions.Logging;

namespace Jobbr.Runtime
{
    public class CoreRuntime
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<CoreRuntime> _logger;

        private readonly JobActivator _jobActivator;

        /// <summary>
        /// Raised immediately after start and indicates that the Runtime is setting up itself
        /// </summary>
        public event EventHandler Initializing;

        /// <summary>
        /// Raised before the runtime activates the job class
        /// </summary>
        public event EventHandler Activating;

        /// <summary>
        /// Raised before the wrapper for the Run() Method is generated
        /// </summary>
        public event EventHandler WiringMethod;

        /// <summary>
        /// Raised before the Run()-Method is executed
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// Raised after the job has come to the end, independent of its success
        /// </summary>
        public event EventHandler<ExecutionEndedEventArgs> Ended;

        /// <summary>
        /// Raised for exceptions in the Core infrastructure that have not been handled
        /// </summary>
        public event EventHandler<InfrastructureExceptionEventArgs> InfrastructureException;

        public CoreRuntime(ILoggerFactory loggerFactory, RuntimeConfiguration runtimeConfiguration)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<CoreRuntime>();
                
            var jobTypeResolver = new JobTypeResolver(loggerFactory, runtimeConfiguration.JobTypeSearchAssemblies);
            var serviceProvider = runtimeConfiguration.ServiceProvider ?? new DefaultServiceProvider();

            _jobActivator = new JobActivator(loggerFactory, jobTypeResolver, serviceProvider);
        }

        public void Execute(ExecutionMetadata executionMetadata)
        {
            var wasSuccessful = false;
            Exception lastException = null;

            try
            {
                OnInitializing();

                var jobTypeName = executionMetadata.JobType;

                var userContext = new UserContext()
                {
                    UserId = executionMetadata.UserId,
                    UserDisplayName = executionMetadata.UserDisplayName
                };

                // Register userContext as RuntimeContext in the DI if available
                _logger.LogDebug("Trying to register additional dependencies if supported.");
                
                #pragma warning disable 618
                var runtimeContext = new RuntimeContext
                {
                    UserId = userContext.UserId,
                    UserDisplayName = userContext.UserDisplayName
                };
                #pragma warning restore 618

                _jobActivator.AddDependencies(runtimeContext);

                // Create instance
                _logger.LogDebug("Create instance of job based on the typename '{jobTypeName}'", jobTypeName);
                OnActivating();

                var jobClassInstance = _jobActivator.CreateInstance(jobTypeName);

                if (jobClassInstance == null)
                {
                    _logger.LogError("Cannot create activate the job based on the typename {jobTypeName}", jobTypeName);
                    return;
                }

                // Create task as wrapper for calling the Run() Method
                _logger.LogDebug("Create task as wrapper for calling the Run() Method");
                OnWiringMethod();

                var runWrapperFactory = new RunWrapperFactory(_loggerFactory, jobClassInstance.GetType(), executionMetadata.JobParameter, executionMetadata.InstanceParameter);
                var wrapper = runWrapperFactory.CreateWrapper(jobClassInstance, userContext);

                if (wrapper == null)
                {
                    _logger.LogError("Unable to create a wrapper for the job");
                    return;
                }

                // Start 
                _logger.LogDebug("Starting Task to execute the Run()-Method.");
                OnStarting();

                wrapper.Start();

                // Wait for completion
                wasSuccessful = wrapper.WaitForCompletion();
                lastException = wrapper.Exception;
            }
            catch (Exception e)
            {
                lastException = e;

                _logger.LogCritical(e, "Exception in the Jobbr-Runtime. Please see details: ");
                OnInfrastructureException(new InfrastructureExceptionEventArgs { Exception = e });
            }
            finally
            {
                OnEnded(new ExecutionEndedEventArgs() { Succeeded = wasSuccessful, Exception = lastException});
            }
        }

        #region Event Invocators

        protected virtual void OnInitializing()
        {
            try
            {
                Initializing?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Recipient of the event {event} threw an exception", nameof(OnInitializing));
            }
        }

        protected virtual void OnActivating()
        {
            try
            {
                Activating?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Recipient of the event {event} threw an exception", nameof(OnActivating));
            }
        }

        protected virtual void OnWiringMethod()
        {
            try
            {
                WiringMethod?.Invoke(this, EventArgs.Empty);
            }
            catch(Exception exception)
            {
                _logger.LogError(exception, "Recipient of the event {event} threw an exception", nameof(OnWiringMethod));
            }
        }

        protected virtual void OnStarting()
        {
            try
            {
                Starting?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Recipient of the event {event} threw an exception", nameof(OnStarting));
            }
        }

        protected virtual void OnEnded(ExecutionEndedEventArgs e)
        {
            try
            {
                Ended?.Invoke(this, e);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Recipient of the event {event} threw an exception", nameof(OnEnded));
            }
        }

        protected virtual void OnInfrastructureException(InfrastructureExceptionEventArgs e)
        {
            try
            {
                InfrastructureException?.Invoke(this, e);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Recipient of the event {event} threw an exception", nameof(OnInfrastructureException));
            }
        }

        #endregion
    }
}