using System.Collections.Generic;
using System.Linq;
using Jobbr.DevSupport.ReferencedVersionAsserter;

namespace Jobbr.Server.ForkedExecution.Tests
{
    public class NoExternalDependenciesRule : IAssertionRule
    {
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