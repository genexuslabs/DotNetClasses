# GxSoapHandler

## Component for Rewriting Web Service Credentials

This component allows you to overwrite web service credentials at runtime using location data type.


## Overview
Starting with GeneXus v15 Upgrade 11, a new non-standard property, location.Configuration, has been introduced to facilitate dynamic configuration of web service parameters.

## Example Implementation
In GeneXus, you can configure and use the web service credentials as follows:

```
&location = GetLocation("WS_eFactura") 
&location.Configuration= "Name;IDENTITY;PATH_CERT_DGI; PATH_ CERT_CLIENT;PASSWORD;URI" 
// This property accepts a semicolon-separated string with the configuration details

// Load a query as a test for DGI
&pWS_eFacturaData.xmlData.FromString("")
&pWS_eFacturaDataResult = &WS_eFactura.EFACRECEPCIONSOBRE(&pWS_eFacturaData)
&longvar = &pWS_eFacturaDataResult.ToXml()
```


## Building the Assembly
To enable dynamic credential modification, create the GxSoapHandler.dll assembly as follows:

1. Include a reference to <Namespace>.Programs.Common.dll in GxSoapHandler.csproj and replace ISdtWS_eFacturaDummy in GxSoapHandler.cs by the correct web service name.

2. Build the assembly with the following command:
```
msbuild GxSoapHandler.csproj
```

3. Copy GxSoapHandler.dll to the web\bin folder.


## Configuration
Additionally, ensure that the following line is added to the web.config file to enable the new credential handler:


```
<add key="NativeChannelConfigurator" value="GxSoapHandler" />
```

