using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Parser for the package reference file.
    /// </summary>
    public class PackagesParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackagesParser"/> class.
        /// </summary>
        /// <param name="packageConfigFile">Optional parameter for the package configuration file path.</param>
        /// <exception cref="ArgumentException">Package configuration file not found.</exception>
        public PackagesParser(string packageConfigFile = null)
        {
            if (!File.Exists(packageConfigFile) || packageConfigFile == null)
            {
                throw new ArgumentException($"File '{packageConfigFile}' does not exist!", nameof(packageConfigFile));
            }

            Parse(packageConfigFile);
        }

        /// <summary>
        /// List of package dependencies.
        /// </summary>
        public List<NuspecDependency> Dependencies { get; set; } = new List<NuspecDependency>();

        private void Parse(string packageConfigFile)
        {
            // Parse dependencies
            var doc = new XmlDocument();

            doc.Load(packageConfigFile);

            var packageNodes = doc.SelectNodes("packages/package");
            if (packageNodes == null)
            {
                return;
            }

            foreach (XmlNode dependencyNode in packageNodes)
            {
                var nuspecDependency = XmlDependencyConverter.Convert(dependencyNode);

                Dependencies.Add(nuspecDependency);
            }
        }
    }
}
