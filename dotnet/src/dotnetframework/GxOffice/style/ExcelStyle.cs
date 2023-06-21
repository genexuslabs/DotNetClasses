using GeneXus.MSOffice.Excel.Style;

namespace GeneXus.MSOffice.Excel.style
{
	public class ExcelStyle : ExcelStyleDimension
	{
		private ExcelFill _cellFill;
		private ExcelFont _cellFont;
		private bool _locked;
		private bool _hidden;
		private bool _wrapText;
		private bool _shrinkToFit;
		private ExcelCellBorder _borders;
		private int _indentation = -1;
		private int _textRotation;
		private string _dataFormat;
		private ExcelAlignment _cellAlignment;

		public ExcelStyle()
		{
			_cellFill = new ExcelFill();
			_cellFont = new ExcelFont();
			_cellAlignment = new ExcelAlignment();
			_borders = new ExcelCellBorder();
		}

		public bool IsLocked()
		{
			return _locked;
		}

		public void SetLocked(bool value)
		{
			_locked = value;
		}

		public bool IsHidden()
		{
			return _hidden;
		}

		public void SetHidden(bool value)
		{
			_hidden = value;
		}

		public ExcelAlignment GetCellAlignment()
		{
			return _cellAlignment;
		}

		public ExcelFill GetCellFill()
		{
			return _cellFill;
		}

		public ExcelFont GetCellFont()
		{
			return _cellFont;
		}

		public override bool IsDirty()
		{
			return base.IsDirty() || _cellFill.IsDirty() || _cellFont.IsDirty() || _cellAlignment.IsDirty();
		}

		public bool GetWrapText()
		{
			return _wrapText;
		}

		public void SetWrapText(bool wrapText)
		{
			_wrapText = wrapText;
		}

		public bool GetShrinkToFit()
		{
			return _shrinkToFit;
		}

		public void SetShrinkToFit(bool shrinkToFit)
		{
			_shrinkToFit = shrinkToFit;
		}

		public int GetTextRotation()
		{
			return _textRotation;
		}

		public void SetTextRotation(int textRotation)
		{
			_textRotation = textRotation;
		}

		public ExcelCellBorder GetBorder()
		{
			return _borders;
		}

		public void SetBorder(ExcelCellBorder borders)
		{
			_borders = borders;
		}

		public int GetIndentation()
		{
			return _indentation;
		}

		public void SetIndentation(int indentation)
		{
			_indentation = indentation;
		}

		public string GetDataFormat()
		{
			return _dataFormat;
		}

		public void SetDataFormat(string dataFormat)
		{
			_dataFormat = dataFormat;
		}
	}
}
