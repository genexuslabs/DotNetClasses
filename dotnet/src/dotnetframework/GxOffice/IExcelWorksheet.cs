namespace GeneXus.MSOffice.Excel
{
	public interface IExcelWorksheet
	{
		string GetName();
		bool IsHidden();
		bool Rename(string newName);
		bool Copy(string newName);
		void SetProtected(string password);
	}
}
