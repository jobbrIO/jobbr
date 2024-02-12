
# Jobbr MSSql Storage Provider [![Develop build status](https://img.shields.io/appveyor/ci/Jobbr/jobbr-storage-mssql/develop.svg?label=develop)](https://ci.appveyor.com/project/Jobbr/jobbr-storage-mssql)

This is a storage adapter implementation for the [Jobbr .NET JobServer](http://www.jobbr.io) to store job related information on MS SQL Servers. 
The Jobbr main repository can be found on [JobbrIO/jobbr-server](https://github.com/jobbrIO/jobbr-server).

[![Master build status](https://img.shields.io/appveyor/ci/Jobbr/jobbr-storage-mssql/master.svg?label=master)](https://ci.appveyor.com/project/Jobbr/jobbr-storage-mssql) 
[![NuGet-Stable](https://img.shields.io/nuget/v/Jobbr.Storage.MsSql.svg?label=NuGet%20stable)](https://www.nuget.org/packages/Jobbr.Storage.MsSql)  
[![Develop build status](https://img.shields.io/appveyor/ci/Jobbr/jobbr-storage-mssql/develop.svg?label=develop)](https://ci.appveyor.com/project/Jobbr/jobbr-storage-mssql) 
[![NuGet Pre-Release](https://img.shields.io/nuget/vpre/Jobbr.Storage.MsSql.svg?label=NuGet%20pre)](https://www.nuget.org/packages/Jobbr.Storage.MsSql) 

## Installation

First of all you'll need a working jobserver by using the usual builder as shown in the demos ([jobbrIO/demo](https://github.com/jobbrIO/demo)). In addition to that you'll need to install the NuGet Package for this extension.

### NuGet

```powershell
Install-Package Jobbr.Storage.MsSql
```

### Configuration

Since you already have a configured server, the registration of the MsSQL Storage Provider is quite easy. Actually you only need a working Database-Connection (A list of typical ConnectionStrings can be found on [https://www.connectionstrings.com/sql-server/](https://www.connectionstrings.com/sql-server/)

```c#
using Jobbr.Storage.MsSql;

/* ... */

var builder = new JobbrBuilder();

builder.AddMsSqlStorage(config =>
{
    // Your connection string
    config.ConnectionString = @"Server=.\SQLEXPRESS;Integrated Security=true;InitialCatalog=JobbrDemoTest;";

    // Configure your SqlDialect (2017 is set by default)
    configuration.DialectProvider = new SqlServer2017OrmLiteDialectProvider();

    // Create tables (is set by default to true)
    configuration.CreateTablesIfNotExists = true;

    // Define how long jobs, triggers & runs should be kept in the database (optional)
    configuration.Retention = TimeSpan.FromDays(365);
});

server.Start();
```

### Database-Schema

By default, the extension tries to create the tables if they are not present. You can disable this behaviour (see example above) and create the tables manually using the script located on [source/Jobbr.Storage.MsSql/CreateTables.sql](source/Jobbr.Storage.MsSql/CreateTables.sql).

![Diagram](https://raw.githubusercontent.com/jobbrIO/jobbr-storage-mssql/develop/docs/diagram.png)

# License

This software is licenced under GPLv3. See [LICENSE](LICENSE), and the related licences of 3rd party libraries below.

# Acknowledgements

This extension is built using the following great open source projects

* [OrmLite](https://github.com/ServiceStack/ServiceStack.OrmLite) 
  [(GNU Affero General Public License)](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/license.txt)

# Credits

This application was built by the following awesome developers:
* [Michael Schnyder](https://github.com/michaelschnyder)
* [Oliver Zürcher](https://github.com/olibanjoli)
