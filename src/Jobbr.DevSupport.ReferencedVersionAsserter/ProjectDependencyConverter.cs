using System;
using System.Xml;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Static helper class for project dependency conversion.
    /// </summary>
    public static class ProjectDependencyConverter
    {
        /// <summary>
        /// Converts <see cref="XmlNode"/> to a <see cref="NuspecDependency"/>.
        /// </summary>
        /// <param name="dependencyNode">The XML node to convert.</param>
        /// <returns>A <see cref="NuspecDependency"/>.</returns>
        /// <exception cref="ArgumentNullException">Null XML node reference.</exception>
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

            var include = dependencyNode.Attributes["Include"];
            var versionString = dependencyNode.Attributes["Version"];

            if (include == null || versionString == null)
            {
                return null;
            }

            var versionStringValue = versionString.Value;
            var nuspecDependency = new NuspecDependency { Name = include.Value, Version = versionStringValue };

            return NuspecVersionParser.Parse(nuspecDependency, versionStringValue);
        }
    }
}