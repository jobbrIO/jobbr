using System;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Jobbr.Runtime.Execution
{
    internal class RunWrapperFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<RunWrapperFactory> _logger;
        private readonly Type _jobType;
        private readonly object _jobParameter;
        private readonly object _instanceParameter;

        public RunWrapperFactory(ILoggerFactory loggerFactory, Type jobType, object jobParameter, object instanceParameter)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<RunWrapperFactory>();
            _jobType = jobType;
            _jobParameter = jobParameter;
            _instanceParameter = instanceParameter;
        }

        private object GetCastedParameterValue(string parameterName, Type targetType, string jobbrParamName, object value)
        {
            object castedValue;

            _logger.LogInformation("Casting {jobbrParamName}-parameter to its target value '{targetType}' based on the Run()-Parameter {parameterName}", jobbrParamName, targetType, parameterName);

            // Try to cast them to specific types
            if (value == null)
            {
                _logger.LogDebug("The {jobbrParamName}-parameter is null - no cast needed.", jobbrParamName);
                castedValue = null;
            }
            else if (targetType == typeof(object))
            {
                _logger.LogDebug("The {jobbrParamName}-parameter is of type 'object' - no cast needed.", jobbrParamName);
                castedValue = value;
            }
            else
            {
                _logger.LogDebug("The {jobbrParamName}-parameter '{parameterName}' is from type '{targetType}'. Casting this value to '{targetType}'", jobbrParamName, parameterName, targetType, targetType);
                castedValue = JsonSerializer.Deserialize(value.ToString(), targetType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return castedValue;
        }

        internal JobWrapper CreateWrapper(object jobClassInstance, UserContext runtimeContext)
        {
            var runMethods = _jobType.GetMethods().Where(m => string.Equals(m.Name, "Run", StringComparison.Ordinal) && m.IsPublic).ToList();

            if (!runMethods.Any())
            {
                _logger.LogError("Unable to find an entrypoint to call your job. Is there at least a public Run()-Method?");
                return null;
            }

            Action runMethodWrapper = null;

            // Try to use the method with 2 concrete parameters
            var parameterizedMethod = runMethods.FirstOrDefault(m => m.GetParameters().Length == 2);
            if (parameterizedMethod != null)
            {
                var jobParamValue = _jobParameter ?? "<null>";
                var instanceParamValue = _instanceParameter ?? "<null>";

                var jobParamJsonString = jobParamValue.ToString();
                var instanceParamJsonString = instanceParamValue.ToString();

                // Note: We cannot use string interpolation here, because LibLog is using string.format again and will fail if there are { } chars in the string, even if there is no formatting needed.
                _logger.LogDebug("Decided to use parameterized method '{parameterizedMethod}' with JobParameter '{jobParameter}' and InstanceParameters '{instanceParameters}'.", parameterizedMethod, jobParamJsonString, instanceParamJsonString);
                var allParams = parameterizedMethod.GetParameters().OrderBy(p => p.Position).ToList();

                var param1Type = allParams[0].ParameterType;
                var param2Type = allParams[1].ParameterType;

                var param1Name = allParams[0].Name;
                var param2Name = allParams[1].Name;

                // Casting in the most preferable type
                var jobParameterValue = GetCastedParameterValue(param1Name, param1Type, "job", _jobParameter);
                var instanceParameterValue = GetCastedParameterValue(param2Name, param2Type, "instance", _instanceParameter);

                runMethodWrapper = () => { parameterizedMethod.Invoke(jobClassInstance, new[] {jobParameterValue, instanceParameterValue}); };
            }
            else
            {
                var fallBackMethod = runMethods.FirstOrDefault(m => !m.GetParameters().Any());

                if (fallBackMethod != null)
                {
                    _logger.LogDebug("Decided to use parameterless method '{fallBackMethod}'", fallBackMethod);
                    runMethodWrapper = () => fallBackMethod.Invoke(jobClassInstance, null);
                }
            }

            if (runMethodWrapper == null)
            {
                _logger.LogError("None of your Run()-Methods are compatible with Jobbr. Please see documentation");
                return null;
            }

            _logger.LogDebug("Initializing task for JobRun");

            return new JobWrapper(_loggerFactory, runMethodWrapper, runtimeContext);
        }
    }
}