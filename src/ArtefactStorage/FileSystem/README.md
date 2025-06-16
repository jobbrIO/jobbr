# Jobbr FileSystem Storage Provider

This is a storage provider implementation for the [Jobbr .NET JobServer](http://www.jobbr.io) to store artefacts related from job runs on the filesystem in a folder of your choice. 

[![GitHub Checks](https://img.shields.io/github/check-runs/jobbrIO/jobbr/master)](https://github.com/jobbrIO/jobbr/actions/workflows/ci.yml)
[![NuGet-Stable](https://img.shields.io/nuget/v/Jobbr.ArtefactStorage.FileSystem?label=NuGet%20stable)](https://www.nuget.org/packages/Jobbr.ArtefactStorage.FileSystem)
[![NuGet Pre-Release](https://img.shields.io/nuget/vpre/Jobbr.ArtefactStorage.FileSystem?label=NuGet%20pre)](https://www.nuget.org/packages/Jobbr.ArtefactStorage.FileSystem)

## Installation

First of all you'll need a working jobserver by using the usual builder as shown in the demos ([jobbrIO/demo](https://github.com/jobbrIO/demo)).
In addition to that you'll need to install the NuGet Package for this extension.

### NuGet

```powershell
Install-Package Jobbr.ArtefactStorage.FileSystem
```

### Configuration

Since you already have a configured server, the registration of the provider is quite easy. See Example below

```c#
using Jobbr.ArtefactStorage.FileSystem;

/* ... */

var builder = new JobbrBuilder();

builder.AddFileSystemArtefactStorage(config =>
{
    // the only required property, can be relative, or an UNC-Path
    config.DataDirectory = @"D:\\ApplicationX\Data\JobRunStore";
});

server.Start();
```

> [!NOTE]
> UNC-Paths are generally possible if they are reliable but have not been tested yet.
> If you're seeking for a high available implementation, the [RavenFS extension](https://github.com/jobbrIO/jobbr/tree/master/src/ArtefactStorage/RavenFS) might be the better candidate for you. 

# License

This software is licenced under GPLv3. See [LICENSE](LICENSE), and the related licences of 3rd party libraries below.

# Acknowledgements

This extension is built using the following great open source projects

* [MimeTypeMap](https://github.com/samuelneff/MimeTypeMap) [(MIT)](https://github.com/samuelneff/MimeTypeMap/blob/master/LICENSE.txt)

# Credits

This application was built by the following awesome developers:
* [Michael Schnyder](https://github.com/michaelschnyder)
* [Oliver Zürcher](https://github.com/olibanjoli)
