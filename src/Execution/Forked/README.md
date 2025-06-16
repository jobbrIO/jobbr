# Jobbr Forked Process Execution

An alternative execution model that starts new processes and executes jobs in these different processes to increase stability.

[![GitHub Checks](https://img.shields.io/github/check-runs/jobbrIO/jobbr/master)](https://github.com/jobbrIO/jobbr/actions/workflows/ci.yml)
[![NuGet-Stable (Server)](https://img.shields.io/nuget/v/Jobbr.Execution.Forked?label=NuGet%20stable%20%28Extension%29)](https://www.nuget.org/packages/Jobbr.Execution.Forked)
[![NuGet-Stable (Runtime)](https://img.shields.io/nuget/v/Jobbr.Runtime.ForkedExecution?label=NuGet%20stable%20%28Runtime%29)](https://www.nuget.org/packages/Jobbr.Runtime.ForkedExecution)
[![NuGet Pre-Release](https://img.shields.io/nuget/vpre/Jobbr.Execution.Forked?label=NuGet%20pre%20%28Extension%29)](https://www.nuget.org/packages/Jobbr.Execution.Forked)
[![NuGet Pre-Release](https://img.shields.io/nuget/vpre/Jobbr.Runtime.ForkedExecution?label=NuGet%20pre%20%28Runtime%29)](https://www.nuget.org/packages/Jobbr.Runtime.ForkedExecution)

Jobbr supports multiple execution models as described **[here]**. The forked execution model is split on the execution extension on the Jobbr-Server and it's counterpart which needs to be included in a simple ConsoleApplication, often referenced as Runner.
Please see the **[wiki:concept]** or more details.

## Server Extension

It's assumed that the Jobbr-Server is already installed. The following sample shows the registration of the forked execution mode.

### NuGet

Install the NuGet `Jobbr.Execution.Forked` to the project where you host your Jobbr-Server.
The extension contains everything that is needed to offload jobs to a forked process.

```powershell
Install-Package Jobbr.Execution.Forked
```

### Registration

```c#
using Jobbr.Execution.Forked;

// ....

// Create a new builder which helps to setup your JobbrServer
var builder = new JobbrBuilder();

jobbrBuilder.AddForkedExecution(config =>
    {
        // Define the executable which runs the jobs. 
        config.JobRunnerExecutable = "Runner/Demo.JobRunner.exe";
    }
);

// Create a new instance of the JobbrServer
var server = builder.Create();

// Start
server.Start();
```

### Configuration

There are additional configuration options beside the required one above.

| Name                         | Description                                                                                                                                                     | Default      |
| ---------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ |
|`JobRunnerExecutable`         | Path to the executable that hosts a Jobbr Runtime and executes your jobs                                                                                        | **Required** |
|`JobRunDirectory`             | Specifies the folder in which the processes should start (subfolder will be created for each run based on the id)                                               | **Required** |
|`BackendAddress`              | Defines the URI Prefix to which the forked process should connect to. <br>**Note:** Should be set manually for production scenarios. to avoid firewall issues.  | Auto         |
|`MaxConcurrentProcesses`      | Maximum cuncurrent executables that should be forked at a time. Usually correlates with number of Cores                                                         | 4            |
|`IsRuntimeWaitingForDebugger` | If set to true, the executable will wait up to 10s before start (or a dabugger is detected), so that a debugger can be attached to the forked process           | `false`      |
|`AddJobRunnerArguments`       | Callback to pass additional parameters to the executable | `null` |

> **Note**: A more detailed explanation can be found the **[wiki:configuration]**

## Runner Installation

Creating a runner and setting up all the required dependencies for the runner is your job, a short reminder:
The forked execution model bases on an additional executable that executes your job.
This additional executable is not part of this package and is under your control.
You'll need to reference the related Job-Types in this application.
The only additional thing you need to do, is to include the NuGet Package `Jobbr.Runtime.ForkedExecution` in you application and start it.

```powershell
Install-Package Jobbr.Runtime.ForkedExecution
```

In your `Main()` method, make sure you instantiate a new JobbrRuntime and pass all command line arguments to the `Start()`-Method.

```c#
using Demo.MyJobs;
using Jobbr.Runtime.Logging;
using Jobbr.Runtime.ForkedExecution;

// ...

public static void Main(string[] args)
{
    // Redirect Log-Output to Trace, remove this if you install any other Log-Framework
    LogProvider.SetCurrentLogProvider(new TraceLogProvider());

    // Create the runtime
    var runtime = new ForkedRuntime();

    // Pass the arguments of the forked execution to the runtime
    runtime.Run(args);
}
```

### Configuration

If you want to configure the runtime, please pass an instance of a `RuntimeConfiguration` to the constructor.
The runtime supports the following configuration properties.

#### JobType Search Hint

Since the Job is registered by its CLR-Name, the runtime needs to find the related type before instantiating it.
The type is queried with the following strategies

1. Treat the name as full qualified and try to activate
2. Enumerate all types from the `JobTypeSearchAssembly` if provided and match against the Job name
3. Enumerate all currently loaded assemblies and try to find the job there
4. Load all referenced (and not yet loaded assemblies) and query again for the job

**Example**
```c#
// Define the assembly which contains the job
var jobAssemblyToQueryJobs = typeof(ProgressJob).Assembly;

var config = new RuntimeConfiguration { JobTypeSearchAssembly = jobAssemblyToQueryJobs };

var runtime = new ForkedRuntime(config);
```

#### Custom Dependency resolver

The default dependency resolver activates the type by using the default constructor.
If your job as additional dependencies, you might need to register a dependency resolver that implements the `IServiceProvider`-Interface.

**Example**
```c#
// Create a wrapper that implements the IServiceProvider interface and delegates the calls to the used DI
var serviceProvider = new MyDiContainerServiceProviderWrapper(new DIContainer());

var config = new RuntimeConfiguration { ServiceProvider = serviceProvider };
```

#### Access to the RuntimeContext

The runtime context contains properties for the current userId and DisplayName that has triggered the jobrun.
If you need access to this information, you need to implement the `IServiceProviderConfigurator` interface on your DiWrapper so that the runtime is able to register an instance of the RuntimeContext on your DI, which then can be injected to your job afterwards

# License

This software is licenced under GPLv3. See [LICENSE](LICENSE) and the related licences of 3rd party libraries below.

# Acknowledgements

Jobbr Server is based on following great open source projects:
* [AutoMapper](https://github.com/AutoMapper/AutoMapper]) [(MIT)](https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt)
* [NCrontab](https://github.com/atifaziz/NCrontab) [(Apache-2.0)](https://github.com/atifaziz/NCrontab/blob/master/COPYING.txt)
* [SimpleInjector](https://github.com/simpleinjector/SimpleInjector) [(MIT)](https://github.com/simpleinjector/SimpleInjector/blob/master/LICENSE)
* [TinyMessenger](https://github.com/grumpydev/TinyMessenger/blob/master/licence.txt) [(Ms-PL)](https://github.com/grumpydev/TinyMessenger/blob/master/licence.txt)

# Credits

This extension was built by the following awesome developers:
* [Michael Schnyder](https://github.com/michaelschnyder)
* [Oliver Zürcher](https://github.com/olibanjoli)
* [Mark Odermatt](https://github.com/mo85)
* [Roope Kivioja](https://github.com/RKivioja)
