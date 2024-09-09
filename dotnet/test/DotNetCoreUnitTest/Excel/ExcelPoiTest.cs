using System;
using System.Collections.Generic;
using System.IO;
using GeneXus.Application;
using GeneXus.MSOffice.Excel;
using GeneXus.MSOffice.Excel.Poi.Xssf;
using GeneXus.MSOffice.Excel.Style;
using GeneXus.Utils;
using NPOI.HPSF;
using Xunit;

namespace DotNetUnitTest.Excel
{
	public class ExcelPoiTest
	{
		string basePath;
		public ExcelPoiTest()
		{
			basePath = GxContext.StaticPhysicalPath();

		}
		[Fact]
		public void TestNumberFormat1()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testNumberFormat1");
			excel.GetCells(1, 1, 1, 1).NumericValue=123.456M;
			excel.GetCells(2, 1, 1, 1).NumericValue = 1;
			excel.GetCells(3, 1, 1, 1).NumericValue = 100;

			excel.GetCells(4, 1, 1, 1).NumericValue = 123.456M;
			excel.Save();

		}
		[Fact]
		public void TestCellStyle1()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testCellStyle1");
			excel.SetColumnWidth(1, 100);
			excel.GetCells(2, 1, 1, 5).NumericValue = 123.456M;
			ExcelStyle newCellStyle = new ExcelStyle();
			newCellStyle.CellFont.Bold = true;
			excel.GetCells(2, 1, 1, 5).SetCellStyle(newCellStyle);

			bool ok = excel.Save();
			Assert.True(ok);
		}
		[Fact]
		public void TestCellStyle2()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testCellStyle2");
			excel.SetColumnWidth(1, 100);
			excel.GetCells(2, 1, 5, 5).NumericValue = 123.456M;
			ExcelStyle newCellStyle = new ExcelStyle();
			newCellStyle.			CellFont.			Bold = true;
			excel.GetCells(2, 1, 3, 3).SetCellStyle(newCellStyle);

			excel.Save();

		}

		[Fact]
		public void TestInsertSheets()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testInsertSheets");
			InsertSheet(excel);
		}

		[Fact]
		public void TestInsertSheetTwice()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testInsertSheetTwice");
			InsertSheet(excel);

			excel = Open("testInsertSheetTwice");
			bool ok = excel.InsertSheet("test1");
			Assert.False(ok);
			ok = excel.InsertSheet("test2");
			Assert.False(ok);
			Assert.True(excel.ErrCode != 0);
			ok = excel.InsertSheet("test1");
			Assert.False(ok);
			Assert.True(excel.ErrCode != 0);
			excel.Save();
			excel.Close();
		}

		private void InsertSheet(ExcelSpreadsheetGXWrapper excel)
		{

			bool ok = excel.InsertSheet("test1");
			Assert.True(ok);
			ok = excel.InsertSheet("test2");
			Assert.True(ok);
			ok = excel.InsertSheet("test1");
			Assert.False(ok);
			excel.Save();
			excel.Close();
		}

		[Fact]
		public void TestInsertDuplicateSheets()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testInsertDuplicateSheets");
			bool ok = excel.InsertSheet("test1");
			Assert.True(ok);
			ok = excel.InsertSheet("test1");
			Assert.False(ok);
			LogErrorCodes(excel);
			ok = excel.InsertSheet("test1");
			LogErrorCodes(excel);
			Assert.False(ok);
			excel.Save();
		}

		[Fact]
		public void TestActiveWorksheet()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testActiveWorksheet");
			excel.GetCells(2, 1, 5, 5).NumericValue = 123.456M;
			excel.InsertSheet("test1");

			excel.InsertSheet("test2");
			excel.InsertSheet("test3");
			excel.SetCurrentWorksheetByName("test2");
			excel.GetCells(2, 1, 5, 5).NumericValue=3;
			excel.Save();

		}

		[Fact]
		public void TestOpenAndSave()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testActive");
			try
			{
				excel.GetCells(2, 1, 5, 5).SetDate(new DateTime());
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
			}
			excel.Save();
		}
		[Fact]
		public void TestOpenAndSaveLocked()
		{
			string filePath = Path.Combine(basePath, "testLocked.xlsx");
			ExcelSpreadsheetGXWrapper newFile = Create("testLocked");
			newFile.Save();
			newFile.Close();

			try
			{
				using (FileStream fs = File.OpenWrite(filePath))
				{

					//Excel should be opened.
					ExcelSpreadsheetGXWrapper excel = Open("testLocked");
					Assert.Equal(7, excel.ErrCode);//"File is locked"
					try
					{
						excel.GetCells(2, 1, 5, 5).SetDate(new DateTime());
					}
					catch (Exception e)
					{
						Console.WriteLine(e.StackTrace);
					}
					excel.Save();
				}
			}
			catch (Exception) { }
		}

		[Fact]

		public void TestFolderNotExists()
		{
			string excel1 = Path.Combine(basePath, "notexistsFolder", "test-active");
			ExcelSpreadsheetGXWrapper excel = new ExcelSpreadsheetGXWrapper();
			excel.Open(excel1);

			try
			{
				excel.GetCells(2, 1, 5, 5).SetDate(new DateTime());
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
			}
			bool saved = excel.Save();

			Assert.False(saved);
			Assert.NotEqual(0, excel.ErrCode);
			Assert.NotEqual(string.Empty, excel.ErrDescription);
		}

		[Fact]

		public void TestWithoutExtensions()
		{
			string excel1 = Path.Combine(basePath, "testWithoutExtensions");
			EnsureFileDoesNotExists(excel1 + ".xlsx");
			ExcelSpreadsheetGXWrapper excel = new ExcelSpreadsheetGXWrapper();
			excel.Open(excel1);
			excel.InsertSheet("genexus0");
			excel.InsertSheet("genexus1");
			excel.InsertSheet("genexus2");

			List<ExcelWorksheet> wSheets = excel.GetWorksheets();
			Assert.True(wSheets.Count == 3);
			Assert.True(wSheets[0].Name == "genexus0");
			Assert.True(wSheets[1].Name == "genexus1");
			Assert.True(wSheets[2].Name == "genexus2");

			excel.Save();

		}

		[Fact]

		public void TestInsertSheet()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testInsertSheet");
			excel.InsertSheet("genexus0");
			excel.InsertSheet("genexus1");
			excel.InsertSheet("genexus2");

			List<ExcelWorksheet> wSheets = excel.GetWorksheets();
			Assert.True(wSheets.Count == 3);
			Assert.True(wSheets[0].Name == "genexus0");
			Assert.True(wSheets[1].Name == "genexus1");
			Assert.True(wSheets[2].Name == "genexus2");

			excel.Save();

		}


		[Fact]

		public void TestDeleteSheet()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testDeleteSheet");
			excel.InsertSheet("gx1");
			excel.InsertSheet("gx2");
			excel.InsertSheet("gx3");
			excel.InsertSheet("gx4");

			List<ExcelWorksheet> wSheets = excel.GetWorksheets();
			Assert.True(wSheets.Count == 4);
			Assert.True(wSheets[0].Name == "gx1");
			Assert.True(wSheets[1].Name == "gx2");
			Assert.True(wSheets[2].Name == "gx3");
			excel.DeleteSheet(2);
			wSheets = excel.GetWorksheets();
			Assert.True(wSheets[0].Name == "gx1");
			Assert.True(wSheets[1].Name == "gx3");
			excel.Save();

		}

		[Fact]

		public void TestSetCellValues()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testSetCellValues");
			excel.Autofit = true;
			excel.GetCells(1, 1, 1, 1).NumericValue = 100;
			excel.GetCells(2, 1, 1, 1).Text="hola!";
			excel.GetCells(3, 1, 1, 1).DateValue=new DateTime();
			excel.GetCells(4, 1, 1, 1).NumericValue = 66.78M;

			excel.Save();
			excel.Close();
			// Verify previous Excel Document
			excel = Open("testSetCellValues");

			Assert.Equal(100, excel.GetCells(1, 1, 1, 1).NumericValue);

			Assert.Equal("hola!", excel.GetCells(2, 1, 1, 1).Text);
			excel.Save();
		}

		[Fact]

		public void TestFormulas()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testFormulas");
			excel.Autofit=true;
			excel.GetCell(1, 1).NumericValue = 5;
			excel.GetCell(2, 1).NumericValue = 6;
			excel.GetCell(3, 1).Text = "=A1+A2";
			excel.Save();
			excel.Close();
			// Verify previous Excel Document
			excel = Open("testFormulas");

			Assert.Equal(11, excel.GetCell(3, 1).NumericValue);

			excel.Save();
		}


		[Fact]

		public void TestExcelCellRange()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testExcelCellRange");
			IExcelCellRange cellRange = excel.GetCells(2, 2, 5, 10);

			Assert.Equal(2, cellRange.ColumnStart);
			Assert.Equal(11, cellRange.ColumnEnd);
			Assert.Equal(2, cellRange.RowStart);
			Assert.Equal(6, cellRange.RowEnd);
			excel.Close();
		}


		[Fact]
		public void TestSetCurrentWorksheetByName()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testSetCurrentWorksheetByName");
			excel.InsertSheet("hoja1");
			excel.InsertSheet("hoja2");
			excel.InsertSheet("hoja3");
			excel.Save();
			excel.Close();
			excel = Open("testSetCurrentWorksheetByName");
			excel.SetCurrentWorksheetByName("hoja2");
			Assert.Equal("hoja2", excel.CurrentWorksheet.Name);
			excel.GetCell(5, 5).Text = "hola";
			excel.Save();
			excel.Close();


			excel = Open("testSetCurrentWorksheetByName");
			excel.SetCurrentWorksheetByName("hoja2");
			Assert.Equal("hola", excel.GetCell(5, 5).Text);

			excel.SetCurrentWorksheetByName("hoja1");
			Assert.Equal("", excel.GetCell(5, 5).Text);
			excel.Close();
		}

		[Fact]

		public void TestSetCurrentWorksheetByIdx()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testSetCurrentWorksheetByIdx");
			excel.InsertSheet("hoja1");
			excel.InsertSheet("hoja2");
			excel.InsertSheet("hoja3");
			excel.Save();
			excel.Close();
			excel = Open("testSetCurrentWorksheetByIdx");
			excel.SetCurrentWorksheet(2);
			Assert.Equal("hoja2", excel.CurrentWorksheet.Name);
			excel.GetCell(5, 5).Text = "hola";
			excel.Save();
			excel.Close();


			excel = Open("testSetCurrentWorksheetByIdx");

			bool ok = excel.SetCurrentWorksheet(2);
			Assert.Equal("hola", excel.GetCell(5, 5).Text);
			Assert.True(ok);

			ok = excel.SetCurrentWorksheet(1);
			Assert.True(ok);
			ok = excel.SetCurrentWorksheet(3);
			Assert.True(ok);
			ok = excel.SetCurrentWorksheet(4);
			Assert.False(ok);
			ok = excel.SetCurrentWorksheet(5);
			Assert.False(ok);
			ok = excel.SetCurrentWorksheet(0);
			Assert.False(ok);
			excel.Close();
		}


		[Fact]

		public void TestCopySheet()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testCopySheet");

			excel.InsertSheet("hoja1");
			excel.SetCurrentWorksheetByName("hoja1");
			excel.GetCells(1, 1, 3, 3).Text="test";
			excel.InsertSheet("hoja2");
			excel.InsertSheet("hoja3");
			excel.Save();
			excel.Close();
			excel = Open("testCopySheet");
			excel.SetCurrentWorksheetByName("hoja1");
			excel.			CurrentWorksheet.Copy("hoja1Copia");
			excel.Save();
			excel.Close();
			excel = Open("testCopySheet");
			excel.SetCurrentWorksheetByName("hoja1Copia");
			Assert.Equal("test", excel.GetCells(1, 1, 3, 3).Text);
			excel.Close();
		}

		[Fact]
		public void TestCopySheet2()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testCopySheet");

			excel.InsertSheet("hoja1");
			excel.SetCurrentWorksheetByName("hoja1");
			excel.GetCells(1, 1, 3, 3).Text="test";
			excel.InsertSheet("hoja2");
			excel.InsertSheet("hoja3");
			excel.Save();
			excel.Close();
			excel = Open("testCopySheet");
			excel.SetCurrentWorksheetByName("hoja1");
			excel.			CurrentWorksheet.Copy("hoja1Copia");
			excel.			CurrentWorksheet.Copy("hoja1Copia");
			excel.			CurrentWorksheet.Copy("hoja1Copia");
			excel.Save();
			excel.Close();
			excel = Open("testCopySheet");
			excel.SetCurrentWorksheetByName("hoja1Copia");
			Assert.Equal("test", excel.GetCells(1, 1, 3, 3).Text);
			excel.Close();
		}


		[Fact]

		public void TestGetWorksheets()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testGetWorksheets");
			excel.InsertSheet("hoja1");
			excel.InsertSheet("hoja2");
			excel.InsertSheet("hoja3");
			excel.InsertSheet("hoja4");
			excel.Save();
			excel.Close();
			excel = Open("testGetWorksheets");
			List<ExcelWorksheet> sheets = excel.GetWorksheets();
			Assert.Equal("hoja1", sheets[0].Name);
			Assert.Equal("hoja2", sheets[1].Name);
			Assert.Equal("hoja3", sheets[2].Name);
			Assert.Equal("hoja4", sheets[3].Name);
			excel.Close();
		}

		[Fact]

		public void TestHiddenCells()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testHiddenCells");

			excel.Autofit = true;
			excel.InsertSheet("hoja1");
			excel.SetCurrentWorksheetByName("hoja1");
			excel.			CurrentWorksheet.SetProtected("password");
			excel.GetCells(1, 1, 3, 3).Text = "texto no se puede editar";
			ExcelStyle style = new ExcelStyle();
			style.Hidden=true;
			excel.GetCells(1, 1, 3, 3).SetCellStyle(style);


			ExcelCells cells = excel.GetCells(5, 1, 3, 3);
			cells.Text = "texto SI se puede editar";
			style = new ExcelStyle();
			style.Locked=false;
			cells.SetCellStyle(style);
			excel.Save();
			excel.Close();
		}

		[Fact]

		public void TestProtectSheet()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testProtectSheet");
			excel.Autofit = true;
			excel.InsertSheet("hoja1");
			excel.SetCurrentWorksheetByName("hoja1");
			excel.			CurrentWorksheet.SetProtected("password");
			excel.GetCells(1, 1, 3, 3).Text="texto no se puede editar";
			ExcelStyle style = new ExcelStyle();
			style.Locked=true;
			excel.GetCells(1, 1, 3, 3).SetCellStyle(style);


			ExcelCells cells = excel.GetCells(5, 1, 3, 3);
			cells.Text="texto SI se puede editar";
			style = new ExcelStyle();
			style.Locked=false;
			cells.SetCellStyle(style);
			excel.Save();
			excel.Close();
		}

		private ExcelSpreadsheetGXWrapper Create(string fileName)
		{
			string excelPath = Path.Combine(basePath, fileName + ".xlsx");
			EnsureFileDoesNotExists(excelPath);
			FileInfo theDir = new FileInfo(basePath);
			if (!theDir.Exists)
			{
				Directory.CreateDirectory(basePath);
			}

			ExcelSpreadsheetGXWrapper excel = new ExcelSpreadsheetGXWrapper();
			excel.Open(excelPath);
			return excel;
		}

		private ExcelSpreadsheetGXWrapper Open(string fileName)
		{
			string excelPath = Path.Combine(basePath, fileName + ".xlsx");
			ExcelSpreadsheetGXWrapper excel = new ExcelSpreadsheetGXWrapper();
			excel.Open(excelPath);
			return excel;
		}

		[Fact]

		public void TestHideSheet()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testHideSheet");
			excel.Autofit=true;
			excel.InsertSheet("hoja1");
			excel.InsertSheet("hoja2");
			excel.InsertSheet("hoja3");
			excel.InsertSheet("hoja4");
			excel.InsertSheet("hoja5");
			excel.InsertSheet("hoja6");
			excel.SetCurrentWorksheetByName("hoja2");

			Assert.False(excel.CurrentWorksheet.Hidden);
			Assert.True(excel.CurrentWorksheet.Hidden = true);
			Assert.True(excel.CurrentWorksheet.Hidden);

			excel.SetCurrentWorksheet(3);
			Assert.True(excel.CurrentWorksheet.Hidden = true);

			excel.SetCurrentWorksheetByName("hoja1");
			excel.Save();
			excel.Close();
		}


		[Fact]

		public void TestCloneSheet()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testCloneSheet");
			excel.InsertSheet("hoja1");
			excel.GetCell(1, 1).Text = "1";
			excel.InsertSheet("hoja2");
			excel.GetCell(1, 1).Text = "2";
			excel.InsertSheet("hoja3");
			excel.CloneSheet("hoja2", "cloned_hoja2");
			excel.Save();
			excel.Close();
			excel = Open("testCloneSheet");
			List<ExcelWorksheet> sheets = excel.GetWorksheets();
			Assert.Equal(4, sheets.Count);
			excel.Close();
		}

		[Fact]

		public void TestCloneSheet2()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testCloneSheet2");
			excel.GetCell(2, 2).Text = "hello";
			bool ok = excel.CloneSheet(excel.CurrentWorksheet.Name, "clonedSheet");
			Assert.True(ok);
			excel.Save();
			excel.Close();
			excel = Open("testCloneSheet2");
			List<ExcelWorksheet> sheets = excel.GetWorksheets();
			Assert.Equal(2, sheets.Count);
			excel.Close();
		}

		[Fact]

		public void TestCloneSheetError()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testCloneSheetError");
			excel.InsertSheet("hoja1");
			excel.GetCell(1, 1).Text = "1";
			excel.InsertSheet("hoja2");
			excel.GetCell(1, 1).Text = "2";
			excel.InsertSheet("hoja3");
			excel.CloneSheet("hoja2", "cloned_hoja2");
			excel.CloneSheet("hoja2", "hoja2");

			excel.CloneSheet("hoja2", "hoja2");
			excel.CloneSheet("hoja2", "hoja2");
			Assert.True(excel.ErrCode > 0);
			excel.CloneSheet("hoja2", "hoja2");
			excel.Save();
			excel.Close();
			excel = Open("testCloneSheetError");
			List<ExcelWorksheet> sheets = excel.GetWorksheets();
			Assert.Equal(4, sheets.Count);
			excel.Close();
		}

		[Fact]

		public void TestWorksheetRename()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testWorksheetRename");
			excel.			CurrentWorksheet.Rename("defaultsheetrenamed");
			excel.InsertSheet("hoja1");
			excel.InsertSheet("hoja2");
			excel.InsertSheet("hoja3");
			excel.InsertSheet("hoja4");

			excel.Save();
			excel.Close();
			excel = Open("testWorksheetRename");
			excel.GetWorksheets()[3].Rename("modificada");
			excel.Save();
			excel.Close();
			excel = Open("testWorksheetRename");
			List<ExcelWorksheet> sheets = excel.GetWorksheets();
			Assert.Equal("hoja1", sheets[1].Name);
			Assert.Equal("hoja2", sheets[2].Name);
			Assert.Equal("modificada", sheets[3].Name);
			Assert.Equal("hoja4", sheets[4].Name);
			excel.Close();
		}

		[Fact]

		public void TestMergeCells()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testMergeCells");
			excel.GetCells(2, 10, 10, 5).MergeCells();
			excel.GetCells(2, 10, 10, 5).Text="merged cells";
			excel.Save();
			excel.Close();
		}

		[Fact]
		public void TestMergeMultipleCells()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testMergeCells-2");
			excel.GetCells(1, 1, 2, 5).MergeCells();
			excel.GetCells(1, 1, 2, 5).Text = "merged cells 1";

			excel.GetCells(5, 1, 2, 5).MergeCells();
			excel.GetCells(5, 1, 2, 5).Text = "merged cells 2";

			excel.GetCells(8, 1, 2, 5).MergeCells();
			excel.GetCells(8, 1, 2, 5).Text = "merged cells 3";

			excel.Save();
			excel.Close();
		}

		[Fact]
		public void TestMergeMultipleCellsIntersect()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testMergeCells-3");
			excel.GetCells(1, 1, 8, 5).MergeCells();
			excel.GetCells(1, 1, 8, 5).Text = "merged cells 1";

			excel.GetCells(5, 1, 8, 5).MergeCells();
			excel.GetCells(5, 1, 8, 5).Text = "merged cells 2";

			excel.Save();
			excel.Close();
		}


		[Fact]
		public void TestMergeNestedCells()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testMergeNestedCells");
			excel.GetCells(5, 5, 4, 4).MergeCells();
			excel.GetCells(5, 5, 4, 4).Text = "merged cells";
			excel.GetCells(1, 1, 10, 10).MergeCells();
			excel.Save();
			excel.Close();
		}

		[Fact]

		public void TestMergeCellsError()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testMergeCellsError");
			excel.GetCells(2, 10, 10, 5).MergeCells();
			excel.GetCells(2, 10, 10, 5).MergeCells();
			excel.GetCells(2, 10, 10, 5).MergeCells();
			excel.GetCells(3, 11, 2, 2).MergeCells();
			excel.GetCells(2, 10, 10, 5).MergeCells();

			excel.GetCells(2, 10, 10, 5).Text = "merged cells";
			excel.Save();
			excel.Close();
		}

		[Fact]

		public void TestColumnAndRowHeight()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testColumnAndRowHeight");
			excel.GetCells(1, 1, 5, 5).Text = "texto de las celdas largo";
			excel.SetRowHeight(2, 50);
			excel.SetColumnWidth(1, 100);
			excel.Save();
			excel.Close();
		}

		[Fact]

		public void TestAlignment()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testAlignment");
			excel.GetCells(2, 2, 3, 3).Text = "a";	
			ExcelStyle style = new ExcelStyle();
			style.			CellAlignment.			HorizontalAlignment = ExcelAlignment.HORIZONTAL_ALIGN_RIGHT; //center
			style.			CellAlignment.			VerticalAlignment = ExcelAlignment.VERTICAL_ALIGN_MIDDLE; //middle
			excel.GetCells(2, 2, 3, 3).SetCellStyle(style);
			excel.Save();
			excel.Close();

		}


		[Fact]

		public void TestExcelCellStyle()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testExcelCellStyle");

			IExcelCellRange cells = excel.GetCells(1, 1, 2, 2);

			ExcelStyle style = new ExcelStyle();

			cells.Text="texto muy largo";
			style.			CellAlignment.			HorizontalAlignment = 3;
			style.			CellFont.			Bold = true;
			style.			CellFont.			Italic = true;
			style.			CellFont.			Size = 18;
			style.			CellFont.			Color.SetColorRGB(1, 1, 1);
			style.			CellFill.			CellBackColor.SetColorRGB(210, 180, 140);
			style.			TextRotation = 5;

			style.
			WrapText = true;
			cells.SetCellStyle(style);
			excel.SetColumnWidth(1, 70);
			excel.SetRowHeight(1, 45);
			excel.SetRowHeight(2, 45);

			cells = excel.GetCells(5, 2, 4, 4);

			cells.Text = "texto2";
			style = new ExcelStyle();
			style.			Indentation = 5;
			style.			CellFont.			Size = 10;
			style.			CellFont.			Color.SetColorRGB(255, 255, 255);
			style.			CellFill.			CellBackColor.SetColorRGB(90, 90, 90);

			cells.SetCellStyle(style);


			cells = excel.GetCells(10, 2, 2, 2);
			cells.Text = "texto3";
			style = new ExcelStyle();
			style.			CellFont.			Bold = false;
			style.			CellFont.			Size = 10;
			style.			CellFont.			Color.SetColorRGB(180, 180, 180);
			style.			CellFill.			CellBackColor.SetColorRGB(45, 45, 45);
			style.			TextRotation = -90;
			cells.SetCellStyle(style);


			excel.Save();
			excel.Close();

		}


		[Fact]

		public void TestExcelBorderStyle()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testExcelBorderStyle");
			IExcelCellRange cells = excel.GetCells(5, 2, 4, 4);
			cells.Text = "texto2";

			ExcelStyle style = new ExcelStyle();
			style.			CellFont.			Size = 10;

			style.
			Border.
			BorderTop.
			Border = "THICK";
			style.			Border.			BorderTop.			BorderColor.SetColorRGB(220, 20, 60);

			style.
			Border.
			BorderDiagonalUp.
			Border = "THIN";
			style.			Border.			BorderDiagonalUp.			BorderColor.SetColorRGB(220, 20, 60);

			style.
			Border.
			BorderDiagonalDown.
			Border = "THIN";
			style.			Border.			BorderDiagonalDown.			BorderColor.SetColorRGB(220, 20, 60);

			cells.SetCellStyle(style);

			cells = excel.GetCells(10, 2, 2, 2);
			cells.Text = "texto3";
			style = new ExcelStyle();

			style.
			CellFont.
			Bold = false;
			style.			CellFont.			Size = 10;
			style.			CellFont.			Color.SetColorRGB(180, 180, 180);

			cells.SetCellStyle(style);


			excel.Save();
			excel.Close();

		}

		[Fact]

		public void TestNumberFormat()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testNumberFormat");
			ExcelStyle style = new ExcelStyle();
			style.			DataFormat = "#.##";
			style.			CellFont.			Bold = true;
			excel.GetCell(1, 1).NumericValue=1.123456789M;
			excel.GetCell(1, 1).SetCellStyle(style);
			excel.GetCell(2, 1).NumericValue=20000.123456789M;

			excel.Save();
			excel.Close();
		}

		[Fact]

		public void TestInsertRow()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testInsertRow");

			excel.GetCell(1, 1).NumericValue=1;
			excel.GetCell(2, 1).NumericValue=2;
			excel.GetCell(3, 1).NumericValue=3;
			excel.GetCell(4, 1).NumericValue=4;
			excel.GetCell(5, 1).NumericValue=5;
			excel.Save();
			excel.Close();
			// Verify previous Excel Document
			excel = Open("testInsertRow");

			Assert.Equal(2, excel.GetCell(2, 1).NumericValue);
			excel.InsertRow(2, 2);
			Assert.Equal(2, excel.GetCell(4, 1).NumericValue);
			excel.Save();
		}


		[Fact]

		public void TestDeleteRow()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testDeleteRow");

			excel.GetCells(1, 1, 1, 5).NumericValue=1;
			excel.GetCells(2, 1, 1, 5).NumericValue=2;
			excel.GetCells(3, 1, 1, 5).NumericValue = 3;
			excel.GetCells(4, 1, 1, 5).NumericValue = 4;
			excel.Save();
			excel.Close();
			// Verify previous Excel Document
			excel = Open("testDeleteRow");

			Assert.Equal(1, excel.GetCell(1, 1).NumericValue);
			Assert.Equal(2, excel.GetCell(2, 1).NumericValue);
			excel.DeleteRow(2);
			excel.Save();
			excel = Open("testDeleteRow");
			Assert.Equal(3, excel.GetCell(2, 1).NumericValue);
			excel.Save();
		}

		[Fact]

		public void TestDeleteRow2()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testDeleteRow2");

			excel.GetCell(2, 2).Text="hola";
			excel.Save();
			excel.Close();
			// Verify previous Excel Document
			excel = Open("testDeleteRow2");
			Assert.Equal("hola", excel.GetCell(2, 2).Text);
			bool result = excel.DeleteRow(1);
			Assert.True(result);
			excel.Save();
			excel.Close();
			excel = Open("testDeleteRow2");
			Assert.Equal("hola", excel.GetCell(1, 2).Text);
			excel.Save();
		}


		[Fact]

		public void TestHideRow()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testHideRow");

			excel.GetCell(1, 1).NumericValue = 1;

			excel.GetCell(2, 1).NumericValue = 2;

			excel.GetCell(3, 1).NumericValue = 3;

			excel.Save();
			excel.Close();
			// Verify previous Excel Document
			excel = Open("testHideRow");

			Assert.Equal(1, excel.GetCell(1, 1).NumericValue);
			excel.ToggleRow(2, false);
			//Assert.Equal(7, excel.GetCell(1, 1).GetNumericValue());
			excel.Save();
		}

		[Fact]
		public void TestHideRow2()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testHideRow2");
			excel.ToggleRow(2, false);
			excel.GetCell(1, 1).NumericValue=1;
			excel.GetCell(2, 1).NumericValue = 2;
			excel.GetCell(3, 1).NumericValue = 3;
			excel.Save();
			excel.Close();
			// Verify previous Excel Document
			excel = Open("testHideRow2");

			Assert.Equal(1, excel.GetCell(1, 1).NumericValue);
			excel.Save();
		}

		[Fact]
		public void TestHideRow3()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testHideRow3");
			excel.ToggleRow(2, false);
			excel.DeleteRow(5);
			excel.ToggleRow(7, true);
			excel.DeleteRow(8);
			excel.GetCell(1, 1).NumericValue = 1;
			excel.GetCell(2, 1).NumericValue = 2;
			excel.GetCell(3, 1).NumericValue = 3;
			excel.Save();
			excel.Close();
			// Verify previous Excel Document
			excel = Open("testHideRow3");

			Assert.Equal(1, excel.GetCell(1, 1).NumericValue);
			excel.Save();
		}

		[Fact]
		public void TestMixed()
		{
			ExcelSpreadsheetGXWrapper excel = Open("testMixed");

			excel.InsertRow(1, 5);
			excel.DeleteRow(2);

			excel.InsertSheet("Inserted Sheet");
			excel.ToggleRow(7, false);
			excel.ToggleColumn(2, false);

			excel.Save();
		}

		[Fact]

		public void TestHideColumn()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testHideColumn");

			excel.GetCell(1, 1).NumericValue = 1;
			excel.GetCell(2, 1).NumericValue = 1;
			excel.GetCell(3, 1).NumericValue = 1;

			excel.GetCell(1, 2).NumericValue = 2;
			excel.GetCell(2, 2).NumericValue = 2;
			excel.GetCell(3, 2).NumericValue = 2;

			excel.GetCell(1, 3).NumericValue = 3;
			excel.GetCell(2, 3).NumericValue = 3;
			excel.GetCell(3, 3).NumericValue = 3;

			excel.Save();
			excel.Close();
			// Verify previous Excel Document
			excel = Open("testHideColumn");

			Assert.Equal(1, excel.GetCell(2, 1).NumericValue);
			excel.ToggleColumn(2, false);
			//Assert.Equal(7, excel.GetCell(1, 1).GetNumericValue());
			excel.Save();
		}

		[Fact]

		public void TestDeleteColumn()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testDeleteColumn");

			excel.GetCell(1, 1).NumericValue = 1;
			excel.GetCell(2, 1).NumericValue = 1;
			excel.GetCell(3, 1).NumericValue = 1;

			excel.GetCell(1, 2).NumericValue = 2;
			excel.GetCell(2, 2).NumericValue = 2;
			excel.GetCell(3, 2).NumericValue = 2;

			excel.GetCell(1, 3).NumericValue = 3;
			excel.GetCell(2, 3).NumericValue = 3;
			excel.GetCell(3, 3).NumericValue = 3;

			excel.Save();
			excel.Close();
			// Verify previous Excel Document
			excel = Open("testDeleteColumn");

			Assert.Equal(2, excel.GetCell(2, 2).NumericValue);
			Assert.True(excel.DeleteColumn(2));
			Assert.Equal(3, excel.GetCell(2, 2).NumericValue);
			excel.Save();
		}

		[Fact]

		public void TestDeleteColumn2()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testDeleteColumn2");
			excel.DeleteColumn(2);
			excel.Save();
		}


		[Fact]
		public void TestDeleteColumn3()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testDeleteColumn3");
			excel.GetCells(1, 1, 5, 5).Text="cell";
			excel.InsertRow(3, 5);
			excel.DeleteRow(3);
			excel.DeleteColumn(2);
			excel.Save();
		}

		[Fact]

		public void TestSaveAs()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testSaveAs");
			excel.GetCells(1, 1, 15, 15).NumericValue=100;
			string excelNew = Path.Combine(basePath, "testSaveAsCopy.xlsx");
			excel.SaveAs(excelNew);
			excel.Close();
			Assert.True(new FileInfo(excelNew).Exists);

		}

		[Fact]
		public void TestAutoFit()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testAutoFit");
			excel.Autofit=true;
			excel.GetCells(1, 2, 1, 1).Text = "LONGTEXTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT";
			excel.GetCells(1, 3, 1, 1).Text = "VERYLONGTEXTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT";
			excel.GetCells(2, 4, 1, 1).Text = "hola!";
			excel.GetCells(6, 6, 1, 1).Text = "VERYLONGTEXTINDIFFERENTROWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW";
			ExcelCells cells = excel.GetCells(7, 7, 1, 1);
			ExcelStyle style = new ExcelStyle();
			style.			DataFormat = "#.##"; //change style, so it shows the full number not scientific notation
			cells.NumericValue=123456789123456789123456789M;
			cells.SetCellStyle(style);
			excel.Save();
			excel.Close();
		}
		[Fact]
		public void TestDateFormat()
		{
			ExcelSpreadsheetGXWrapper excel = Create("testDateFormat");
			excel.Autofit = true;
			DateTime date = new DateTime();
			//sets date with default format
			ExcelCells cells = excel.GetCells(1, 1, 1, 1);
			cells.DateValue=date;
			//sets date and apply format after
			cells = excel.GetCells(2, 1, 1, 1);
			ExcelStyle style = new ExcelStyle();
			cells.DateValue=date;
			style.			DataFormat = "YYYY/MM/DD hh:mm:ss";
			cells.SetCellStyle(style);
			//sets date and apply format before
			cells = excel.GetCells(3, 1, 1, 1);
			style = new ExcelStyle();
			style.			DataFormat = "YYYY/MM/DD hh:mm:ss";
			cells.SetCellStyle(style);
			cells.DateValue = date;

			date = DateTimeUtil.ResetTime(date);
			//sets date with default format without hours
			cells = excel.GetCells(4, 1, 1, 1);
			cells.DateValue = date;
			//sets date and apply format after
			cells = excel.GetCells(5, 1, 1, 1);
			style = new ExcelStyle();
			cells.DateValue = date;
			style.			DataFormat = "YYYY/MM/DD hh:mm:ss";
			cells.SetCellStyle(style);
			//sets date and apply format before
			cells = excel.GetCells(6, 1, 1, 1);
			style = new ExcelStyle();
			style.			DataFormat = "YYYY/MM/DD hh:mm:ss";
			cells.SetCellStyle(style);
			cells.DateValue = date;

			excel.Save();
			excel.Close();
		}
		[Fact]
		public void TestTemplate()
		{
			string excelPath = Path.Combine(basePath, "testTemplate.xlsx");
			string excelTemplatePath = Path.Combine(basePath, "template.xlsx");
			EnsureFileDoesNotExists(excelPath);
			EnsureFileDoesNotExists(excelTemplatePath);
			FileInfo theDir = new FileInfo(basePath);
			if (!theDir.Exists)
			{
				Directory.CreateDirectory(basePath);
			}
			ExcelSpreadsheetGXWrapper excel = new ExcelSpreadsheetGXWrapper();
			excel.Open(excelPath, excelTemplatePath);
			Assert.Equal(4, excel.ErrCode);//Template not found
			excel.Close();
		}

		private void LogErrorCodes(ExcelSpreadsheetGXWrapper excel)
		{
			// System.out.println(String.format("%s - %s", excel.GetErrCode(), excel.GetErrDescription()));
		}

		private void EnsureFileDoesNotExists(string path)
		{
			try
			{
				FileInfo file = new FileInfo(path);
				if (file.Exists)
				{
					file.Delete();
				}
			}
			catch (Exception)
			{

			}
		}
	}
}

