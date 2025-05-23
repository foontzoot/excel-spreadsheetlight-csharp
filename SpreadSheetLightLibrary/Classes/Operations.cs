﻿using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;
using Color = System.Drawing.Color;
#pragma warning disable CS8602

namespace SpreadSheetLightLibrary.Classes;

public class Operations
{
    /// <summary>
    /// Create a new Excel file, rename the default sheet from
    /// Sheet1 to the value in pSheetName
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    public bool CreateNewFile(string fileName, string sheetName)
    {
        using SLDocument document = new();

        try
        {
            document.RenameWorksheet("Sheet1", sheetName);
            document.SaveAs(fileName);
            return true;
        }
        catch (Exception)
        {

            return false;
        }
    }
    /// <summary>
    /// Create a new Excel file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public bool CreateNewFile(string fileName)
    {
        using SLDocument document = new();
        
        document.SaveAs(fileName);

        return true;
    }

    /// <summary>
    /// Add a new sheet if it does not currently exist.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    public bool AddNewSheet(string fileName, string sheetName)
    {
        using SLDocument document = new(fileName);

        if (!(document.GetSheetNames(false)
                .Any((workSheetName) => 
                    string.Equals(workSheetName, sheetName, StringComparison.CurrentCultureIgnoreCase))))
        {
            document.AddWorksheet(sheetName);
            document.Save();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Remove a sheet if it exists.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    public bool RemoveWorkSheet(string fileName, string sheetName)
    {
        using SLDocument document = new(fileName);
        var workSheets = document.GetSheetNames(false);
        if (workSheets.Any((workSheetName) => string.Equals(workSheetName, sheetName, StringComparison.CurrentCultureIgnoreCase)))
        {
            if (workSheets.Count > 1)
            {
                document.SelectWorksheet(document.GetSheetNames().FirstOrDefault((sName) => 
                    sName.ToLower() != sheetName.ToLower()));
            }
            else if (workSheets.Count == 1)
            {
                throw new Exception("Can not delete the sole worksheet");
            }

            document.DeleteWorksheet(sheetName);
            document.Save();

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Example for formatting currency and dates
    /// 
    /// var ops = new SpreadSheetLightLibrary.Examples();
    /// var excelFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpreadSheetLightFormatting.xlsx");
    /// if (File.Exists(excelFileName))
    /// {
    ///     File.Delete(excelFileName);
    /// }
    /// 
    /// ops.CreateNewFile(excelFileName);
    /// ops.SimpleFormatting(excelFileName);
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public bool SimpleFormatting(string fileName)
    {
        using SLDocument document = new(fileName, "Sheet1");

        SLStyle currencyStyle = document.CreateStyle();
        currencyStyle.FormatCode = "$#,##0.000";

        document.SetCellValue("H3", 100.3);
        document.SetCellValue("I3", 200.5);
        document.SetCellStyle("H3", currencyStyle);
        document.SetCellStyle("I3", currencyStyle);

        SLStyle dateStyle = document.CreateStyle();
        dateStyle.FormatCode = "mm-dd-yyyy";
        
        Dictionary<string, DateTime> dictDates = new()
        {
            { "H4", new(2017, 1, 1) },
            { "H5", new(2017, 1, 2) },
            { "H6", new(2017, 1, 3) },
            { "H7", new(2017, 1, 4) }
        };

        foreach (var (cell, dateTime) in dictDates)
        {
            if (!document.SetCellValue(cell, dateTime)) continue;
            document.SetCellStyle(cell, dateStyle);
            document.SetColumnWidth(cell, 12);
        }

        document.Save();

        return true;

    }

    /// <summary>
    /// Sets the value of a specified cell in a given Excel sheet.
    /// </summary>
    /// <param name="excelFileName">The name of the Excel file.</param>
    /// <param name="sheetName">The name of the sheet where the cell is located.</param>
    /// <param name="cell">The address of the cell to set the value for (e.g., "A1").</param>
    /// <param name="cellValue">The value to set in the specified cell.</param>
    public static void SetCellValue(string excelFileName, string sheetName, string cell, string cellValue)
    {
        if (File.Exists(excelFileName))
        {
            try
            {
                using SLDocument document = new(excelFileName, sheetName);
                if (document.GetSheetNames(false).Contains(sheetName))
                {
                    document.SetCellValue(cell, cellValue);
                    document.Save();
                }
            }
            catch (Exception ex)
            {
                // ignored - could be logged to a file via Serilog
            }
        }

    }

    /// <summary>
    /// Imports data from a tab-delimited text file into a specified worksheet in an Excel file.
    /// </summary>
    /// <param name="textFileName">The full path to the tab-delimited text file to be imported.</param>
    /// <param name="excelFileName">The full path to the Excel file where the data will be imported.</param>
    /// <param name="pSheetName">The name of the worksheet where the data will be imported. If the worksheet does not exist, it will be created.</param>
    /// <returns>
    /// <c>true</c> if the import operation is successful; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// If the specified worksheet already exists, its content will be cleared before importing the new data.
    /// The method also applies header styles, auto-fits columns, and freezes panes to ensure the header remains visible when scrolling.
    /// </remarks>
    public static bool ImportTabDelimitedTextFile(string textFileName, string excelFileName, string pSheetName)
    {
        try
        {

            var line = File.ReadAllLines(excelFileName).FirstOrDefault();
            /*
             * Needed later for auto-fit columns
             */
            var columnCount = line.Split('\t').Length;


            using SLDocument document = new();
            var headerStyle = HeaderStyle(document);
            var sheets = document.GetSheetNames(false);
            if (sheets.Any(workSheetName => string.Equals(workSheetName, pSheetName, StringComparison.CurrentCultureIgnoreCase)))
            {
                document.SelectWorksheet(pSheetName);
                document.ClearCellContent();
            }
            else
            {
                document.AddWorksheet(pSheetName);
            }

            var importOptions = new SLTextImportOptions();

            document.ImportText(textFileName, "A1", importOptions);

            // do not need Sheet1
            if (sheets.FirstOrDefault((sheetName) => sheetName.ToLower() == "sheet1") != null)
            {
                if (pSheetName.ToLower() != "sheet1")
                {
                    document.DeleteWorksheet("Sheet1");
                }
            }

            document.SetCellStyle(1, 1, 1, columnCount, headerStyle);

            for (int columnIndex = 1; columnIndex < columnCount +1; columnIndex++)
            {
                document.AutoFitColumn(columnIndex);
            }
            
            document.SetActiveCell("C2");

            // ensure header is visible when scrolling down
            document.FreezePanes(1, 6);

            document.SaveAs(excelFileName);

            return true;

        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// demonstrate how to get used columns in the format a letter rather than an integer
    /// </summary>
    /// <returns></returns>
    public string[] UsedCellsInWorkSheet(string fileName, string sheetName)
    {
        using SLDocument document = new(fileName, sheetName);

        SLWorksheetStatistics stats = document.GetWorksheetStatistics();

        IEnumerable<string> columnNames = Enumerable.Range(1, stats.EndColumnIndex)
            .Select(SLConvert.ToColumnName);

        return columnNames.ToArray();
    }

    /// <summary>
    /// Retrieves the last used row index in the specified worksheet of an Excel file.
    /// </summary>
    /// <param name="fileName">The name of the Excel file.</param>
    /// <param name="sheetName">The name of the worksheet within the Excel file.</param>
    /// <returns>The index of the last used row in the specified worksheet.</returns>
    public static int GetWorkSheetLastRow(string fileName, string sheetName)
    {

        using SLDocument document = new(fileName, sheetName);

        /*
         * get statistics, in this case we want the last used row, so we
         * simply index into EndRowIndex yet there are more properties.
         */
        return document.GetWorksheetStatistics().EndRowIndex;
    }

    /// <summary>
    /// Retrieves the last row and column indices used in a specified worksheet of an Excel file.
    /// </summary>
    /// <param name="fileName">The full path to the Excel file.</param>
    /// <param name="sheetName">The name of the worksheet to inspect.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item>
    /// <description><c>EndRowIndex</c>: The index of the last row with data in the worksheet.</description>
    /// </item>
    /// <item>
    /// <description><c>EndColumnIndex</c>: The index of the last column with data in the worksheet.</description>
    /// </item>
    /// </list>
    /// </returns>
    public static (int EndRowIndex, int EndColumnIndex) GetLastRowColumn(string fileName, string sheetName)
    {
        using SLDocument document = new(fileName, sheetName);
        return (
            document.GetWorksheetStatistics().EndRowIndex, 
            document.GetWorksheetStatistics().EndColumnIndex);
    }

    /// <summary>
    /// Get sheet names in an Excel file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public List<string> SheetNames(string fileName)
    {
        using SLDocument document = new(fileName);
        return document.GetSheetNames(false);
    }


    /// <summary>
    /// Checks if a sheet with the specified name exists in the given Excel file.
    /// </summary>
    /// <param name="fileName">The path to the Excel file.</param>
    /// <param name="pSheetName">The name of the sheet to check for existence.</param>
    /// <returns>
    /// <c>true</c> if the sheet exists; otherwise, <c>false</c>.
    /// </returns>
    public bool SheetExists(string fileName, string pSheetName)
    {
        using SLDocument document = new(fileName);
        return document.GetSheetNames(false).Any((sheetName) => 
            sheetName.ToLower() == pSheetName.ToLower());
    }


    /// <summary>
    /// Creates and returns a header style for the specified SLDocument.
    /// </summary>
    /// <param name="document">The SLDocument for which the header style is created.</param>
    /// <returns>An SLStyle object representing the header style.</returns>
    public static SLStyle HeaderStyle(SLDocument document)
    {

        SLStyle headerStyle = document.CreateStyle();

        headerStyle.Font.Bold = true;
        headerStyle.Font.FontColor = Color.White;
        headerStyle.Fill.SetPattern(
            PatternValues.LightGray,
            SLThemeColorIndexValues.Accent1Color,
            SLThemeColorIndexValues.Accent5Color);

        return headerStyle;
    }
}