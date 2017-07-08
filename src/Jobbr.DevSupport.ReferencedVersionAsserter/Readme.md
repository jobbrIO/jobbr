Referenced Version Asserter
===========================

Sole puprose of this library is to provide a unit-test that asserts the version if the version of referenced assamblies in the project with their corresponding version information in the nuspec.
This makes sure, that if a Jobbr-project references an assebly via NuGet, that the same NuGet package is also listed as reference in the related nuspec file.