using System.Collections.Generic;
using System.Linq;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Rule for disallowing external dependencies.
    /// </summary>
    public class NoExternalDependenciesRule : IAssertionRule
    {
        /// <inheritdoc/>
        public bool Validate(List<NuspecDependency> nuspecDependencies, List<NuspecDependency> packageDependencies, out string message)
        {
            if (nuspecDependencies == null || !nuspecDependencies.Any())
            {
                message = null;
                return true;
            }

            message = $"Package specification should not contain any dependencies but found: {string.Join(", ", nuspecDependencies.Select(d => d.Name).ToArray())}";

            return false;
        }
    }
}