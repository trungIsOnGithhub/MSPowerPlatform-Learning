using OfficeOpenXml;
using PAS.Model.Enum.HRM;
using PAS.Model.HRM;
using PAS.Repositories.HRM;
using PAS.Services.HRM.Infrastructures;
using System;
using System.Data;
using System.IO;
using System.Linq;

namespace PAS.Services.HRM.Excel
{
    public interface IEpplusExcelService : IBaseService
    {
        byte[] ExportExcelLeaveTracking(LeavePaging leaves);
        byte[] ExportExcelLeaveTrackingByTemplate(UserWithUserLeaveTrackPaging leaves, LeavesFilter filter, string filePath);
    }

    public class EpplusExcelService : BaseService, IEpplusExcelService
    {
        private string[,] _leaveApproveHeaderArray = new string[8, 2] {
            { "A", "No." }, { "B", "Full Name" }, { "C", "Start Date" },{ "D", "End Date" }, {"E", "Leave Type"},
            {"F", "Number of days" }, {"G","Reason" }, {"H","Status"} };
        private static int leaveRequestHeaderRow = 1;
        private static int leaveRequestBodyRow = 4;

        public EpplusExcelService(IHRMUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        public byte[] ExportExcelLeaveTrackingByTemplate(UserWithUserLeaveTrackPaging leaves, LeavesFilter filter, string filePath)
        {
            byte[] xlsxBytes = null;
            var dateFormat = "dd/MM/yyyy";

            using (ExcelPackage pck = new ExcelPackage())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    pck.Load(stream);
                }
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                var fromDate = ws.Cells.Where(x => x.Text.Contains("[fromdate]")).FirstOrDefault();
                if (fromDate != null)
                {
                    fromDate.Value = fromDate.Value.ToString().Replace("[fromdate]", filter.StartDate.HasValue ? filter.StartDate.Value.Date.ToString(dateFormat) : new DateTime(DateTime.Now.Year, 1, 1).Date.ToString(dateFormat));
                }
                var todate = ws.Cells.Where(x => x.Text.Contains("[todate]")).FirstOrDefault();
                if (todate != null)
                {
                    todate.Value = todate.Value.ToString().Replace("[todate]", filter.EndDate.HasValue ? filter.EndDate.Value.Date.ToString(dateFormat) : new DateTime(DateTime.Now.Year, 12, 31).Date.ToString(dateFormat));
                }

                /// number row start to insert data
                int tableStartRow = 7;
                int tableStartColumn = 1;
                int tableEndRow = 7;
                int tableEndColumn = 8 < ws.Dimension.End.Column ? 13 : ws.Dimension.End.Column;
                var templateDt = ExcelUtilities.ReadListTemplateToDataTable(ws, tableStartRow, tableStartColumn, tableEndRow, tableEndColumn);
                FillLeaveTemplateDataTable(templateDt, leaves);
                ExcelUtilities.CloneFormat(ws, templateDt, tableStartRow, tableStartColumn, tableEndRow, tableEndColumn);
                xlsxBytes = pck.GetAsByteArray();
            }


            return xlsxBytes;
        }
        private void FillLeaveTemplateDataTable(DataTable templateTable, UserWithUserLeaveTrackPaging item)
        {
            if (item == null)
            {
                return;
            }
            DataColumnCollection columns = templateTable.Columns;
            foreach (var model in item.Users)
            {
                var row = templateTable.NewRow();
                if (columns.Contains("employeeId"))
                {

                    row["employeeId"] = model.Id;
                }
                if (columns.Contains("fullname"))
                {
                    row["fullname"] = $"{model.UserInformation?.FirstName} {model.UserInformation?.LastName}";
                }
                if (columns.Contains("totalleave"))
                {
                    row["totalleave"] = model.TotalAnnualNumber;
                }
                if (columns.Contains("leavetaken"))
                {
                    row["leavetaken"] = model.UsedAnnualNumber;
                }
                if (columns.Contains("leaveremain"))
                {
                    row["leaveremain"] = model.RemainAnnualNumber;
                }
                if (columns.Contains("totalsick"))
                {
                    row["totalsick"] = model.TotalSickNumber;
                }
                if (columns.Contains("sicktaken"))
                {
                    row["sicktaken"] = model.UsedSickNumber;
                }
                if (columns.Contains("sickremain"))
                {
                    row["sickremain"] = model.RemainSickNumber;
                }
                if (columns.Contains("totalpersonal"))
                {
                    row["totalpersonal"] = model.TotalPersonalNumber;
                }
                if (columns.Contains("personaltaken"))
                {
                    row["personaltaken"] = model.UsedPersonalNumber;
                }
                if (columns.Contains("personalremain"))
                {
                    row["personalremain"] = model.RemainPersonalNumber;
                }
                templateTable.Rows.Add(row);
            }
        }
        public byte[] ExportExcelLeaveTracking(LeavePaging leaves)
        {
            byte[] xlsxBytes = null;

            using (var stream = new MemoryStream())
            {
                using (var excelPackage = new ExcelPackage(stream))
                {
                    var worksheet = excelPackage.Workbook.Worksheets.Add("Leave tracking");
                    SetExcelTitle(worksheet, "C", leaveRequestHeaderRow, "Leave Request Tracking");
                    SetExcelHeader(worksheet, _leaveApproveHeaderArray, 3);
                    var index = 0;
                    var valueStartRow = leaveRequestBodyRow;
                    foreach (var item in leaves.LeaveRequests)
                    {
                        object[] values = new object[8];
                        values[0] = (++index);
                        values[1] = item.RequestForUser.DisplayName;
                        values[2] = item.StartDate.ToString("dd/MM/yyyy");
                        values[3] = item.EndDate.ToString("dd/MM/yyyy");
                        values[4] = item.LeaveType.Name;
                        values[5] = item.NumberOfDay;
                        values[6] = item.Reason;
                        values[7] = item.IsRemoved ? ApprovalStep.Removed.ToString() : item.Status.ToString();
                        var valueRow = valueStartRow + index - 1;
                        SetExcelCellValue(worksheet, _leaveApproveHeaderArray, values, valueRow);
                    }
                    xlsxBytes = excelPackage.GetAsByteArray();
                }
            }

            return xlsxBytes;
        }

        public void SetExcelHeader(ExcelWorksheet worksheet, string[,] headerArray, int headerRow)
        {
            for (int i = 0; i < headerArray.GetLength(0); i++)
            {
                var index = headerArray[i, 0] + headerRow;
                var header = headerArray[i, 1];
                using (ExcelRange cell = worksheet.Cells[index])
                {
                    cell.Value = header;
                    var font = cell.Style.Font;
                    font.Bold = true;
                    font.Size = 14;
                }
            }

        }

        public void SetExcelCellValue(ExcelWorksheet worksheet, string[,] headerArray, object[] values, int valueRow)
        {
            for (int i = 0; i < headerArray.GetLength(0); i++)
            {
                var index = headerArray[i, 0] + valueRow;
                var value = values[i];
                using (ExcelRange cell = worksheet.Cells[index])
                {

                    cell.Value = value;
                    cell.AutoFitColumns();
                }
            }
        }

        public void SetExcelTitle(ExcelWorksheet worksheet, string row, int column, string value)
        {
            using (ExcelRange cell = worksheet.Cells[row + column])
            {
                cell.Value = value;
                var font = cell.Style.Font;
                font.Bold = true;
                font.Size = 16;
            }
        }

    }
}
