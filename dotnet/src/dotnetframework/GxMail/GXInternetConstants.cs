using System;

namespace GeneXus.Mail
{
	
	public abstract class GXInternetConstants
	{
		public const string BASE64 = "base64";
		public const string QUOTED_PRINTABLE = "quoted-printable";

		public const string DATE = "DATE";
		public const string FROM = "FROM";
		public const string TO = "TO";
		public const string CC = "CC";
		public const string BCC = "BCC";
		public const string SUBJECT = "Subject";
		public const string PRIORITY = "X-Priority";
		public const string ORGANIZATION = "Organization";
		public const string REPLY_TO = "REPLY-TO";
	
		public const string CONTENT_TRANSFER_ENCODING 	= "Content-Transfer-Encoding";
		public const string CONTENT_DISPOSITION			= "Content-Disposition";
		public const string CONTENT_TYPE 			 		= "Content-type";

		public const string BOUNDARY				    	= "boundary";
		public const string TEXT 							= "text";
		public const string FILENAME 						= "filename";
		public const string NAME 							= "name";
		public const string ATTACHMENT 					= "attachment";

		public const string TYPE_MULTIPART 				= "multipart";
		public const string SUBTYPE_ALTERNATIVE 			= "alternative";

		public const int MAIL_Ok =  0;      				
		public const int MAIL_AlreadyLogged =  1;      	
		public const int MAIL_NotLogged =  2;     		
		public const int MAIL_CantLogin =  3;      		
		public const int MAIL_CantOpenOutlook =  4;     	
		public const int MAIL_CantOpenFolder =  5;      	
		public const int MAIL_InvalidSenderName =  6;   	
		public const int MAIL_InvalidSenderAddress =  7;	
		public const int MAIL_InvalidUser=  8;      		
		public const int MAIL_InvalidPassword =  9;     	
		public const int MAIL_MessageNotSent = 10;      	
		public const int MAIL_NoMessages = 11;      		
		public const int MAIL_CantDeleteMessage = 12;   	
		public const int MAIL_NoRecipient = 13;     		
		public const int MAIL_InvalidRecipient = 14;    	
		public const int MAIL_InvalidAttachment = 15;   	
		public const int MAIL_CantSaveAttachment = 16;  	
		public const int MAIL_InvalidValue = 17;      	
		public const int MAIL_ConnectionLost = 19;      	
		public const int MAIL_TimeoutExceeded = 20;     	
		public const int MAIL_ErrorReceivingMessage = 22;	
		public const int MAIL_NoAuthentication = 23;      
		public const int MAIL_AuthenticationError = 24;   
		public const int MAIL_PasswordRefused = 25;      	
        public const int MAIL_SMTPOverSSLNotSupported = 26;      	
        public const int MAIL_EmailId_NotFound = 27;      	
	
		public const int MAIL_ServerReplyInvalid = 50;
		public const int MAIL_ServerRepliedErr = 51;  

		public const int MAIL_InvalidMode = 100;      	
	}
}
