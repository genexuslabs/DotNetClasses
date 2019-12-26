using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneXus.Mail
{
    public interface IMailService
    {
        
        void Login(GXMailServiceSession sessionInfo);
        void Logout(GXMailServiceSession sessionInfo);
        void Skip(GXMailServiceSession sessionInfo);
        string GetNextUID(GXMailServiceSession session);
        void Send(GXMailServiceSession sessionInfo, GXMailMessage gxmessage);
        void Receive(GXMailServiceSession sessionInfo, GXMailMessage gxmessage);
        void GetMailMessage(GXMailServiceSession sessionInfo, string MsgId, bool dwnEntireMsg,  GXMailMessage gxmessage);
        void Delete(GXMailServiceSession sessionInfo, GXMailMessage gxmessage);
        short MarkAs(GXMailServiceSession sessionInfo, GXMailMessage gxmessage, bool isRead);
        void ChangeFolder(GXMailServiceSession sessionInfo, string folder);
        void SetProperty(string key, string value);
  
        string AttachDir { get; set; }
        int Count { get; set; }
        string ServerUrl { get; set; }
        int Port { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        int Timeout { get; set; }
        bool NewMessages { get; set; }

    }
}
