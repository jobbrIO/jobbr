using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Parser for the .nuspec file.
    /// </summary>
    public class NuspecParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NuspecParser"/> class.
        /// </summary>
        /// <param name="nuspecFile">Optional parameter for specifying the name of the .nuspec file.</param>
        /// <exception cref="ArgumentException">.nuspec file not found.</exception>
        public NuspecParser(string nuspecFile = null)
        {
            if (!File.Exists(nuspecFile) || nuspecFile == null)
            {
                throw new ArgumentException($"File '{nuspecFile}' does not exist!", nameof(nuspecFile));
            }

            Parse(nuspecFile);
        }

        /// <summary>
        /// List of dependencies parsed from the file.
        /// </summary>
        public List<NuspecDependency> Dependencies { get; set; } = new List<NuspecDependency>();

        private void Parse(string nuspecFile)
        {
            // Parse dependencies
            var doc = new XmlDocument();

            doc.Load(nuspecFile);

            var mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("nu", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
            var namespaceNodes = doc.SelectNodes("nu:package/nu:metadata/nu:dependencies/nu:dependency", mgr);
            var noNameSpaceNodes = doc.SelectNodes("package/metadata/dependencies/dependency");
            var groupNameSpaceNodes = doc.SelectNodes("package/metadata/dependencies/group/dependency");

            if (namespaceNodes != null)
            {
                foreach (XmlNode dependencyNode in namespaceNodes)
                {
                    var nuspecDependency = XmlDependencyConverter.Convert(dependencyNode);

                    Dependencies.Add(nuspecDependency);
                }
            }

            if (noNameSpaceNodes != null)
            {
                foreach (XmlNode dependencyNode in noNameSpaceNodes)
                {
                    var nuspecDependency = XmlDependencyConverter.Convert(dependencyNode);

                    Dependencies.Add(nuspecDependency);
                }
            }

            if (groupNameSpaceNodes != null)
            {
                foreach (XmlNode dependencyNode in groupNameSpaceNodes)
                {
                    var nuspecDependency = XmlDependencyConverter.Convert(dependencyNode);

                    Dependencies.Add(nuspecDependency);
                }
            }
        }
    }
}