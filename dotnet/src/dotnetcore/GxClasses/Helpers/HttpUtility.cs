using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace System.Web
{
	//
	// Summary:
	//     Provides access to individual files that have been uploaded by a client.
	public sealed class HttpPostedFile
	{
		IFormFile _file;

		public HttpPostedFile(IFormFile file)
		{
			_file = file;
		}
		//
		// Summary:
		//     Gets the size of an uploaded file, in bytes.
		//
		// Returns:
		//     The file length, in bytes.
		public long ContentLength { get { return _file.Length; } }
		//
		// Summary:
		//     Gets the MIME content type of a file sent by a client.
		//
		// Returns:
		//     The MIME content type of the uploaded file.
		public string ContentType { get { return _file.ContentType; } }
		//
		// Summary:
		//     Gets the fully qualified name of the file on the client.
		//
		// Returns:
		//     The name of the client's file, including the directory path.
		public string FileName { get { return _file.FileName; } }
		//
		// Summary:
		//     Gets a System.IO.Stream object that points to an uploaded file to prepare for
		//     reading the contents of the file.
		//
		// Returns:
		//     A System.IO.Stream pointing to a file.
		public Stream InputStream { get { return _file.OpenReadStream(); } }

		//
		// Summary:
		//     Saves the contents of an uploaded file.
		//
		// Parameters:
		//   filename:
		//     The name of the saved file.
		//
		// Exceptions:
		//   T:System.Web.HttpException:
		//     The System.Web.Configuration.HttpRuntimeSection.RequireRootedSaveAsPath property
		//     of the System.Web.Configuration.HttpRuntimeSection object is set to true, but
		//     filename is not an absolute path.
		public void SaveAs(string filename) {
			using (Stream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
			{
				_file.CopyTo(fs);
			}
		}

	}
}
