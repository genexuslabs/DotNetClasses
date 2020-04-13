# ChangePublicKeyToken Tool

This is a tool which modifies assemblies replacing references for one with different strong name (in particular different public key token).


## Usage

```
ChangePublicKeyToken [-a <INPUT_ASSEMBLY>] [-d <INPUT_DIRECTORY>]

Given an assembly INPUT_ASSEMBLY with a public key token B, it searches for all the assemblies in a directory INPUT_DIRECTORY that reference INPUT_ASSEMBLY with
public key token Z (different from P) and replaces the public key token of the reference by P:

Example:

ChangePublicKeyToken -a C:\DotNetClasses\GxClasses.dll -d C:\Model\KB\Web\bin
  

  -v, --verbose            Set output to verbose messages.

  -a, --assembly           Required. The name of the assembly that changed its strong name.

  -d, --targetDirectory    Required. Specify the directory to search for assemblies that reference the assembly which
                           changed its strong name. These assemblies will be modified to link the new assembly strong
                           name.

  --help                   Display this help screen.

  --version                Display version information.
 
```

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
