using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Artech.Genexus.SDAPI
{
    internal class AndroidNotifications
    {

        public static String getToken(String email, String password) {
		    // Create the post data
		    // Requires a field with the email and the password
		    StringBuilder builder = new StringBuilder();
		    builder.Append("Email=").Append(email);
		    builder.Append("&Passwd=").Append(password);
		    builder.Append("&accountType=GOOGLE");
		    builder.Append("&source=MyLittleExample");
		    builder.Append("&service=ac2dm");

		    // Setup the Http Post
            string postData = builder.ToString();
		    Uri uri = new Uri("https://www.google.com/accounts/ClientLogin");
		    HttpWebRequest request= (HttpWebRequest) WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;

            using(Stream writeStream = request.GetRequestStream())
            {
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = encoding.GetBytes(postData);
                writeStream.Write(bytes, 0, bytes.Length);
            }

		    // Read the response
            string result = null; 
            string line = string.Empty; 
            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader (responseStream, Encoding.UTF8))
                    {
                        while ( (line = readStream.ReadLine())!=null )
                        {
                            if (line.StartsWith("Auth=")) {
				                result = line.Substring(5);
			                }
                        }
                    }
                }
            }

		    // Finally get the authentication token
		    // To something useful with it
		    return result;
	    }

	    public static string PARAM_REGISTRATION_ID = "registration_id";
    
	    public static string PARAM_DELAY_WHILE_IDLE = "delay_while_idle";

        public static string PARAM_PRIORITY = "priority";

	    public static string PARAM_COLLAPSE_KEY = "collapse_key";

	    public static bool sendMessage(string auth_token, string registrationId,
            string message, string action, NotificationParameters props, out string log)
        {
            
		    StringBuilder postDataBuilder = new StringBuilder();
		    postDataBuilder.Append(PARAM_REGISTRATION_ID).Append("=")
			    	.Append(registrationId);
		    postDataBuilder.Append("&").Append(PARAM_COLLAPSE_KEY).Append("=")
			    	.Append("0");
            postDataBuilder.Append("&").Append(PARAM_PRIORITY).Append("=")
                   .Append("high");
		    postDataBuilder.Append("&").Append("data.payload").Append("=")
				    .Append( System.Web.HttpUtility.UrlEncode(message));
		    postDataBuilder.Append("&").Append("data.action").Append("=")
		        .Append(System.Web.HttpUtility.UrlEncode(action));

            //add parameters 
            postDataBuilder.Append("&").Append("data.parameters").Append("=")
                .Append(System.Web.HttpUtility.UrlEncode(props.ToJson()));


            String postData = postDataBuilder.ToString();

            UTF8Encoding encoding = new UTF8Encoding();
            byte[] bytes = encoding.GetBytes(postData);
        
		    // Hit the dm URL.
		    //Uri uri = new Uri("https://android.clients.google.com/c2dm/send");
            // new url for gcm service
            Uri uri = new Uri("https://android.googleapis.com/gcm/send");
                    

            HttpWebRequest request= (HttpWebRequest) WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8;";
            request.ContentLength = postData.Length;
            request.Headers["Authorization"] = "key=" + auth_token;
            //    HttpsURLConnection
		    //		.setDefaultHostnameVerifier(new CustomizedHostnameVerifier());
            //allows for validation of SSL certificates 
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(ValidateServerCertificate);

            using(Stream writeStream = request.GetRequestStream())
            {
                 writeStream.Write(bytes, 0, bytes.Length);
            }

            HttpStatusCode responseCode;
		    using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            {
                 responseCode = response.StatusCode;
            }
            log = "";
            if (responseCode != HttpStatusCode.OK)
                log = "Cannot send message to device " + responseCode.ToString();
	    		
		    return responseCode==HttpStatusCode.OK;
    	}

        //for testing purpose only, accept any dodgy certificate... 
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

    }
}
