using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jobbr.Runtime.ForkedExecution
{
    /// <summary>
    /// https://github.com/dotnet/runtime/issues/31094
    /// Default options for JSON serialization and deserialization.
    /// These are required because the System.Text.Json doesn't provide global defaults.
    /// </summary>
    public static class DefaultJsonOptions
    {
        /// <summary>
        /// The default options.
        /// </summary>
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };
    }
}
