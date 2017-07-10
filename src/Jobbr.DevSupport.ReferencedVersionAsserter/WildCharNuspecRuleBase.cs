using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public abstract class WildCharNuspecRuleBase : IAssertionRule
    {
        private readonly string packageName;

        protected WildCharNuspecRuleBase(string packageName)
        {
            this.packageName = packageName;
        }

        public bool Validate(List<NuspecDependency> nuspecDependencies, List<NuspecDependency> packageDependencies, out string message)
        {
            string searchString;
            message = string.Empty;

            if (!this.packageName.EndsWith("*"))
            {
                searchString = this.packageName;
            }
            else
            {
                searchString = this.packageName.Substring(0, this.packageName.Length - 1);
            }

            var nuspecPackage = nuspecDependencies.Where(np => np.Name.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (!nuspecPackage.Any())
            {
                message = $"No Package with name '{this.packageName}' can be found in NuSpec.";
                return false;
            }

            var allMessages = new StringBuilder();

            foreach (var nuspecDependency in nuspecPackage)
            {
                var nuConfigPackage = packageDependencies.SingleOrDefault(d => d.Name.Equals(nuspecDependency.Name, StringComparison.InvariantCultureIgnoreCase));

                string singleMessage = "";

                if (this.Validate(nuspecDependency, nuConfigPackage, out singleMessage))
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

        protected abstract bool Validate(NuspecDependency nuspecDependency, NuspecDependency nuConfigPackage, out string message);
    }
}