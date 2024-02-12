using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Wild character .nuspec rule base class.
    /// </summary>
    public abstract class WildCharNuspecRuleBase : IAssertionRule
    {
        private readonly string _packageName;

        /// <summary>
        /// Initializes a new instance of the <see cref="WildCharNuspecRuleBase"/> class.
        /// </summary>
        /// <param name="packageName">The target package name.</param>
        protected WildCharNuspecRuleBase(string packageName)
        {
            _packageName = packageName;
        }

        /// <inheritdoc/>
        public bool Validate(List<NuspecDependency> nuspecDependencies, List<NuspecDependency> packageDependencies, out string message)
        {
            string searchString;
            message = string.Empty;

            if (!_packageName.EndsWith("*"))
            {
                searchString = _packageName;
            }
            else
            {
                searchString = _packageName.Substring(0, _packageName.Length - 1);
            }

            var nuspecPackage = nuspecDependencies.Where(np => np.Name.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (!nuspecPackage.Any())
            {
                message = $"No Package with name '{_packageName}' can be found in NuSpec.";
                return false;
            }

            var allMessages = new StringBuilder();

            foreach (var nuspecDependency in nuspecPackage)
            {
                var nuConfigPackage = packageDependencies.SingleOrDefault(d => d.Name.Equals(nuspecDependency.Name, StringComparison.InvariantCultureIgnoreCase));

                if (Validate(nuspecDependency, nuConfigPackage, out var singleMessage))
                {
                    continue;
                }

                if (allMessages.Length > 0)
                {
                    allMessages.Append("\n");
                }

                allMessages.Append(singleMessage);
            }

            if (allMessages.Length > 0)
            {
                message = allMessages.ToString();
                return false;
            }

            message = null;
            return true;
        }

        /// <summary>
        /// Validates that the .nuspec dependencies match.
        /// </summary>
        /// <param name="nuspecDependency">.nuspec dependency.</param>
        /// <param name="nuConfigPackage">.nuspec dependency comparer.</param>
        /// <param name="message">Error message.</param>
        /// <returns>If the dependencies match.</returns>
        protected abstract bool Validate(NuspecDependency nuspecDependency, NuspecDependency nuConfigPackage, out string message);
    }
}