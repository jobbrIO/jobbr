﻿# Referenced Version Asserter

Sole purpose of this library is to provide a unit-test that asserts the version if the version of referenced assemblies in the project with their corresponding version information in the nuspec.
This makes sure, that if a Jobbr-project references an assembly via NuGet, that the same NuGet package is also listed as reference in the related nuspec file.