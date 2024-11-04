using System.Reflection;
using Jobbr.Runtime;
using Jobbr.Runtime.ForkedExecution;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jobbr.Server.ForkedExecution.TestRunner
{
    /// <summary>
    /// Root.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Execution arguments.</param>
        public static void Main(string[] args)
        {
            var runtime = new ForkedRuntime(NullLoggerFactory.Instance, new RuntimeConfiguration { JobTypeSearchAssemblies = new[] { Assembly.GetEntryAssembly() } });

            runtime.Run(args);
        }
    }
}
