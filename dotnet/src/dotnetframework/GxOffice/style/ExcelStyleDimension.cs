namespace GeneXus.MSOffice.Excel.Style
{
	public abstract class ExcelStyleDimension
	{
		private bool isDirty = false;

		public virtual bool IsDirty()
		{
			return isDirty;
		}

		public void SetChanged()
		{
			isDirty = true;
		}
	}
}
