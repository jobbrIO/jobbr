using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class NuSpecDependencyParser
    {
        public List<NuspecDependency> Dependencies { get; set; } = new List<NuspecDependency>();

        public NuSpecDependencyParser(string nuspecFile = null)
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

            foreach (XmlNode depdencencyNode in depdencencyNodes)
            {
                var id = depdencencyNode.Attributes["id"];
                var versionString = depdencencyNode.Attributes["version"];

                this.Dependencies.Add(new NuspecDependency {Name = id.Value, Version = versionString.Value});
            }
        }
    }
}