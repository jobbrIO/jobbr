using System;

namespace Jobbr.Server.ForkedExecution.TestRunner.TestJobs
{
    /// <summary>
    /// Job with one progress update.
    /// </summary>
    public class JobWithOneProgress
    {
        /// <summary>
        /// Gets hardcoded progress value for the test job.
        /// </summary>
        public static double DefinedProgressValue => 42.135;

        /// <summary>
        /// Run job with one progress update.
        /// </summary>
        /// <param name="jobParams">Job parameters.</param>
        /// <param name="runParams">Run parameters.</param>
        public void Run(object jobParams, TestArguments runParams)
        {
            Console.WriteLine("##jobbr[progress percent='" + DefinedProgressValue + "']");

            if (runParams?.ShouldFail == true)
            {
                Environment.Exit(-38464);
            }
        }
    }
}