using System.Net.Mail;
using GeneXus.Mail.Smtp;
using Xunit;

namespace Mail
{
	public class MailTest
	{
		[Fact]
		public void TestValidateConnection()
		{
			using (SmtpClient client = new SmtpClient())
			{
				client.Port = 587;
				client.EnableSsl = true;
				client.Host = "smtp.gmail.com";
				client.UseDefaultCredentials = false;
				bool result = SmtpHelper.ValidateConnection(client, string.Empty, false);
				Assert.True(result);
			}
		}
	}
}