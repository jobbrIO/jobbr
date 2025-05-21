﻿namespace Jobbr.ComponentModel.Management.Model
{
    public enum JobRunStates
    {
        /// <summary>
        /// The null :)
        /// </summary>
        Null,

        /// <summary>
        /// The JoBRun is queued unless a JobStarter starts a new executable
        /// </summary>
        Scheduled,

        /// <summary>
        /// The JobStarter has created a enviornment for the Job and copies a related files/data to the working directory
        /// </summary>
        Preparing,

        /// <summary>
        /// The JobStarted has started a new executable 
        /// </summary>
        Starting,

        /// <summary>
        /// The JobStarted has created a new environment and the executable has been started
        /// </summary>
        Started,

        /// <summary>
        /// The Executable itself has connected to the jobServer
        /// </summary>
        Connected,

        /// <summary>
        /// The Executable is running and connected to the jobserver
        /// </summary>
        Initializing,

        /// <summary>
        /// The logic has started to run
        /// </summary>
        Processing,

        /// <summary>
        /// The external code was run
        /// </summary>
        Finishing,

        /// <summary>
        /// Collecting the files
        /// </summary>
        Collecting,

        /// <summary>
        /// The job as executed sucessfully and the executer has cleaned up and terminated
        /// </summary>
        Completed,

        /// <summary>
        /// The job failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The JobRun has ben deleted in advance
        /// </summary>
        Deleted,

        /// <summary>
        /// The JobRun has been omitted. Eg job has been scheduled, Jobserver stopped (before the jobrun is executed) and After PlannedStartDateTime started again -> JobRun won't be started in that case but set to Omitted.
        /// </summary>
        Omitted
    }
}