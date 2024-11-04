using System.IO;

namespace Jobbr.Server.ForkedExecution.TestRunner.TestJobs
{
    /// <summary>
    /// Job with artifacts.
    /// </summary>
    public class JobWithArtefacts
    {
        /// <summary>
        /// Run job.
        /// </summary>
        public void Run()
        {
            File.AppendAllText("testfile.txt", "Testcontent");
        }
    }
}
