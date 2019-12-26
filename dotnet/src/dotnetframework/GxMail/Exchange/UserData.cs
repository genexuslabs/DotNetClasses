using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using Microsoft.Exchange.WebServices.Data;

namespace GeneXus.Mail.Exchange
{
    public interface IUserData
    {
        ExchangeVersion Version { get; }
        string EmailAddress { get; }
        string Password { get; }
        Uri AutodiscoverUrl { get; set; }
    }

    public class UserData : IUserData
    {

        public UserData()
        {
            Version = ExchangeVersion.Exchange2010_SP2;
        }

        public ExchangeVersion Version
        {
            get;
            set;
        }
        public string EmailAddress
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public Uri AutodiscoverUrl
        {
            get;
            set;
        }
    }


}
