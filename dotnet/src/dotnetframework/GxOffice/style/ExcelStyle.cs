namespace GeneXus.MSOffice.Excel.Style
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

		public bool Locked
		{
			set { _locked = value; }
		}

		public bool IsHidden()
		{
			return _hidden;
		}

		public bool Hidden
		{
			set => _hidden = value;
		}

		public ExcelAlignment CellAlignment => _cellAlignment;

		public ExcelFill CellFill => _cellFill;

		public ExcelFont CellFont => _cellFont;

		public override bool IsDirty()
		{
			return base.IsDirty() || _cellFill.IsDirty() || _cellFont.IsDirty() || _cellAlignment.IsDirty();
		}

		public bool WrapText { get => _wrapText; set => _wrapText = value; }

		public bool ShrinkToFit { get => _shrinkToFit; set => _shrinkToFit = value; }

		public int TextRotation { get => _textRotation; set => _textRotation = value; }

		public ExcelCellBorder Border { get => _borders; set => _borders = value; }

		public int Indentation { get => _indentation; set => _indentation = value; }

		public string DataFormat { get => _dataFormat; set => _dataFormat = value; }
	}
}
