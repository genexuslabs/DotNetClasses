# GeneXus Standard Classes for .NET and .NET Core
GeneXus Standard Classes for .NET and .NET Core generators.

## Modules

| Name  | Description
|---|---
| gxencrypt | Encryption classes based on Twofish algorithm 


# How to compile

## Requirements
- msbuild >= 15 or Visual Stuio 2019 or dotnet SDK >= 3.1 
- .Net framework 4.6 

# Instructions

## How to build all projects?
- ```dotnet build StandardClasses.sln```
or
- ```msbuild /t:restore;build StandardClasses.sln```

## How to build a specific project?
- ```dotnet build project.csproj```
- ```msbuild /t:restore;build project.csproj```

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
