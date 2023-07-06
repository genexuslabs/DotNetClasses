using System;
using GeneXus.MSOffice.Excel.Style;

namespace GeneXus.MSOffice.Excel
{
	public interface IExcelCellRange
	{
		int RowStart { get; }

		int RowEnd { get; }

		int ColumnStart { get; }

		int ColumnEnd { get; }

		string GetCellAdress();

		string ValueType { get; }

		/*
         * 
         * D: for Date and DateTime 
         * C: Characteres 
         * N: Numerics
         * U: Unknown
         */
		string Text { get; set; }

		decimal NumericValue { get; set; }

		DateTime DateValue { get; set; }

		bool Empty();

		bool MergeCells();

		bool SetCellStyle(ExcelStyle style);

		ExcelStyle GetCellStyle();
	}
}
