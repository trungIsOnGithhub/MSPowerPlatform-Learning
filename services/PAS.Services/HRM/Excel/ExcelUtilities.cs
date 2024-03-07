
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services.HRM.Excel
{
    public struct ExcelConfigConst
    {
        public const string Bold = "&EXBOLD&";
        public const string Italic = "&EXITALIC&";
    }
    public class ExcelUtilities
    {
        public static DataTable ReadListTemplateToDataTable(ExcelWorksheet ws, int tableStartColumn, int tableEndColumn)
        {
            var tbl = new DataTable();

            for (var i = tableStartColumn; i <= tableEndColumn; i++)
            {

                tbl.Columns.Add();

            }
            return tbl;
        }
        public static DataTable ReadListTemplateToDataTable(ExcelWorksheet ws, int tableStartRow, int tableStartColumn, int tableEndRow, int tableEndColumn
           , string wordStart = "[", string wordEnd = "]", bool hasHeader = true)
        {
            var tbl = new DataTable();

            foreach (var firstRowCell in ws.Cells[tableStartRow, tableStartColumn, tableEndRow, tableEndColumn])
            {
                if (firstRowCell.Text.IndexOf(wordStart, StringComparison.Ordinal) < firstRowCell.Text.IndexOf(wordEnd, StringComparison.Ordinal))
                {
                    var text = firstRowCell.Text.Replace(wordStart, string.Empty).Replace(wordEnd, string.Empty);
                    tbl.Columns.Add(hasHeader ? text : $"Column {firstRowCell.Start.Column}");
                }
            }
            return tbl;
        }
        public static ExcelWorksheet GenerateData(ExcelWorksheet ws, DataTable templateDt, int tableStartRow)
        {
            for (var i = 0; i < templateDt.Rows.Count; i++)
            {

                for (var j = 0; j < templateDt.Columns.Count; j++)
                {
                    var value = templateDt.Rows[i][j];
                    ws.Cells[i + tableStartRow, j + 1].Value = value;
                }

            }
            ws.DeleteRow(tableStartRow + templateDt.Rows.Count, 1);
            return ws;
        }
        public static ExcelWorksheet CloneFormat(ExcelWorksheet ws, DataTable templateDt, int tableStartRow, int tableStartColumn, int tableEndRow, int tableEndColumn, string wordStart = "[", string wordEnd = "]")
        {
            for (var i = 0; i < templateDt.Rows.Count; i++)
            {
                try
                {
                    ws.InsertRow(tableStartRow + i + 1, 1);
                    ws.Cells[tableStartRow + i, tableStartColumn, tableEndRow + i, tableEndColumn]
                        .Copy(ws.Cells[tableStartRow + i + 1, tableStartColumn, tableStartRow + i + 1, tableEndColumn]);
                    foreach (var firstRowCell in ws.Cells[tableStartRow + i, tableStartColumn, tableEndRow + i, tableEndColumn])
                    {
                        var text = firstRowCell.Text;
                        if (string.IsNullOrWhiteSpace(text)) continue;
                        if (text.Contains("}{")) text = text.Replace("}{", "");
                        for (var j = 0; j < templateDt.Columns.Count; j++)
                        {
                            var columnName = templateDt.Columns[j].ColumnName;
                            if (text.Equals(wordStart + columnName + wordEnd, StringComparison.CurrentCultureIgnoreCase))
                            {
                                #region Apply format for each column

                                if (!string.IsNullOrWhiteSpace(columnName))
                                {
                                    var value = templateDt.Rows[i][columnName];

                                    if (value.ToString().Contains(ExcelConfigConst.Bold))
                                    {
                                        firstRowCell.Style.Font.Bold = true;
                                        value = value.ToString().Replace(ExcelConfigConst.Bold, "");
                                    }
                                    if (value.ToString().Contains(ExcelConfigConst.Italic))
                                    {
                                        firstRowCell.Style.Font.Italic = true;
                                        value = value.ToString().Replace(ExcelConfigConst.Italic, "");
                                    }

                                    //decimal number = (decimal)0;
                                    //if (decimal.TryParse(value.ToString(), out number))
                                    //{
                                    //    firstRowCell.Value = number;
                                    //}
                                    //else
                                    //{

                                    firstRowCell.Value = value;
                                    /*var heightDefault = 15;
                                    if (!string.IsNullOrWhiteSpace(value.ToString()) && firstRowCell.Style.WrapText)
                                    {

                                        var count = value.ToString().Split('\n').Length;
                                        var width = (int)GetWidthRangeAddress(firstRowCell);
                                        var height = ws.Row(firstRowCell.Start.Row).Height;
                                        var linecount = GetLineCount(value.ToString(), width);
                                        double heightNew = 0;
                                        if (count == linecount && linecount >= 2)
                                        {
                                            heightNew = heightDefault * (count <= 0 ? 1 : (count + 1));
                                        }
                                        else if (count > linecount)
                                        {
                                            heightNew = heightDefault * (count <= 0 ? 1 : count);
                                        }
                                        else
                                        {
                                            heightNew = heightDefault * (linecount <= 0 ? 1 : linecount);
                                        }
                                        if (height < heightNew)
                                        {
                                            ws.Row(firstRowCell.Start.Row).Height = heightNew;
                                        }
                                        ws.Row(firstRowCell.Start.Row).CustomHeight = true;
                                    }*/
                                    firstRowCell.Style.WrapText = true;
                                    ws.Row(firstRowCell.Start.Row).CustomHeight = false;
                                    //}
                                }
                                else
                                {
                                    break;
                                }
                                #endregion

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    string s = e.Message;
                }
            }
            ws.DeleteRow(tableStartRow + templateDt.Rows.Count, 1);
            // autofit all columns
            //ws.Cells[ws.Dimension.Address].AutoFitColumns();
            //ws.Cells.AutoFitColumns();

            return ws;
        }
    }
}
