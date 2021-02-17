$GetFileVersionOutput = dotnet msbuild $PSScriptRoot/../dotnet/Directory.Build.props /t:GetFileVersionForPackage
"$GetFileVersionOutput" -match "(?<=FileVersion:)(.*)" > $null
$GetFileVersionOutput = $Matches[0]

$NuGetPackageVersion = $GetFileVersionOutput

if ([System.Convert]::ToBoolean($Env:IsPrerelease)) {
  $IsMaster = $Env:GIT_REF -match 'master$'
  $VersionTag =  @("trunk", "stable")[$IsMaster]
  $Timestamp = (Get-Date).ToString("yyyyMMddHHmmss")
  $NuGetPackageVersion = $NuGetPackageVersion + "-" + $VersionTag + "." + $Timestamp
}

return $NuGetPackageVersion