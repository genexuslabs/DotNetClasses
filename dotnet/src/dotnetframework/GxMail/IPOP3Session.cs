using System;
using System.Collections.Generic;
using System.Text;

namespace GeneXus.Mail
{
    public interface IPOP3Session
    {
        int GetMessageCount();
        void Login(GXPOP3Session sessionInfo);
        void Logout(GXPOP3Session sessionInfo);
        void Skip(GXPOP3Session sessionInfo);
        string GetNextUID(GXPOP3Session session);
        void Receive(GXPOP3Session sessionInfo, GXMailMessage gxmessage);
        void Delete(GXPOP3Session sessionInfo);

        string AttachDir { get; set; }   
        string Host { get; set; }
        int Port { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        int Timeout { get; set; }		
		string AuthenticationMethod { get; set; }

	}
}
