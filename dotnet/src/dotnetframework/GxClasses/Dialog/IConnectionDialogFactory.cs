using System.Runtime.InteropServices;

namespace ConnectionBuilder
{
	[GuidAttribute("6F0A0D61-E98E-41a2-9F72-A6A0A5BCF413")]
	public interface IConnectionDialogFactory
	{
		IConnectionDialog GetConnectionDialog(int iType);
	}

	[GuidAttribute("6F0A0D62-E98E-41a2-9F72-A6A0A5BCF413")]
	public interface IConnectionDialog
	{
		bool Show(string initString);
		string ConnectionString { get; }
	}

}
