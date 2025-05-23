﻿using SpreadsheetLight;
using SpreadSheetLightConsoleApp.Models;

namespace SpreadSheetLightConsoleApp.Classes;

public class ExcelOperations
{
    /// <summary>
    /// Locate text in used cells
    /// </summary>
    /// <param name="searchItem"><seealso cref="SearchItem"/></param>
    /// <returns>Named value tuple list of <seealso cref="FoundItemImmutable"/> and <seealso cref="Exception"/></returns>
    /// <remarks>
    /// Optional, add logic for like/contains
    /// </remarks>
    public static (IReadOnlyList<FoundItemImmutable> items, Exception exception) FindText(SearchItem searchItem)
    {

        List<FoundItemImmutable> foundList = [];

        try
        {
            using SLDocument document = new(searchItem.FileName, searchItem.SheetName);
            var stats = document.GetWorksheetStatistics();

            // start on first column to last known column
            for (int columnIndex = 1; columnIndex < stats.EndColumnIndex + 1; columnIndex++)
            {

                // from first to last row
                for (int rowIndex = 1; rowIndex < stats.EndRowIndex + 1; rowIndex++)
                {
                    if (document.GetCellValueAsString(rowIndex, columnIndex).Equals(searchItem.Token, searchItem.StringComparison))
                    {
                        foundList.Add(new FoundItemImmutable(rowIndex, columnIndex, SLConvert.ToColumnName(columnIndex)));
                    }
                }
            }

            return (foundList, null)!;

        }
        catch (Exception exception)
        {
            return (foundList, exception);
        }

    }

    /// <summary>
    /// Iterates through the first column of an Excel worksheet and prints each cell's value to the console.
    /// </summary>
    /// <remarks>
    /// This method opens an Excel file named "Excel1.xlsx" and iterates through all rows in the first column,
    /// printing the value of each cell to the console.
    /// </remarks>
    public static void Iterate()
    {
        using SLDocument document = new("Excel1.xlsx");
        int columnIndex = 1;
        var stats = document.GetWorksheetStatistics();

        for (int rowIndex = 1; rowIndex < stats.EndRowIndex + 1; rowIndex++)
        {
            Console.WriteLine(document.GetCellValueAsString(rowIndex, columnIndex));
        }
    }
     
}