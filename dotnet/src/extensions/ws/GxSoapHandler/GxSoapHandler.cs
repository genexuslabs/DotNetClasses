using System;
using System.ServiceModel;
using GeneXus.Utils;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
public class GxSoapHandler
{
	public void Setup(string eoName, GxLocation loc, object serviceClient)
	{
		string Parameters = loc.Configuration.ToString();
		string[] Parameter = Parameters.Split(';');
		string IdentStr = Parameter[1];
		string DGICert = Parameter[2];
		string ClientCert = Parameter[3];
		string Clientpassword = Parameter[4];
		string ServUri = Parameter[5];

		if (eoName == "WS_eFactura")   // Name of the external object to filter, in this example EfacturaUy
		{
			//Replace ISdtWS_eFacturaDummy by the appropiate name which is contained in *.Programs.Common.dll, which must be added as a reference to the project
			ClientBase<ISdtWS_eFacturaDummy> svc = serviceClient as ClientBase<ISdtWS_eFacturaDummy>;
			
			svc.Endpoint.Contract.ProtectionLevel = System.Net.Security.ProtectionLevel.Sign;

			//Declare the objects to store the Certificates
			X509Certificate2 crtCLI;
			X509Certificate2 crtDGI;

			crtCLI = new X509Certificate2(ClientCert.Trim(), Clientpassword.Trim());
			crtDGI = new X509Certificate2(DGICert.Trim());

			//Add the certificates to the web service client
			svc.ClientCredentials.ClientCertificate.Certificate = crtCLI;
			svc.ClientCredentials.ServiceCertificate.DefaultCertificate = crtDGI;
			svc.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;


			//Create the WS address, which includes the URI and the DNS (the CN of the DGI certificate)
			svc.Endpoint.Address = new EndpointAddress(new Uri(ServUri.Trim()),EndpointIdentity.CreateDnsIdentity(IdentStr.Trim()));


		}
	}
	internal interface ISdtWS_eFacturaDummy
	{
	}
}