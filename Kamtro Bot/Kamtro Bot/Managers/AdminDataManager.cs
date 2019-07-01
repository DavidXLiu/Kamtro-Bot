using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kamtro_Bot.Nodes;
using OfficeOpenXml;

namespace Kamtro_Bot.Managers
{
    public class AdminDataManager {
        private const string StrikeLogPath = @"Admin\strikelog.xlsx";
        private const string StrikeLogPage = "Strike Log";

        private static ExcelPackage StrikeLog;

        /// <summary>
        /// This method will generate and save the excel file.
        /// Only the header is generated, all other data is left as-is.
        /// </summary>
        public static void InitExcel() {
            Console.WriteLine("[I] Initializing Excel...");
            FileInfo strikeFile = new FileInfo(StrikeLogPath);
            ExcelPackage excel = new ExcelPackage(strikeFile);

            if (excel.Workbook.Worksheets.Count < 1) {
                excel.Workbook.Worksheets.Add(StrikeLogPage);
            }

            // if the cell A1 is the word "reset", then regenerate the header.
            if (excel.Workbook.Worksheets[StrikeLogPage].Cells["A1"].Value == null) {
                Console.WriteLine("[I] Adding Header...");
                AddHeader(excel);
            }

            StrikeLog = excel;  // now add a hook to the log file.
            excel.Save();
        }

        /// <summary>
        /// Adds the header to the excel file
        /// </summary>
        /// <param name="ex">The excel package to add the header to</param>
        private static void AddHeader(ExcelPackage ex) {
            List<string[]> headerRow = new List<string[]>() {
                new string[] { "ID", "Username", "Strike Count",
                    "Strike 1 Date", "Strike 1 Moderator", "Strike 1 Reason",
                    "Strike 2 Date", "Strike 2 Moderator", "Strike 2 Reason",
                    "Strike 3 Date", "Strike 3 Moderator", "Strike 3 Reason"
                }
            };

            string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

            ExcelWorksheet worksheet = ex.Workbook.Worksheets[StrikeLogPage];

            // load the data
            worksheet.Cells[headerRange].LoadFromArrays(headerRow);

            // Style the header
            worksheet.Cells[headerRange].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[headerRange].Style.Fill.BackgroundColor.SetColor(255, 250, 117, 117);
            worksheet.Cells[headerRange].Style.Font.Bold = true;
            worksheet.Cells[headerRange].Style.Font.Size = 14;

            // Save the file
            FileInfo file = new FileInfo(StrikeLogPath);
            ex.SaveAs(file);
        }

        public static void SaveExcel() {
            KLog.Info("Saving Excel...");
            StrikeLog.Save();
        }

        /// <summary>
        /// Adds a strike to a user
        /// </summary>
        /// <param name="target"></param>
        /// <param name="strike"></param>
        public static void AddStrike(SocketUser target, StrikeDataNode strike) {

        }
    }
}
