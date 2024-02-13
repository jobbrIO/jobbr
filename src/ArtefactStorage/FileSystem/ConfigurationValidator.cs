using System;
using System.IO;
using System.Linq;
using Jobbr.ComponentModel.Registration;

namespace Jobbr.ArtefactStorage.FileSystem
{
    public class ConfigurationValidator : IConfigurationValidator
    {
        public Type ConfigurationType { get; set; } = typeof(FileSystemArtefactStorageConfiguration);

        public bool Validate(object configuration)
        {
            var config = configuration as FileSystemArtefactStorageConfiguration;

            if (string.IsNullOrWhiteSpace(config?.DataDirectory))
            {
                return false;
            }

            if (!IsValidPath(config.DataDirectory))
            {
                return false;
            }

            return true;
        }

        private static bool IsValidPath(string path)
        {
            // Checking of UNC-Paths can take too much time, we need to skip here
            // See: http://stackoverflow.com/questions/5152647/how-to-quickly-check-if-unc-path-is-available
            if (path.StartsWith(@"\\"))
            {
                return true;
            }

            FileInfo fi = null;
            try
            {
                fi = new FileInfo(path);
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            catch (NotSupportedException) { }

            if (ReferenceEquals(fi, null))
            {
                return false;
            }

            // Check for drive
            string pathRoot = Path.GetPathRoot(path);
            if (!Directory.GetLogicalDrives().Contains(pathRoot))
            {
                return false;
            }

            return true;
        }
    }
}
