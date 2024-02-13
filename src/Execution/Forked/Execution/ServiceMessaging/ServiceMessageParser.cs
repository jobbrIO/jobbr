using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Jobbr.Server.ForkedExecution.Execution.ServiceMessaging
{
    /// <summary>
    /// Parser for service messages.
    /// </summary>
    public class ServiceMessageParser
    {
        private static readonly IEnumerable<Type> KnownTypes;

        static ServiceMessageParser()
        {
            KnownTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes());
        }

        /// <summary>
        /// Parses a service message string to <see cref="ServiceMessage"/>.
        /// </summary>
        /// <param name="serviceMessage">Service message as string.</param>
        /// <returns>A <see cref="ServiceMessage"/> object.</returns>
        public ServiceMessage Parse(string serviceMessage)
        {
            var messageTypeRaw = string.Empty;
            var parametersRaw = string.Empty;

            var regex = new Regex(@"##jobbr\[([a-z]*[A-Z]*) (.*)\]");

            foreach (Match match in regex.Matches(serviceMessage))
            {
                var collection = match.Groups;

                if (collection.Count == 3)
                {
                    messageTypeRaw = collection[1].Value;
                    parametersRaw = collection[2].Value;
                }
            }

            // Identity CLR-MessageType
            var typeNameLowerCase = messageTypeRaw + "servicemessage";

            var messageTypes = KnownTypes.Where(t => t.IsSubclassOf(typeof(ServiceMessage)));
            var type = messageTypes.FirstOrDefault(t => string.Equals(t.Name.ToLowerInvariant(), typeNameLowerCase, StringComparison.Ordinal));

            if (type == null)
            {
                return null;
            }

            // Identity Parameters
            var split = parametersRaw.Split(new[] { '\'', '=' }, StringSplitOptions.RemoveEmptyEntries);
            var parameters = new Dictionary<string, string>();

            for (var i = 0; i < split.Length - 1; i += 2)
            {
                parameters.Add(split[i], split[i + 1]);
            }

            var instance = (ServiceMessage)Activator.CreateInstance(type);

            foreach (var key in parameters.Keys)
            {
                var prop = type.GetProperties().FirstOrDefault(p => string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));

                if (prop != null)
                {
                    var value = parameters[key];

                    if (prop.PropertyType == typeof(double))
                    {
                        if (string.IsNullOrEmpty(value) == false)
                        {
                            value = value.Replace(",", ".");
                        }

                        prop.SetValue(instance, value != null ? double.Parse(value, CultureInfo.InvariantCulture) : 0);
                    }

                    if (prop.PropertyType == typeof(int))
                    {
                        prop.SetValue(instance, int.Parse(value));
                    }

                    if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(instance, value);
                    }
                }
            }

            return instance;
        }
    }
}