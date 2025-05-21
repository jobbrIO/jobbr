# Jobbr

Jobbr is a .NET job server. It is designed to reduce artificial complexity when a job server is needed in a project.
Jobbr tries to get out of your way as much as possible so that you can focus on business logic.
Job implementations have no dependency to Jobbr which makes it easy to integrate Jobbr in any .NET project.

> [!IMPORTANT]
> This mono-repo continues the development of Jobbr while drastically reducing the overhead in development.
> All other Jobbr repositories have been merged into this repository and are now obsolete.

## Main Features

- Isolation of Jobs on process-level
- REST API to manage and trigger Jobs and watch the execution state
- Provides a typed client to consume the REST API
- Persists created files from running jobs in an artefact store.
- Supports CRON expressions for recurring triggers
- Embeddable in your own C# application (JobServer and Runner)
- Easily testable
- IOC for your jobs
- Extendable (jobstorage providers, artefact storage providers, execution etc)
- Progress tracking via stdout (`Console.WriteLine()`)

## QuickStart

The best way to get started is to check out the [demo repo](https://github.com/jobbrIO/demo) to see a running example of Jobbr.

## Implementing a job

Good news!
All your C#-Code is compatible with jobbr as long as the CLR-type can be instantiated and has at least a public `Run()`- Method.

```c#
public class SampleJob
{
    public void Run()
    {
        const int iterations = 15;

        for (var i = 0; i < iterations; i++)
        {
            Thread.Sleep(500);

            Console.WriteLine("##jobbr[progress percent='{0:0.00}']", (double)(i + 1) / iterations * 100); // optional: report progress
        }
    }
}
```

## Configuring job with triggers

To define jobs use the `AddJobs` extension method:

```c#
jobbrBuilder.AddJobs(repo =>
{
    // define one job with two triggers
    repo.Define("SampleJob", "Quickstart.SampleJob")
        .WithTrigger(DateTime.UtcNow.AddHours(2)) /* run once in two hours */
        .WithTrigger("* * * * *"); /* run every minute */
});
```

# License

This software is licenced under GPLv3, see the [LICENSE](LICENSE) file for more details.

## Credits

This application was built by the following awesome developers:

- [Michael Schnyder](https://github.com/michaelschnyder)
- [Oliver Zürcher](https://github.com/olibanjoli)
- [Peter Gfader](https://twitter.com/peitor)
- [Mark Odermatt](https://github.com/mo85)
- [Steven Giesel](https://github.com/linkdotnet)
- [David Fiebig](https://github.com/david-fiebig)
- [Martin Bundschuh](https://github.com/chuma2150)
- [Lukas Dürrenberger](https://github.com/eXpl0it3r)
- [Roope Kivioja](https://github.com/RKivioja)
- [Maria Huminiuc](https://github.com/mara-huminiuc)
