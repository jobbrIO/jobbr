using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Jobbr.Runtime.ForkedExecution.RestClient;
using Microsoft.Extensions.Logging;

namespace Jobbr.Runtime.ForkedExecution
{
    /// <summary>
    /// Forked runtime.
    /// </summary>
    public class ForkedRuntime
    {
        private readonly ILogger<ForkedRuntime> _logger;
        private readonly CoreRuntime _coreRuntime;
        private ForkedExecutionRestClient _forkedExecutionRestClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForkedRuntime"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="runtimeConfiguration">Runtime configuration.</param>
        /// <exception cref="ArgumentNullException">Runtime configuration is null.</exception>
        public ForkedRuntime(ILoggerFactory loggerFactory, RuntimeConfiguration runtimeConfiguration)
        {
            if (runtimeConfiguration == null)
            {
                throw new ArgumentNullException(nameof(runtimeConfiguration));
            }

            _logger = loggerFactory.CreateLogger<ForkedRuntime>();

            _coreRuntime = new CoreRuntime(loggerFactory, runtimeConfiguration);

            // Wire Events to publish status
            _coreRuntime.Initializing += (sender, args) => HandleEvent("Initializing", () => _forkedExecutionRestClient?.PublishState(JobRunStates.Initializing));
            _coreRuntime.Starting += (sender, args) => HandleEvent("Starting", () => _forkedExecutionRestClient?.PublishState(JobRunStates.Processing));

            _coreRuntime.Ended += (sender, args) => HandleEvent("Ended", () => CoreRuntimeOnEnded(args));
        }

        /// <summary>
        /// Execute runtime.
        /// </summary>
        /// <param name="args">Execution arguments.</param>
        public void Run(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            _logger.LogInformation("ForkedRuntime started at {now} (UTC) with cmd-arguments {arguments}", DateTime.UtcNow, string.Join(" ", args));

            var cmdlineOptions = ParseArguments(args);

            _logger.LogInformation("JobRunId: {jobRunId}", cmdlineOptions.JobRunId);
            _logger.LogInformation("JobServer: {jobServer}", cmdlineOptions.JobServer);
            _logger.LogInformation("IsDebug: {isDebug}", cmdlineOptions.IsDebug);

            WaitForDebugger(cmdlineOptions.IsDebug);

            // Create client
            var jobbrRuntimeClient = new ForkedExecutionRestClient(cmdlineOptions.JobServer, cmdlineOptions.JobRunId);
            _forkedExecutionRestClient = jobbrRuntimeClient;

            jobbrRuntimeClient.PublishState(JobRunStates.Connected);

            var jobRunInfoDto = _forkedExecutionRestClient.GetJobRunInfo();

            var jobRunInfo = new ExecutionMetadata
            {
                JobType = jobRunInfoDto.JobType,
                JobParameter = jobRunInfoDto.JobParameter,
                InstanceParameter = jobRunInfoDto.InstanceParameter,
                UserId = jobRunInfoDto.UserId,
                UserDisplayName = jobRunInfoDto.UserDisplayName
            };

            _coreRuntime.Execute(jobRunInfo);
        }

        private static CommandlineOptions ParseArguments(IEnumerable<string> args)
        {
            return Parser.Default.ParseArguments<CommandlineOptions>(args).Value;
        }

        private static void WaitForDebugger(bool isDebugEnabled)
        {
            if (isDebugEnabled)
            {
                var beginWaitForDebugger = DateTime.Now;
                var endWaitForDebugger = beginWaitForDebugger.AddSeconds(10);
                var pressedEnter = false;

                Console.WriteLine(string.Empty);
                Console.WriteLine(">>> DEBUG-Mode is enabled. You have 10s to attach a Debugger");
                Console.Write("    or press enter to continue. Counting...");

                new TaskFactory().StartNew(
                    () =>
                    {
                        Console.ReadLine();
                        pressedEnter = true;
                    });

                while (!(pressedEnter || Debugger.IsAttached || endWaitForDebugger < DateTime.Now))
                {
                    Thread.Sleep(500);
                }
            }

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            _logger.LogError((Exception)unhandledExceptionEventArgs.ExceptionObject, "Unhandled Infrastructure Exception in Jobbr-Runtime. Please contact the developers!");
        }

        private void CoreRuntimeOnEnded(ExecutionEndedEventArgs executionEndedEventArgs)
        {
            if (!executionEndedEventArgs.Succeeded)
            {
                // Indicate failure also via exit code
                Environment.ExitCode = 1;
            }

            _forkedExecutionRestClient.PublishState(JobRunStates.Finishing);

            // Are there any files to collect?
            var allFiles = Directory.GetFiles(Directory.GetCurrentDirectory());

            if (allFiles.Any())
            {
                _forkedExecutionRestClient.PublishState(JobRunStates.Collecting);
                _forkedExecutionRestClient.SendFiles(allFiles);
            }

            _forkedExecutionRestClient.PublishState(executionEndedEventArgs.Succeeded ? JobRunStates.Completed : JobRunStates.Failed);
        }

        private void HandleEvent(string eventName, Action eventHandler)
        {
            try
            {
                eventHandler();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while handling event {eventName}.");
            }
        }
    }
}
