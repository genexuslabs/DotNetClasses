using System;
using GeneXus.MSOffice.Excel.Style;

namespace GeneXus.MSOffice.Excel
{
	
	public class ExcelCellGXWrapper : IExcelCellRange
	{
		IExcelCellRange _value;
		public ExcelCellGXWrapper(IExcelCellRange value) {
			_value = value;
		}
		public ExcelCellGXWrapper()
		{
			System.Diagnostics.Debugger.Launch();
		}

		public int RowStart => _value.RowStart;

		public int RowEnd => _value.RowEnd;

		public int ColumnStart => _value.ColumnStart;


		public int ColumnEnd => _value.ColumnEnd;

		public string ValueType => _value.ValueType;

		public string Text { get => _value.Text; set => _value.Text=value; }
		public decimal NumericValue { get => _value.NumericValue; set => _value.NumericValue=value; }
		public DateTime DateValue { get => _value.DateValue; set => _value.DateValue=value; }

		public bool Empty()
		{
			return _value.Empty();
		}

		public string GetCellAdress()
		{
			return _value.GetCellAdress();
		}

		public ExcelStyle GetCellStyle()
		{
			return _value.GetCellStyle();
		}

		public bool MergeCells()
		{
			return _value.MergeCells();	
		}

		public bool SetCellStyle(ExcelStyle style)
		{
			return _value.SetCellStyle(style);
		}
	}
}
