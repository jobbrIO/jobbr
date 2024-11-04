using System.Collections.Generic;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Interface for the assertion rules.
    /// </summary>
    public interface IAssertionRule
    {
        /// <summary>
        /// Validates that the .nuspec and package dependencies match.
        /// </summary>
        /// <param name="nuspecDependencies">.nuspec dependencies.</param>
        /// <param name="packageDependencies">Package dependencies.</param>
        /// <param name="message">Possible error message.</param>
        /// <returns>If valid.</returns>
        bool Validate(List<NuspecDependency> nuspecDependencies, List<NuspecDependency> packageDependencies, out string message);
    }
}
