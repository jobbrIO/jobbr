using System;
using System.Xml;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class XmlDependencyConverter
    {
        public static NuspecDependency Convert(XmlNode depdencencyNode)
        {
            if (depdencencyNode == null)
            {
                throw new ArgumentNullException(nameof(depdencencyNode));
            }

            if (depdencencyNode.Attributes == null)
            {
                return null;
            }

            var id = depdencencyNode.Attributes["id"];
            var versionString = depdencencyNode.Attributes["version"];

            var nuspecDependency = new NuspecDependency { Name = id.Value, Version = versionString.Value };
            return nuspecDependency;
        }
    }
}