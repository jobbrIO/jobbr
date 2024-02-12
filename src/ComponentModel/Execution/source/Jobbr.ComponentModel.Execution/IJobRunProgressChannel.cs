using System.IO;
using Jobbr.ComponentModel.Execution.Model;

namespace Jobbr.ComponentModel.Execution
{
    /// <summary>
    /// IJobRunProgressChannel is implemented by Jobbr.Server. Job Executors report progress
    /// of jobs via this interface to the Server.
    /// </summary>
    public interface IJobRunProgressChannel
    {
        void PublishStatusUpdate(long jobRunId, JobRunStates state);

        void PublishProgressUpdate(long jobRunId, double progress);

        void PublishArtefact(long jobRunId, string fileName, Stream content);

        void PublishPid(long jobRunId, int pid, string host);
    }
}