using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class NuspecParser
    {
        public List<NuspecDependency> Dependencies { get; set; } = new List<NuspecDependency>();

        public NuspecParser(string nuspecFile = null)
        {
            if (!File.Exists(nuspecFile) || nuspecFile == null)
            {
                throw new ArgumentException($"File '{nuspecFile}' does not exist!", nameof(nuspecFile));
            }

            Parse(nuspecFile);
        }

        private void Parse(string nuspecFile)
        {
            // Parse dependencies
            var doc = new XmlDocument();

            doc.Load(nuspecFile);

            var mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("nu", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
            var namespaceNodes = doc.SelectNodes("nu:package/nu:metadata/nu:dependencies/nu:dependency", mgr);
            var noNameSpaceNodes = doc.SelectNodes("package/metadata/dependencies/dependency");

            if (namespaceNodes != null)
            {
                foreach (XmlNode depdencencyNode in namespaceNodes)
                {
                    var nuspecDependency = XmlDependencyConverter.Convert(depdencencyNode);

                    this.Dependencies.Add(nuspecDependency);
                }
            }

            if (noNameSpaceNodes != null)
            {
                foreach (XmlNode depdencencyNode in noNameSpaceNodes)
                {
                    var nuspecDependency = XmlDependencyConverter.Convert(depdencencyNode);

                    this.Dependencies.Add(nuspecDependency);
                }
            }
        }
    }
}