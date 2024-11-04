using System;
using System.Xml;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Helper class for converting XML dependencies.
    /// </summary>
    public static class XmlDependencyConverter
    {
        /// <summary>
        /// Converts an <see cref="XmlNode"/> to a <see cref="NuspecDependency"/>.
        /// </summary>
        /// <param name="dependencyNode">The target <see cref="XmlNode"/>.</param>
        /// <returns>The resulting <see cref="NuspecDependency"/>.</returns>
        /// <exception cref="ArgumentNullException">The provided <see cref="XmlNode"/> is null.</exception>
        public static NuspecDependency Convert(XmlNode dependencyNode)
        {
            if (dependencyNode == null)
            {
                throw new ArgumentNullException(nameof(dependencyNode));
            }

            if (dependencyNode.Attributes == null)
            {
                return null;
            }

            var id = dependencyNode.Attributes["id"];
            var versionString = dependencyNode.Attributes["version"];

            if (id == null || versionString == null)
            {
                return null;
            }

            var versionStringValue = versionString.Value;
            var nuspecDependency = new NuspecDependency
            {
                Name = id.Value,
                Version = versionStringValue
            };

            return NuspecVersionParser.Parse(nuspecDependency, versionStringValue);
        }
    }
}