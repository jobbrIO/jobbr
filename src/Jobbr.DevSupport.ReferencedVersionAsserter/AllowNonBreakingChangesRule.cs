using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class AllowNonBreakingChangesRule : WildCharNuspecRuleBase
    {
        public AllowNonBreakingChangesRule(string packageName) : base(packageName)
        {
        }

        protected override bool Validate(NuspecDependency nuspecDependency, NuspecDependency nuConfigPackage, out string message)
        {
            var specMin = nuspecDependency.MinVersion;
            var specMax = nuspecDependency.MaxVersion;

            if (specMax == null)
            {
                message = $"Cannot evaluate rule for '{nuspecDependency.Name}' if no max range is set";
                return false;
            }

            if (specMax.Major == specMin.Major && !(specMax.Minor > specMin.Minor))
            {
                message = $"There package reference '{nuspecDependency.Name}' with version '{nuspecDependency.Version}' does not allow non-breaking changes";
                return false;
            }

            message = null;
            return false;
        }
    }
}
