using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class ProjectParser
    {
        public List<NuspecDependency> Dependencies { get; set; } = new List<NuspecDependency>();

        public ProjectParser(string projectFile = null)
        {
            if (!File.Exists(projectFile) || projectFile == null)
            {
                throw new ArgumentException($"File '{projectFile}' does not exist!", nameof(projectFile));
            }

            Parse(projectFile);
        }

        private void Parse(string projectFile)
        {
            // Parse dependencies
            var doc = new XmlDocument();

            doc.Load(projectFile);

            var packageNodes = doc.SelectNodes("Project/ItemGroup/PackageReference");
            if (packageNodes == null)
            {
                return;
            }

            foreach (XmlNode dependencyNode in packageNodes)
            {
                var nuspecDependency = ProjectDependencyConverter.Convert(dependencyNode);

                this.Dependencies.Add(nuspecDependency);
            }
        }
    }
}
