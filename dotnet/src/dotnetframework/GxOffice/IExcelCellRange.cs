using System;
using GeneXus.MSOffice.Excel.style;

namespace GeneXus.MSOffice.Excel
{
	public interface IExcelCellRange
	{
		int GetRowStart();

		int GetRowEnd();

		int GetColumnStart();

		int GetColumnEnd();

		string GetCellAdress();

		string GetValueType();

		/*
         * 
         * D: for Date and DateTime 
         * C: Characteres 
         * N: Numerics
         * U: Unknown
         */
		string GetText();

		decimal GetNumericValue();

		DateTime GetDateValue();

		bool SetText(string value);

		bool SetNumericValue(decimal value);

		bool SetDateValue(DateTime value);

		bool Empty();

		bool MergeCells();

		bool SetCellStyle(ExcelStyle style);

		ExcelStyle GetCellStyle();
	}
}
