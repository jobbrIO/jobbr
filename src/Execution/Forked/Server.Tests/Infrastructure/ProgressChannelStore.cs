using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;

namespace Jobbr.Server.ForkedExecution.Tests.Infrastructure
{
    /// <summary>
    /// Acts as a IJobRunProgressChannel but stores all updates internally, with the option to wait for Given conditions to happen.
    /// </summary>
    public class ProgressChannelStore : IJobRunProgressChannel
    {
        private readonly Dictionary<long, List<JobRunStates>> _jobRunStatusUpdates = new ();
        private readonly Dictionary<long, List<double>> _jobRunProgressUpdates = new ();
        private readonly Dictionary<long, List<string>> _jobRunArtefactUploads = new ();
        private readonly Dictionary<long, List<Tuple<string, long>>> _jobRunPids = new ();
        private readonly Dictionary<Func<Dictionary<long, List<JobRunStates>>, bool>, AutoResetEvent> _statusUpdateWaitCallBacks = new ();
        private readonly Dictionary<Func<Dictionary<long, List<double>>, bool>, AutoResetEvent> _progressUpdateWaitCallBacks = new ();

        public Dictionary<long, List<JobRunStates>> AllStatusUpdates => _jobRunStatusUpdates;

        public Dictionary<long, List<double>> AllProgressUpdates => _jobRunProgressUpdates;

        public Dictionary<long, List<string>> AllUploadedArtefacts => _jobRunArtefactUploads;

        public Dictionary<long, List<Tuple<string, long>>> AllPids => _jobRunPids;

        public void PublishStatusUpdate(long jobRunId, JobRunStates state)
        {
            if (!_jobRunStatusUpdates.ContainsKey(jobRunId))
            {
                _jobRunStatusUpdates.Add(jobRunId, new List<JobRunStates>());
            }

            _jobRunStatusUpdates[jobRunId].Add(state);

            foreach (var kvp in _statusUpdateWaitCallBacks)
            {
                try
                {
                    var callback = kvp.Key;

                    var callbackHasPermitted = callback(_jobRunStatusUpdates);

                    if (callbackHasPermitted)
                    {
                        kvp.Value.Set();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public bool WaitForStatusUpdate(Func<Dictionary<long, List<JobRunStates>>, bool> updatesFromAllJobs, int millisecondsTimeout)
        {
            try
            {
                var alreadyTrue = updatesFromAllJobs(_jobRunStatusUpdates);
                if (alreadyTrue)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var are = new AutoResetEvent(false);

            _statusUpdateWaitCallBacks.Add(updatesFromAllJobs, are);

            var successful = are.WaitOne(millisecondsTimeout, false);
            _statusUpdateWaitCallBacks.Remove(updatesFromAllJobs);

            return successful;
        }

        public bool WaitForProgressUpdate(Func<Dictionary<long, List<double>>, bool> updatesFromAllJobs, int millisecondsTimeout)
        {
            try
            {
                var alreadyTrue = updatesFromAllJobs(_jobRunProgressUpdates);
                if (alreadyTrue)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var are = new AutoResetEvent(false);

            _progressUpdateWaitCallBacks.Add(updatesFromAllJobs, are);

            var successful = are.WaitOne(millisecondsTimeout, false);
            _progressUpdateWaitCallBacks.Remove(updatesFromAllJobs);

            return successful;
        }

        public void PublishProgressUpdate(long jobRunId, double progress)
        {
            if (!_jobRunProgressUpdates.ContainsKey(jobRunId))
            {
                _jobRunProgressUpdates.Add(jobRunId, new List<double>());
            }

            _jobRunProgressUpdates[jobRunId].Add(progress);
        }

        public void PublishArtefact(long uniqueId, string fileName, Stream result)
        {
            if (!_jobRunArtefactUploads.ContainsKey(uniqueId))
            {
                _jobRunArtefactUploads.Add(uniqueId, new List<string>());
            }

            _jobRunArtefactUploads[uniqueId].Add(fileName);
        }

        public void PublishPid(long jobRunId, int pid, string host)
        {
            if (!_jobRunPids.ContainsKey(jobRunId))
            {
                _jobRunPids.Add(jobRunId, new List<Tuple<string, long>>());
            }

            _jobRunPids[jobRunId].Add(new Tuple<string, long>(host, pid));
        }
    }
}