using System.IO;

namespace Jobbr.Server.ForkedExecution.TestEcho
{
    /// <summary>
    /// Root.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Entry arguments.</param>
        public static void Main(string[] args)
        {
            var filename = Path.GetRandomFileName();
            File.WriteAllText(filename, string.Join("\n", args));
        }
    }
}
