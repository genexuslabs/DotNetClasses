using System;
using System.Text;

namespace GxClasses.Helpers
{
	public class BasicAuthenticationHeaderValue
	{
		const char UserNamePasswordSeparator= ':';
		public BasicAuthenticationHeaderValue(string authenticationHeaderValue)
		{
			if (!string.IsNullOrWhiteSpace(authenticationHeaderValue))
			{
				_authenticationHeaderValue = authenticationHeaderValue;
				if (TryDecodeHeaderValue())
				{
					ReadAuthenticationHeaderValue();
				}
			}
		}

		private readonly string _authenticationHeaderValue;
		private string _usernamePassword;

		public bool IsValidBasicAuthenticationHeaderValue { get; private set; }
		public string UserIdentifier { get; private set; }
		public string UserPassword { get; private set; }

		private bool TryDecodeHeaderValue()
		{
			const int headerSchemeLength = 6; // "Basic ".Length;
			if (_authenticationHeaderValue.Length <= headerSchemeLength)
			{
				return false;
			}
			string encodedCredentials = _authenticationHeaderValue.Substring(headerSchemeLength);
			try
			{
				byte[] decodedCredentials = Convert.FromBase64String(encodedCredentials);
				_usernamePassword = Encoding.ASCII.GetString(decodedCredentials);
				return true;
			}
			catch (FormatException)
			{
				return false;
			}
		}

		private void ReadAuthenticationHeaderValue()
		{
			IsValidBasicAuthenticationHeaderValue = !string.IsNullOrEmpty(_usernamePassword) && _usernamePassword.Contains(UserNamePasswordSeparator);
			if (IsValidBasicAuthenticationHeaderValue)
			{
				int separatorIndex = _usernamePassword.IndexOf(UserNamePasswordSeparator);
				UserIdentifier = _usernamePassword.Substring(0, separatorIndex);
				if (separatorIndex + 1 < _usernamePassword.Length)
				{
					UserPassword = _usernamePassword.Substring(separatorIndex + 1);
				}
				else
				{
					UserPassword = string.Empty;
				}
			}
		}
	}
}
