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

            var depdencencyNodes = doc.SelectNodes("package/metadata/dependencies/dependency");
            if (depdencencyNodes == null)
            {
                return;
            }

            foreach (XmlNode depdencencyNode in depdencencyNodes)
            {
                var nuspecDependency = XmlDependencyConverter.Convert(depdencencyNode);

                this.Dependencies.Add(nuspecDependency);
            }
        }
    }
}