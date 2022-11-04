using System;
using System.Xml;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public static class ProjectDependencyConverter
    {
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