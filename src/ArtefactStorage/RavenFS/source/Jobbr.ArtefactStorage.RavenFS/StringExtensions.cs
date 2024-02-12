using System;

namespace Jobbr.ArtefactStorage.RavenFS
{
    public static class StringExtensions
    {
        public static string ExtractExtension(this string str)
        {
            var index = str.LastIndexOf(".", StringComparison.Ordinal);

            if (index < 0)
            {
                return string.Empty;
            }

            return str.Substring(index);
        }
    }
}
