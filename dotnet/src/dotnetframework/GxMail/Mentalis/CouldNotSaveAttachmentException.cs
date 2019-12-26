using System;

namespace GeneXus.Mail
{
	/// <summary>
    /// Summary description for CouldNotSaveAttachmentException.
	/// </summary>
	public class CouldNotSaveAttachmentException : ApplicationException
	{
        public CouldNotSaveAttachmentException(Exception innerException)
			: base("Could not save attachment", innerException)
		{
		}
	}
}
