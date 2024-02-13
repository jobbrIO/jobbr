using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Project file parser.
    /// </summary>
    public class ProjectParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectParser"/> class.
        /// </summary>
        /// <param name="projectFile">Optional parameter for the project file filepath.</param>
        /// <exception cref="ArgumentException">Project file not found.</exception>
        public ProjectParser(string projectFile = null)
        {
            if (!File.Exists(projectFile) || projectFile == null)
            {
                throw new ArgumentException($"File '{projectFile}' does not exist!", nameof(projectFile));
            }

            Parse(projectFile);
        }

        /// <summary>
        /// List of package dependencies.
        /// </summary>
        public List<NuspecDependency> Dependencies { get; set; } = new List<NuspecDependency>();

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

                Dependencies.Add(nuspecDependency);
            }
        }
    }
}
