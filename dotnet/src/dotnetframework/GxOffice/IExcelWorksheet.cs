namespace GeneXus.MSOffice.Excel
{
	public interface IExcelWorksheet
	{
		string Name { get; }

		bool Hidden { get; }

		bool Rename(string newName);
		bool Copy(string newName);
		void SetProtected(string password);
	}
}
