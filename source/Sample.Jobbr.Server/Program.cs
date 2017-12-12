﻿using Jobbr.Server.Builder;
using Jobbr.Server.ForkedExecution;
using Jobbr.Server.JobRegistry;
using Jobbr.Server.WebAPI;

namespace Sample.Jobbr.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string baseAddress = "http://localhost:1338/";

            var jobbrBuilder = new JobbrBuilder();
            jobbrBuilder.AddForkedExecution(config =>
            {
                config.JobRunDirectory = "c:/jobbr/temp";
                config.JobRunnerExecutable = "../../../Sample.JobRunner/bin/Debug/Sample.JobRunner.exe";
                config.MaxConcurrentProcesses = 1;
            });

            jobbrBuilder.AddJobs(repo =>
            {
                repo.Define(typeof(MinutelyJob).Name, typeof(MinutelyJob).FullName) // why no assembly overload?
                    .WithTrigger("* * * * *");
            });

            jobbrBuilder.AddWebApi(config => config.BackendAddress = baseAddress);
            //jobbrBuilder.AddDashboard();

            using (var jobbr = jobbrBuilder.Create())
            {
                jobbr.Start();
            }
        }
    }
}
