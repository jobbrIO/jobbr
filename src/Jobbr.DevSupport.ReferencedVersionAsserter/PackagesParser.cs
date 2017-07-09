using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class PackagesParser
    {
        public List<NuspecDependency> Dependencies { get; set; } = new List<NuspecDependency>();

        public PackagesParser(string projFile = null)
        {
            if (!File.Exists(projFile) || projFile == null)
            {
                throw new ArgumentException($"File '{projFile}' does not exist!", nameof(projFile));
            }

            Parse(projFile);
        }

        private void Parse(string nugetConfigfile)
        {

            // Parse dependencies
            var doc = new XmlDocument();

            doc.Load(nugetConfigfile);

            var packageNodes = doc.SelectNodes("packages/package");
            if (packageNodes == null)
            {
                return;
            }

            foreach (XmlNode depdencencyNode in packageNodes)
            {
                var nuspecDependency = XmlDependencyConverter.Convert(depdencencyNode);

                this.Dependencies.Add(nuspecDependency);
            }
        }
    }
}
