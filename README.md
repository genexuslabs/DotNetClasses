# GeneXus Standard Classes for .NET and .NET Framework
GeneXus Standard Classes for .NET and .NET Framework generators.

## Repo status
| Branch | Build | Security
|---|---|---
|master|[![Build](https://github.com/genexuslabs/DotNetClasses/actions/workflows/Build.yml/badge.svg)](https://github.com/genexuslabs/DotNetClasses/actions/workflows/Build.yml)|[![CodeQL](https://github.com/genexuslabs/DotNetClasses/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/genexuslabs/DotNetClasses/actions/workflows/codeql-analysis.yml)
|beta|[![Build](https://github.com/genexuslabs/DotNetClasses/actions/workflows/Build.yml/badge.svg?branch=beta)](https://github.com/genexuslabs/DotNetClasses/actions/workflows/Build.yml)|[![CodeQL](https://github.com/genexuslabs/DotNetClasses/actions/workflows/codeql-analysis.yml/badge.svg?branch=beta)](https://github.com/genexuslabs/DotNetClasses/actions/workflows/codeql-analysis.yml)

## Modules

| Name  | Description | Package Id
|---|---|---
| GxEncrypt | Classes common to .NET and .NET Framework related to encryption based on Twofish algorithm | GeneXus.Encrypt
| GxEncryptCMD | Command line tool that allows encryption and decryption of data. [Help](https://wiki.genexus.com/commwiki/servlet/wiki?45615) | GeneXus.EncryptCMD
| GxCryptography | Provide classes that support CryptoAsymmetricEncrypt and GXSymmetricEncryption data type | GeneXus.Cryptography
| GxCryptographyCommon | Contants and Exceptions classes common to GxCryptography and GxClasses | GeneXus.Cryptography.Common
| DynService.Core | Provide data types to support DynamoDB, OData and Fabric | GeneXus.DynService.Core(\*\*) 
| DynService.DynamoDB | Provide classes that support Amazon DynamoDB | GeneXus.DynService.DynamoDB(\*\*) 
| DynService.Fabric | Provide classes that support Hyperledger Fabric storage | GeneXus.DynService.Fabric(\*\*) 
| DynServiceOData | Provide classes that support Microsoft OData protocol | GeneXus.DynService.OData(\*\*) 
| GxClasses.Win | Provide classes to support windows operative system interaction such has MessageBox and Shell function | GeneXus.Classes.Win(\*\*) 
| GxClasses | Core classes related to data access, presentation views and data types used by generated code | GeneXus.Classes(\*)
| GxExcel | Provide classes that support Excel data type | GeneXus.Excel(\*)
| GxMail | Provide classes that support Mail data type | GeneXus.Mail(\*)
| GxMaps | Provide classes that support Map data type | GeneXus.Maps(\*)
| GxPdfReportsCS | Provides classes related to PDF manipulation | GeneXus.PdfReportsCS(\*)
| GxSearch | Provides classes related to full text search | GeneXus.Search(\*)
| GxWebSocket | Provides classes related to notifications | GeneXus.WebSockets(\*)
| StoreManager | Provides classes related to push notifications | GeneXus.StoreManager(\*)
| Artech.Genexus.SDAPI | Provides classes related to smart devices push notifications | GeneXus.SDAPI(\*)
| GxMemcached | Provides support for manipulation of the distributed memory object cached system Memcached | GeneXus.Memcached(\*)
| GxRedis | Provides support for manipulation of the distributed memory object cached system Redis | GeneXus.Redis(\*)
| GXAmazonS3 | Provides classes related to Amazon Simple Storage Service | GeneXus.AmazonS3(\*)
| GXAzureStorage | Provides classes related to Azure Store service | GeneXus.Azure(\*\*) 
| GXBluemix | Provides classes related to Bluemix platform | GeneXus.Bluemix(\*\*) 
| GXGoogleCloud | Provides classes related to Google Cloud Platform | GeneXus.Google.Cloud(\*\*) 
| GXOpenStack | Provides classes related to Open Stack Platform | GeneXus.OpenStack
| Reor | Executable utility to execute reorganization | GeneXus.Reorganization(\*)
| GxSetFrm | Executable utility to handle GXPRN.INI (reports configuration file) | GeneXus.SetFrm
| GxConfig | Executable utility to update web.config | GeneXus.Config
| GxDataInitialization | Executable utility to support dynamic transactions initialization at impact process | GeneXus.DataInitialization(\*)

(\*) For .NET add suffix ".Core" to Package Id

(\*\*) Package not available for .NET

## Repository Layout
This repository contains projects for .NET and .NET Framework. It is organized as follows:

```
.
├── .editorconfig (configuration for visual studio)
├── README.md (this file)
├── dotnet/ 
    ├── src/
        ├── dotnetcommon/ (Shared projects that build for both TargetFrameworks: dotnet and dotnetcore)
        ├── dotnetcore/ (.NET projects, several of these projects link sources from dotnetframework)
        └── dotnetframework/ (.NET Framework projects)
    ├── Directory.Build.props (default configuration for projects, imported early in the import order)
    ├── Directory.Build.targets (configuration for particular projects, imported late in the build order)
    ├── DotNetStandardClasses.sln (solution to build all the projects)
    └── StandardClasses.ruleset (Code analysis rulesets)
```

# How to build

## Requirements
- Visual Studio 2022 (17.8 or higher).
- .NET 6 & .NET 8 
- .NET Framework 4.7 DevPack

# Instructions
For the following steps must be executed from inside ```dotnet``` directory:
```c:\DotNetClasses>cd dotnet```

## How to build all projects?
- ```dotnet build DotNetStandardClasses.sln```

## How to build a specific project?
- ```dotnet build project.csproj```

## How to test your changes with a GeneXus installation?
- ```dotnet msbuild /p:TF=net462 /p:DeployDirectory=C:\KB\CSharpModel\web\bin DotNetStandardClasses.sln```

It compiles the solution and copies all the .NET Framework assemblies to the folder C:\KB\CSharpModel\web\bin.

- ```dotnet msbuild /p:TF=net8.0 /p:DeployDirectory=C:\KB\NetModel\web\bin DotNetStandardClasses.sln```

It compiles the solution and copies all the .NET 8 assemblies to the folder C:\KB\NetModel\web\bin.

- TF: target framework that will be deployed. Valid values are: `net462` (for GeneXus NET Framework generator) and `net8.0` (for GeneXus NET generator).
- DeployDirectory: specifies the target directory to copy assemblies.


## Advanced information

### Replacing standard classes mechanism
How to compile an assembly and replace it in a GeneXus generated application. 

Suppose you do a fix in GxClasses project. In order to get that fix in your generated application follow these steps:

1. Set AssemblyOriginatorKeyFile property in [Directory.Build.props](dotnet/Directory.Build.props) with the full path of your .snk file. It is required to set a strong name for the assembly.
	- A new .snk file can be created with the command [sn.exe](https://docs.microsoft.com/en-us/dotnet/framework/tools/sn-exe-strong-name-tool) -k keyPair.snk  
2. Build DotNetStandardClasses.sln and copy ```DotNetClasses\dotnet\src\dotnetframework\GxClasses\bin\Release\net462\GxClasses.dll``` to your ```<KB>\CSharpModel\web\bin directory```
3. Patch all the ```<KB>\CSharpModel\web\bin``` assemblies to reference the new GxClasses.dll. To do this run [UpdateAssemblyReference tool](dotnet/tools) with the following parameters
	```UpdateAssemblyReference.exe -a <KB>\CSharpModel\web\bin\GxClasses.dll -d <KB>\CSharpModel\web\bin```
	- To get UpdateAssemblyReference.exe build [UpdateAssemblyReference.sln](dotnet/tools/updateassemblyreference/UpdateAssemblyReference.sln)
4. Since GxClasses references other assemblies, it is needed to keep that references unchanged. So this command will patch the new GxClasses.dll to reference the original ones:

	```UpdateAssemblyReference.exe -a <KB>\CSharpModel\web\bin\GxCryptography.dll -d <KB>\CSharpModel\web\bin```
	```UpdateAssemblyReference.exe -a <KB>\CSharpModel\web\bin\GxCryptographyCommon.dll -d <KB>\CSharpModel\web\bin```
	```UpdateAssemblyReference.exe -a <KB>\CSharpModel\web\bin\GxEncrypt.dll -d <KB>\CSharpModel\web\bin```
5. Execute the web application.


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
