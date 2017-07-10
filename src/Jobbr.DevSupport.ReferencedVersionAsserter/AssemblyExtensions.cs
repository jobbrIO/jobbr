using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public static class AssemblyExtensions
    {
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
