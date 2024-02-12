using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Jobbr.Runtime.Activation
{
    internal class JobTypeResolver
    {
        private readonly ILogger<JobTypeResolver> _logger;

        private readonly IList<Assembly> _assemblies;

        public JobTypeResolver(ILoggerFactory loggerFactory, IList<Assembly> assemblies)
        {
            _logger = loggerFactory.CreateLogger<JobTypeResolver>();
            _assemblies = assemblies;
        }

        internal Type ResolveType(string typeName)
        {
            _logger.LogDebug("Resolve type using '{typeName}' like a full qualified CLR-Name", typeName);
            var type = Type.GetType(typeName);

            if (type == null && _assemblies != null)
            {
                foreach (var assembly in _assemblies)
                {
                    _logger.LogDebug("Trying to resolve '{typeName}' by the assembly '{fullName}'", typeName, assembly.FullName);
                    type = assembly.GetType(typeName, false, true);
                    if (type != null)
                    {
                        break;
                    }
                }
            }

            if (type == null)
            {
                // Search in all Assemblies
                var allReferenced = Assembly.GetExecutingAssembly().GetReferencedAssemblies();

                _logger.LogDebug("Trying to resolve type by asking all referenced assemblies ('{assemblies}')", string.Join(", ", allReferenced.Select(a => a.Name)));

                foreach (var assemblyName in allReferenced)
                {
                    var assembly = Assembly.Load(assemblyName);

                    var foundType = assembly.GetType(typeName, false, true);

                    if (foundType != null)
                    {
                        type = foundType;
                    }
                }
            }

            if (type == null)
            {
                _logger.LogDebug("Still no luck finding '{typeName}' somewhere. Iterating through all types and comparing class-names. Please hold on", typeName);

                // Absolutely no clue
                var matchingTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => string.Equals(x.Name, typeName, StringComparison.Ordinal) && x.IsClass && !x.IsAbstract).ToList();

                if (matchingTypes.Count() == 1)
                {
                    _logger.LogDebug("Found matching type: '{matchingTypes}'", matchingTypes[0]);
                    type = matchingTypes.First();
                }
                else if (matchingTypes.Count > 1)
                {
                    _logger.LogWarning("More than one matching type found for '{typeName}'. Matches: {matchingTypes}", typeName, string.Join(", ", matchingTypes.Select(t => t.FullName)));
                }
                else
                {
                    _logger.LogWarning("No matching type found for '{typeName}'.", typeName);
                }
            }

            return type;
        }
    }
}