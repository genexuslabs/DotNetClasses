# GeneXus Standard Classes for .NET and .NET Core
GeneXus Standard Classes for .NET and .NET Core generators.

## Modules

| Name  | Description
|---|---
| GxEncrypt | Classes common to .NET and .NET Core related to encryption based on Twofish algorithm 
| DynService | Provide classes that support DynamoDB and OData access
| GxClasses.Win | Provide classes to support windows operative system interaction such has MessageBox and Shell function
| GxClasses | Core classes related to data access, presentation views and data types used by generated code
| GxExcel | Provide classes that support Excel data type
| GxMail | Provide classes that support Mail data type
| GxMaps | Provide classes that support Map data type
| GxPdfReportsCS | Provides classes related to PDF manipulation
| GxSearch | Provides classes related to full text search
| GxWebSocket | Provides classes related to notifications
| SDNetAPI | Provides classes related to push notifications
| GxMemcached | Provides support for manipulation of the distributed memory object cached system Memcached
| GxRedis | Provides support for manipulation of the distributed memory object cached system Redis
| GXAmazonS3 | Provides classes related to Amazon Simple Storage Service 
| GXAzureStorage | Provides classes related to Azure Store service 
| GXBluemix | Provides classes related to Bluemix platform
| GXGoogleCloud | Provides classes related to Google Cloud Platform 
| GXOpenStack | Provides classes related to Open Stack Platform
| Reor | Executable utility to execute reorganization
| GxSetFrm | Executable utility to handle GXPRN.INI (reports configuration file)
| GxConfig | Executable utility to update web.config
| GxDataInitialization | Executable utility to support dynamic transactions initialization at impact process

This repository contains projects for .NET and .NET CORE. It uses the following directory structure:

```
├── .editorconfig (configuration for visual studio)
├── README.md (this file)
├── dotnet/ 
    ├── src/
        ├── dotnetcommon/ (Shared projects that build for both TargetFrameworks: dotnet and dotnetcore)
        ├── dotnetcore/ (.NET Core projects, many of them share sources with dotnetframework projects)
        └── dotnetframework/ (.NET projects)
    ├── Directory.Build.props (default common configuration for all the projects, imported early in the import order)
    ├── Directory.Build.targets (configuration for particular projects, imported late in the build order)
    ├── DotNetStandardClasses.sln (solution to build all the projects)
    └── StandardClasses.ruleset (Code analysis rulesets)
```

# How to build

## Requirements
- msbuild >= 15 or Visual Stuio 2019 or dotnet SDK >= 3.1 
- .Net framework 4.6 

# Instructions

## How to build all projects?
- ```dotnet build DotNetStandardClasses.sln```
or
- ```msbuild /t:restore;build DotNetStandardClasses.sln```

## How to build a specific project?
- ```dotnet build project.csproj```
or
- ```msbuild /t:restore;build project.csproj```

## How to copy assemblies to build directory?
- ```dotnet msbuild /t:build;CopyAssemblies DotNetStandardClasses.sln```
or
- ```msbuild /t:build;CopyAssemblies DotNetStandardClasses.sln```

It copies the .NET assemblies to the folder build/*gxnet/bin* and .NET CORE assemblies to build/*gxnetcore/bin*

## License

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
