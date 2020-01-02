using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;
using System.IO;
using System.Diagnostics;

namespace Artech.Genexus.SDAPI
{
    internal class IOSNotifications
    {
        internal static bool Send(string deviceToken, string apnMessage, string server, string p12path, string p12password, out string log)
        {
            log = string.Empty;
            try
            {
                bool err = PushMsg(server, p12path, deviceToken, apnMessage, p12password, out log);

                // Warning: feedback service returns an empty list in sandbox.
                if (string.IsNullOrEmpty(log))
                {
                    server = server.Replace("gateway", "feedback");
                    FeedbackTuple[] tuples = GetFeedback(server, p12path, p12password, out log);
                    if (!string.IsNullOrEmpty(log))
                    {
                        log += " (@GetFeedback)";
                    }
                    else
                    {
                        foreach (FeedbackTuple feedbackTuple in tuples)
                            if (deviceToken.Equals(feedbackTuple.DeviceToken))
                            {
                                log = "Application was uninstalled in device";
                                break;
                            }
                    }
                }
                return err;
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(log))
                    log += "\n";
                log += string.Format("Exception: {0}\n", e.Message);
                if (e.InnerException != null)
                    log += string.Format("Inner exception: {0}\n", e.InnerException.Message);
                log += "Unexpected fail";
                return false;
            }
            finally
            {
                if (!string.IsNullOrEmpty(log))
                    log = "PushMessage: " + log;
            }
        }

        private const int WRITE_TIMEOUT = 10000; // miliseconds
        private const int READ_TIMEOUT = 10000; // miliseconds
        private const int ERROR_RESPONSE_LENGTH = 1 + 1 + 4;
        private static bool PushMsg(string server, string p12path, string deviceToken, string apnMessage, string p12password, out string log)
        {
            bool err = false;

            string responseError = string.Empty;
            err = DoCall(server, p12path, 2195, sslStream =>
            {
                byte[] payload = GeneratePayload1(0, 0, deviceToken, apnMessage);
                sslStream.WriteTimeout = WRITE_TIMEOUT;
                sslStream.Write(payload, 0, payload.Length);
                sslStream.Flush();

                sslStream.ReadTimeout = READ_TIMEOUT;
                byte[] response = new byte[ERROR_RESPONSE_LENGTH];
                try
                {
                    if (sslStream.Read(response, 0, ERROR_RESPONSE_LENGTH) == ERROR_RESPONSE_LENGTH)
                        responseError = ResponseText(response);
                }
                catch (IOException) 
                { 
                
                } // ignore timeout exception
            },
            p12password,
            out log);
            if (string.IsNullOrEmpty(log))
                log = responseError;
            return err;
        }

        // 
        // Error-response format
        // Command
        //  |  Status
        // |8| |n| |Identifier|
        //  1   1       4
        private static string ResponseText(byte[] response)
        {
            Debug.Assert(response.Length == 6);

            // Command = 8
            Debug.Assert(response[0] == 8);

            // Identifier = 0
            if (BitConverter.IsLittleEndian)
                Array.Reverse(response, 2, 4);
            int identifier = BitConverter.ToInt32(response, 2);
            Debug.Assert(identifier == 0);

            // Status
            byte status = response[1];
            switch (status)
            {
                case 0: return "No errors encountered";
                case 1: return "Processing error";
                case 2: return "Missing device token";
                case 3: return "Missing topic";
                case 4: return "Missing payload";
                case 5: return "Invalid token size";
                case 6: return "Invalid topic size";
                case 7: return "Invalid payload size";
                case 8: return "Invalid token";
                case 255: return "None (unknown)";
                default: return string.Format("Unknown ({0})", status);
            }
        }

        private class FeedbackTuple
        {
            public DateTime Time; // when the application no longer exists on the device
            public string DeviceToken;
        }

        private const int FEEDBACK_TUPLE_LENGTH = 4 + 2 + 32;
        private static FeedbackTuple[] GetFeedback(string server, string p12path, string p12password, out string log)
        {
            List<FeedbackTuple> ftList = new List<FeedbackTuple>();
            DoCall(server, p12path, 2196, sslStream =>
            {
                sslStream.ReadTimeout = READ_TIMEOUT;
                byte[] feedback = new byte[FEEDBACK_TUPLE_LENGTH];
                while (sslStream.Read(feedback, 0, FEEDBACK_TUPLE_LENGTH) == FEEDBACK_TUPLE_LENGTH)
                    ftList.Add(ReadFeedbackTuple(feedback));
            }, 
            p12password,
            out log);
            return ftList.ToArray();
        }

        private static bool DoCall(string server, string p12path, int port, Action<SslStream> action, string p12password, out string log)
        {
            log = "";

            // Create a TCP/IP client socket.
            using (TcpClient client = new TcpClient())
            {
                client.Connect(server, port);
                try
                {
                    using (NetworkStream networkStream = client.GetStream())
                    {
                        networkStream.ReadTimeout = READ_TIMEOUT;
                        networkStream.WriteTimeout = WRITE_TIMEOUT;
                        X509Certificate2 clientCertificate = new X509Certificate2(p12path, p12password, X509KeyStorageFlags.MachineKeySet);
                        X509Certificate2Collection clientCertificateCollection = new X509Certificate2Collection(new X509Certificate2[1] { clientCertificate });
                        // Create an SSL stream that will close the client's stream.
                        SslStream sslStream = new SslStream(
                             networkStream,
                             false,
                             new RemoteCertificateValidationCallback(ValidateServerCertificate),
                             null
                             );

                        try
                        {
                            sslStream.AuthenticateAsClient(server, clientCertificateCollection, SslProtocols.Tls, true);
                        }
                        catch (Exception e)
                        {
                            log += string.Format("Exception: {0}\n", e.Message);
                            if (e.InnerException != null)
                                log += string.Format("Inner exception: {0}\n", e.InnerException.Message);
                            log += "Authentication failed";
                            return false;
                        }

                        try
                        {
                            action(sslStream);
                        }
                        catch (Exception e)
                        {
                            log += string.Format("Exception: {0}\n", e.Message);
                            if (e.InnerException != null)
                                log += string.Format("Inner exception: {0}\n", e.InnerException.Message);
                            log += "Read/Write failed";
                            return false;
                        }
                    }
                    return true;
                }
                finally
                {
                    client.Close();
                }
            }

        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        // Simple notification format
        // Command
        //  |  Token length (bigendian)      Payload length (bigendian)
        // |0| |0|32| |deviceToken (binary)| |n|n| |payload|
        //  1    2             32              2    <= 256
        private static byte[] GeneratePayload0(string deviceToken, string apnMessage)
        {
            MemoryStream memoryStream = new MemoryStream();

            // Command
            memoryStream.WriteByte(0);

            WritePayloadTokenMessage(memoryStream, deviceToken, apnMessage);
            return memoryStream.ToArray();
        }

        private static void WritePayloadTokenMessage(MemoryStream memoryStream, string deviceToken, string apnMessage)
        {            
            // Device token length
            byte[] tokenArray = Convert.FromBase64String(deviceToken);
            Debug.Assert(tokenArray.Length == 32);
            byte[] tokenLength = BitConverter.GetBytes((Int16)tokenArray.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(tokenLength);
            memoryStream.Write(tokenLength, 0, 2);

            // Device token
            memoryStream.Write(tokenArray, 0, tokenArray.Length);

            //Message bytes
            byte[] apnMessageBytes = System.Text.ASCIIEncoding.UTF8.GetBytes(apnMessage);

            // Message length
            byte[] apnMessageLength = BitConverter.GetBytes((Int16)apnMessageBytes.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(apnMessageLength);
            memoryStream.Write(apnMessageLength, 0, 2);

            // Write the message           
            memoryStream.Write(apnMessageBytes, 0, apnMessageBytes.Length);
        }

        // Simple notification format
        // Command                   Token length (bigendian)      Payload length (bigendian)
        // |1| |Identifier| |Expiry| |0|32| |deviceToken (binary)| |n|n| |payload|
        //  1       4           4      2             32              2    <= 256
        private static byte[] GeneratePayload1(int identifier, int expireSeconds, string deviceToken, string apnMessage)
        {
            MemoryStream memoryStream = new MemoryStream();

            // Command
            memoryStream.WriteByte(1);

            // Identifier
            byte[] indentifierArray = BitConverter.GetBytes(identifier);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(indentifierArray);
            memoryStream.Write(indentifierArray, 0, 4);

            // Expire
            if (expireSeconds <= 0)
                memoryStream.Write(new byte[] { 0, 0, 0, 0 }, 0, 4); // 0 means send immediately or discard it
            else
            {
                DateTime expireDateTime = DateTime.UtcNow.AddSeconds(expireSeconds);
                int time_t = (expireDateTime - new DateTime(1970, 1, 1)).Seconds;
                byte[] expireArray = BitConverter.GetBytes(identifier);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(indentifierArray);
                memoryStream.Write(indentifierArray, 0, 4);
            }

            WritePayloadTokenMessage(memoryStream, deviceToken, apnMessage);
            return memoryStream.ToArray();
        }

        //  time_t   Token length (bigendian)
        // |n|n|n|n| |0|32| |deviceToken (binary)|
        //     4       2            32
        private static FeedbackTuple ReadFeedbackTuple(byte[] feedback)
        {
            FeedbackTuple feedbackTuple = new FeedbackTuple();

            // time_t
            if (BitConverter.IsLittleEndian)
                Array.Reverse(feedback, 0, 4);
            int time_t = BitConverter.ToInt32(feedback, 0);
            feedbackTuple.Time = new DateTime(1970, 1, 1).AddSeconds(time_t);

            // Device token length
            if (BitConverter.IsLittleEndian)
                Array.Reverse(feedback, 4, 2);
            ushort tokenLength = BitConverter.ToUInt16(feedback, 4);
            Debug.Assert(tokenLength == 32);

            // Device token
            feedbackTuple.DeviceToken = Convert.ToBase64String(feedback, 6, 32);

            return feedbackTuple;
        }
    }
}
