using System;
using System.Diagnostics;
using System.IO;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.Server.ForkedExecution.Execution.ServiceMessaging;
using Microsoft.Extensions.Logging;

namespace Jobbr.Server.ForkedExecution.Execution
{
    /// <summary>
    /// The runner process.
    /// </summary>
    internal class JobRunContext : IJobRunContext
    {
        private readonly ILogger _logger;

        private readonly JobRunInfo _jobRunInfo;
        private readonly ForkedExecutionConfiguration _configuration;
        private readonly IJobRunProgressChannel _progressChannel;
        private readonly ServiceMessageParser _serviceMessageParser;

        private bool _didReportProgress;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobRunContext"/> class.
        /// </summary>
        public JobRunContext(ILoggerFactory loggerFactory, JobRunInfo jobRunInfo, ForkedExecutionConfiguration configuration, IJobRunProgressChannel progressChannel)
        {
            _logger = loggerFactory.CreateLogger<JobRunContext>();
            _jobRunInfo = jobRunInfo;
            _configuration = configuration;
            _progressChannel = progressChannel;

            _serviceMessageParser = new ServiceMessageParser();
        }

        /// <summary>
        /// Job run ended event.
        /// </summary>
        public event EventHandler<JobRunEndedEventArgs> Ended;

        /// <summary>
        /// Job run ID.
        /// </summary>
        public long JobRunId => _jobRunInfo.Id;

        /// <inheritdoc />
        public void Start()
        {
            _logger.LogInformation("[{jobRunId}] A new JobRunContext is starting for JobRun with ID: '{jobRunId}'. (JobID: '{jobId}', TriggerID: '{triggerId}'", _jobRunInfo.Id, _jobRunInfo.Id, _jobRunInfo.JobId, _jobRunInfo.TriggerId);

            try
            {
                var workDir = SetupDirectories(_jobRunInfo);

                StartProcess(_jobRunInfo, workDir);

                _progressChannel.PublishStatusUpdate(_jobRunInfo.Id, JobRunStates.Started);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[{jobRunId}] Exception thrown while starting JobRun with ID: '{jobRunId}'. (JobID: '{jobId}', TriggerID: '{triggerId}'", _jobRunInfo.Id, _jobRunInfo.Id, _jobRunInfo.JobId, _jobRunInfo.TriggerId);
            }
        }

        /// <summary>
        /// Event handler for the job run ended event.
        /// </summary>
        /// <param name="eventArgs">Event arguments.</param>
        protected virtual void OnEnded(JobRunEndedEventArgs eventArgs)
        {
            _logger.LogInformation("[{jobRunId}] The runner with process ID '{processId}' has ended at '{exitTime}'. Exit code: '{exitCode}'", _jobRunInfo.Id, eventArgs.ProcInfo.Id, eventArgs.ProcInfo.ExitTime, eventArgs.ProcInfo.ExitCode);

            Ended?.Invoke(this, eventArgs);
        }

        private void StartProcess(JobRunInfo jobRun, string workDir)
        {
            var runnerFileExe = Path.GetFullPath(_configuration.JobRunnerExecutable);
            _logger.LogInformation("[{jobRunId}] Preparing to start the runner from '{executablePath}' in '{workDir}'", jobRun.Id, runnerFileExe, workDir);

            var proc = new Process
            {
                EnableRaisingEvents = true,
                StartInfo =
                {
                    FileName = runnerFileExe,
                },
            };

            var arguments = $"--jobRunId {jobRun.Id} --server {_configuration.BackendAddress}";
            if (_configuration.IsRuntimeWaitingForDebugger)
            {
                arguments += " --debug";
            }

            if (_configuration.AddJobRunnerArguments != null)
            {
                var model = new JobRunStartInfo
                {
                    JobType = jobRun.Type,
                    UniqueName = jobRun.UniqueName,
                    JobRunId = jobRun.Id,
                    JobId = jobRun.JobId,
                    TriggerId = jobRun.TriggerId,
                    UserId = jobRun.UserId,
                };

                var additionalArguments = _configuration.AddJobRunnerArguments(model);

                foreach (var additionalArgument in additionalArguments)
                {
                    if (additionalArgument.Value.Contains(" "))
                    {
                        arguments += $" --{additionalArgument.Key} \"{additionalArgument.Value}\"";
                    }
                    else
                    {
                        arguments += $" --{additionalArgument.Key} {additionalArgument.Value}";
                    }
                }
            }

            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.WorkingDirectory = workDir;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            // Wire events
            proc.OutputDataReceived += ProcOnOutputDataReceived;
            proc.Exited += (o, args) => OnEnded(new JobRunEndedEventArgs
            {
                ExitCode = proc.ExitCode,
                JobRun = jobRun,
                ProcInfo = proc,
                DidReportProgress = _didReportProgress,
            });

            _progressChannel.PublishStatusUpdate(_jobRunInfo.Id, JobRunStates.Starting);
            _logger.LogInformation("[{jobRunId}] Starting '{executablePath} {arguments}' in '{workDir}'", jobRun.Id, runnerFileExe, arguments, workDir);
            proc.Start();

            _logger.LogInformation("[{jobRunId}] Started Runner with process ID '{processId}' at '{startTime}'", jobRun.Id, proc.Id, proc.StartTime);
            _progressChannel.PublishPid(jobRun.Id, proc.Id, Environment.MachineName);

            proc.BeginOutputReadLine();
        }

        private string SetupDirectories(JobRunInfo jobRun)
        {
            // Create the WorkingDir and TempDir for the execution
            var jobRunPath = Path.Combine(_configuration.JobRunDirectory, "jobbr-" + jobRun.Id);

            _logger.LogInformation("[{jobRunId}] Preparing filesystem directories in '{jobRunDirectory}'", jobRun.Id, _configuration.JobRunDirectory);

            var tempDir = Path.Combine(jobRunPath, "temp");
            var workDir = Path.Combine(jobRunPath, "work");

            Directory.CreateDirectory(tempDir);
            _logger.LogInformation("[{jobRun.Id}] Created Temp-Directory '{tempDir}'", jobRun.Id, tempDir);

            Directory.CreateDirectory(workDir);
            _logger.LogInformation("[{jobRun.Id}] Created Working-Directory '{workDir}'", jobRun.Id, tempDir);

            return workDir;
        }

        private void ProcOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            var data = dataReceivedEventArgs.Data;

            if (data == null)
            {
                return;
            }

            var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // Detect ServiceMessage
                if (line.StartsWith("##jobbr"))
                {
                    var message = _serviceMessageParser.Parse(line);

                    try
                    {
                        if (message != null)
                        {
                            if (HandleMessage(message as dynamic))
                            {
                                _logger.LogDebug("[{jobRunId}] Handled service-message of type '{messageType}'. Raw value: '{line}'", _jobRunInfo.Id, message.GetType().Name, line);
                            }
                            else
                            {
                                _logger.LogWarning("[{jobRunId}] Cannot handle messages of type '{messageType}'. Raw value: '{line}'", _jobRunInfo.Id, message.GetType().Name, line);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("[{jobRunId}] Parsing Error while processing service message '{line}'", _jobRunInfo.Id, line);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "[{jobRunId}] Exception while processing service message '{line}'", _jobRunInfo.Id, line);
                    }
                }
            }
        }

        private bool HandleMessage(ProgressServiceMessage message)
        {
            _progressChannel.PublishProgressUpdate(_jobRunInfo.Id, message.Percent);
            _didReportProgress = true;

            return true;
        }
    }
}