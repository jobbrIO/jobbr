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

            if (id == null || versionString == null)
            {
                return null;
            }

            var versionStringValue = versionString.Value;

            var nuspecDependency = new NuspecDependency { Name = id.Value, Version = versionStringValue };

            // Parse Version
            if (versionStringValue.Contains(","))
            {
                if(!((versionStringValue.StartsWith("[") || versionStringValue.StartsWith("(")) && (versionStringValue.EndsWith("]") || versionStringValue.EndsWith(")"))))
                {
                    throw new ArgumentException();
                }
      
                var rangeParts = versionStringValue.Split(new[] {","}, StringSplitOptions.None);

                // Left Part
                if (!string.IsNullOrWhiteSpace(rangeParts[0]) && rangeParts[0].Length > 1)
                {
                    var leftVersion = CreateVersion(rangeParts[0].Substring(1));
                    leftVersion.Inclusive = rangeParts[0].StartsWith("[");

                    nuspecDependency.MinVersion = leftVersion;
                }

                if (!string.IsNullOrWhiteSpace(rangeParts[1]) && rangeParts[1].Length > 1)
                {
                    // Right Part
                    var rightVersion = CreateVersion(rangeParts[1].Substring(0, rangeParts[1].Length - 1));
                    rightVersion.Inclusive = rangeParts[1].EndsWith("]");

                    nuspecDependency.MaxVersion = rightVersion;
                }
            }
            else
            {
                // [1.0]	x == 1.0	Exact version match
                if (versionStringValue.StartsWith("[") && versionStringValue.EndsWith("]"))
                {
                    var exactVersion = CreateVersion(versionStringValue.Substring(1, versionStringValue.Length - 2));

                    exactVersion.Inclusive = true;

                    nuspecDependency.MinVersion = exactVersion;
                    nuspecDependency.MaxVersion = exactVersion;
                }
                else
                {
                    var minVersion = CreateVersion(versionStringValue);
                    minVersion.Inclusive = true;

                    nuspecDependency.MinVersion = minVersion;
                    nuspecDependency.MaxVersion = null;
                }
            }

            return nuspecDependency;
        }

        private static NuspecVersion CreateVersion(string versionStringValue)
        {
            // Filter PreTags
            var tagSplitted = versionStringValue.Split(new[] {"-"}, StringSplitOptions.RemoveEmptyEntries);

            var versionParts = tagSplitted[0].Split(new[] {"."}, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                var major = versionParts.Length >= 1 ? Int32.Parse(versionParts[0]) : 0;
                var minor = versionParts.Length >= 2 ? Int32.Parse(versionParts[1]) : 0;
                var bugix = versionParts.Length == 3 ? Int32.Parse(versionParts[2]) : 0;

                var exactVersion = new NuspecVersion() { Major = major, Minor = minor, Bugfix = bugix };

                if (tagSplitted.Length == 2)
                {
                    exactVersion.Tag = tagSplitted[1];
                }

                return exactVersion;
            }
            catch (Exception)
            {
                throw new Exception($"Unable to cast version-string '{versionStringValue}'.");
            }
        }
    }
}