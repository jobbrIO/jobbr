# Jobbr Storage Provider for LiteDB

This package provides a LiteDB-based storage implementation for the Jobbr .NET job server. LiteDB is a serverless document database that stores data in a single file, making it an excellent choice for smaller applications or when you want to avoid setting up a separate database server.

## Features

- **Serverless**: No need for a separate database server installation
- **Single file storage**: All data is stored in a single file on disk
- **ACID compliance**: Supports transactions and data integrity
- **High performance**: Fast read/write operations with built-in indexing
- **Zero configuration**: Works out of the box with minimal setup

## Installation

Install the package via NuGet:

```
dotnet add package Jobbr.Storage.LiteDB
```

## Usage

Configure Jobbr to use the LiteDB storage provider:

```csharp
var builder = new JobbrBuilder();

builder.AddLiteDbStorage(config =>
{
    config.ConnectionString = "jobbr.db"; // Path to the database file
    config.CreateDatabaseIfNotExists = true;
    config.Retention = TimeSpan.FromDays(30); // Optional: Clean up old job runs
});

var jobbr = builder.Create();
jobbr.Start();
```

## Configuration Options

### `ConnectionString`
- **Type**: `string`
- **Default**: `"jobbr.db"`
- **Description**: The path to the LiteDB database file. Can be a relative or absolute path.

### `CreateDatabaseIfNotExists`
- **Type**: `bool`
- **Default**: `true`
- **Description**: Whether to create the database file if it doesn't exist.

### `Retention`
- **Type**: `TimeSpan?`
- **Default**: `null`
- **Description**: Optional retention policy for cleaning up old job run data. If set, job runs older than this timespan will be automatically deleted.

### `RetentionEnforcementInterval`
- **Type**: `TimeSpan`
- **Default**: `TimeSpan.FromHours(12)`
- **Description**: How often the retention policy should be enforced when `Retention` is set.

## Performance Considerations

- The database file will grow over time as more jobs and job runs are stored
- Consider using the `Retention` setting to automatically clean up old data
- For high-throughput scenarios, you might want to consider SQL Server or RavenDB storage providers instead
- The database file should be stored on a fast disk (SSD recommended) for optimal performance

## File Location

By default, the database file (`jobbr.db`) will be created in the application's working directory. You can specify a different location using an absolute path:

```csharp
config.ConnectionString = @"C:\Data\Jobbr\jobbr.db";
```

Or use a relative path:

```csharp
config.ConnectionString = @"data\jobbr.db";
```

## Backup and Recovery

Since all data is stored in a single file, backup is straightforward:

1. Stop the Jobbr server
2. Copy the database file to your backup location
3. Restart the Jobbr server

For recovery, simply restore the database file and restart the application.

## Limitations

- Single file means all data must fit on a single disk
- Not suitable for distributed scenarios (use SQL Server or RavenDB for clustering)
- File locking means only one application instance can access the database at a time