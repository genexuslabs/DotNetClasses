using System;
using System.Collections.Generic;
using System.Text;

namespace GeneXus.Mail
{
    public interface ISMTPSession
    {

        void Login(GXSMTPSession session);

        void Send(GXSMTPSession session, GXMailMessage msg);
        void Logout(GXSMTPSession session);
    }
}
