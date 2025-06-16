# Jobbr Execution InProcess

Components to execute Jobs within same process

[![GitHub Checks](https://img.shields.io/github/check-runs/jobbrIO/jobbr/master)](https://github.com/jobbrIO/jobbr/actions/workflows/ci.yml)
[![NuGet-Stable](https://img.shields.io/nuget/v/Jobbr.Execution.InProcess?label=NuGet%20stable)](https://www.nuget.org/packages/Jobbr.Execution.InProcess)
[![NuGet Pre-Release](https://img.shields.io/nuget/vpre/Jobbr.Execution.InProcess?label=NuGet%20pre)](https://www.nuget.org/packages/Jobbr.Execution.InProcess)

## Idea

Let Jobs run withing the current process where also the JobbrServer runs, so that no additional processes are required.

## Challenges

* Don't crash the process --> AppDomains
* How to identify written files/artefacts at job completion --> ?
* How to handle process updates (24%, 99%, etc.) --> Is Console available / Multiple Jobs?, IProgressUpdate interface?
* How can log-files be separated into different files? --> Add JobRUnId to LogContext? (How is that mapped?)

## Knowhow

* Static Instances are per AppDomain only
* Threads can be named
* ThreadLocal could also help?
