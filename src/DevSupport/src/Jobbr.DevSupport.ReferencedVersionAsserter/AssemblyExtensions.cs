using System.Reflection;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// Static class for <see cref="Assembly"/> extension methods.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets the informal version of the assembly.
        /// </summary>
        /// <param name="assembly">Target assembly.</param>
        /// <returns>Informal version.</returns>
        public static string GetInformalVersion(this Assembly assembly)
        {
            var attr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (attr == null)
            {
                return string.Empty;
            }

            return attr.InformationalVersion;
        }
    }
}
