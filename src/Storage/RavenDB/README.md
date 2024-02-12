
# Jobbr RavenDB Storage Provider [![Develop build status](https://img.shields.io/appveyor/ci/Jobbr/jobbr-storage-ravendb/develop.svg?label=develop)](https://ci.appveyor.com/project/Jobbr/jobbr-storage-ravendb)

This is a storage adapter implementation for the [Jobbr .NET JobServer](http://www.jobbr.io) to store job related information in RavenDB. 
The Jobbr main repository can be found on [JobbrIO/jobbr-server](https://github.com/jobbrIO).

[![Master build status](https://img.shields.io/appveyor/ci/Jobbr/jobbr-storage-ravendb/master.svg?label=master)](https://ci.appveyor.com/project/Jobbr/jobbr-storage-ravendb) 
[![NuGet-Stable](https://img.shields.io/nuget/v/Jobbr.Storage.RavenDB.svg?label=NuGet%20stable)](https://www.nuget.org/packages/Jobbr.Storage.RavenDB) 
[![Develop build status](https://img.shields.io/appveyor/ci/Jobbr/jobbr-storage-ravendb/develop.svg?label=develop)](https://ci.appveyor.com/project/Jobbr/jobbr-storage-ravendb) 
[![NuGet Pre-Release](https://img.shields.io/nuget/vpre/Jobbr.Storage.RavenDB.svg?label=NuGet%20pre)](https://www.nuget.org/packages/Jobbr.Storage.RavenDB)

## Installation

First of all you'll need a working jobserver by using the usual builder as shown in the demos ([jobbrIO/demo](https://github.com/jobbrIO/demo)). In addition to that you'll need to install the NuGet Package for this extension.

### NuGet

```powershell
Install-Package Jobbr.Storage.RavenDB
```

### Configuration

To configure the Jobbr server to use RavenDB, use the AddRavenDbStorage extension method on JobbrBuilder:

```c#
using Jobbr.Storage.RavenDB;

/* ... */

var builder = new JobbrBuilder();

builder.AddRavenDbStorage(config =>
{
    config.Url = "http://localhost:8080";
    config.Database = "Jobbr";
});

server.Start();
```

# License

This software is licenced under GPLv3. See [LICENSE](LICENSE), and the related licences of 3rd party libraries below.

# Acknowledgements

This extension is built using the following great open source projects

* [RavenDB](https://github.com/ravendb/ravendb) 

# Credits

This application was built by the following awesome developers:
* [Michael Schnyder](https://github.com/michaelschnyder)
* [Oliver Zürcher](https://github.com/olibanjoli)
