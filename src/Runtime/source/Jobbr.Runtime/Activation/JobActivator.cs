using System;
using Microsoft.Extensions.Logging;

namespace Jobbr.Runtime.Activation
{
    internal class JobActivator
    {
        private readonly ILogger<JobActivator> _logger;

        private readonly JobTypeResolver _jobTypeResolver;
        private readonly IServiceProvider _serviceProvider;

        internal JobActivator(ILoggerFactory loggerFactory, JobTypeResolver jobTypeResolver, IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<JobActivator>();
            _jobTypeResolver = jobTypeResolver;
            _serviceProvider = serviceProvider;
        }

        internal object CreateInstance(string jobTypeName)
        {
            // Resolve Type
            _logger.LogDebug("Trying to resolve the specified type '{jobTypeName}'...", jobTypeName);

            var type = _jobTypeResolver.ResolveType(jobTypeName);

            if (type == null)
            {
                _logger.LogError("Unable to resolve the type '{jobTypeName}'!", jobTypeName);
                return null;
            }

            // Activation
            _logger.LogDebug("Type '{jobTypeName}' has been resolved to '{type}'. Activating now.", jobTypeName, type);

            object jobClassInstance;

            try
            {
                jobClassInstance = _serviceProvider.GetService(type);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Exception while activating type '{type}'. See Exception for details!", type);
                return null;
            }

            if (jobClassInstance == null)
            {
                _logger.LogError("Unable to create an instance ot the type '{type}'!", type);
            }

            return jobClassInstance;
        }

        internal void AddDependencies(params object[] additionalDependencies)
        {
            var registrar = _serviceProvider as IConfigurableServiceProvider;

            try
            {
                foreach (var dep in additionalDependencies)
                {
                    registrar?.RegisterInstance(dep);
                }

            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unable to register additional dependencies on {registrar}!", registrar);
            }
        }
    }
}