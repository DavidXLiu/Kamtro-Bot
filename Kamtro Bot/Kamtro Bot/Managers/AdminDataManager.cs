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
        public const string StrikeLogPath = @"Admin\strikelog.xlsx";
        private const string StrikeLogPage = "Strike Log";

        private const string IDColumn = "A";

        private static ExcelPackage StrikeLog;

        /// <summary>
        /// This method will generate and save the excel file.
        /// Only the header is generated, all other data is left as-is.
        /// </summary>
        public static void InitExcel() {
            KLog.Info("Initializing Excel...");
            FileInfo strikeFile = new FileInfo(StrikeLogPath);
            ExcelPackage excel = new ExcelPackage(strikeFile);

            if (excel.Workbook.Worksheets.Count < 1) {
                excel.Workbook.Worksheets.Add(StrikeLogPage);
            }

            // if the cell A1 is the word "reset", then regenerate the header.
            if (excel.Workbook.Worksheets[StrikeLogPage].Cells["A1"].Value == null) {
                KLog.Info("Adding Header...");
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
        /// <returns>The number of strikes the user has.</returns>
        public static int AddStrike(SocketUser target, StrikeDataNode strike) {
            ulong targetId = target.Id;
            int pos = 2;
            ExcelRange cells = StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells;
            while (cells["A"+pos].Value != null) {
                ExcelRange cell = cells["A" + pos];
                object test = cell.Value;
                if (test == null) break;

                if (Convert.ToUInt64(test) == targetId) {
                    // Add the srike to this row.

                    // First, check the username. 
                    if(cells["B" + pos].Text != BotUtils.GetFullUsername(target)) {
                        // if it doesn't check out, update it.
                        cells["B" + pos].Value = BotUtils.GetFullUsername(target);
                    }

                    // now for the strike address. This will be based off of the number of strikes.
                    // This is in column C
                    int strikes = cells["C" + pos].GetValue<int>();

                    if (strikes == 3) return 4;  // 4 is the signal

                    // now to get the column. Fun ascii math.
                    // 68 = ASCII for capital D. 
                    string range = char.ConvertFromUtf32(68 + strikes*3) + pos + ":" + char.ConvertFromUtf32(70 + strikes * 3) + pos;

                    cells[range].LoadFromArrays(strike.GetStrikeForExcel());

                    cells[$"C:{pos}"].Value = (Convert.ToInt32(cells[$"C{pos}"].Text) + 1).ToString();
                    StrikeLog.Save();

                    KLog.Info($"Added strike {cells[$"C:{pos}"].Value.ToString()} for {BotUtils.GetFullUsername(target)} in cell range {range}");

                    return Convert.ToInt32(cells[$"C{pos}"].Text);
                }

                pos++;
            }

            // The user doesn't have an entry. So make one.
            GenUserStrike(pos, target);

            // Now add the strike
            ExcelRange er = cells[$"D{pos}:F{pos}"];
            er.LoadFromArrays(strike.GetStrikeForExcel());
            StrikeLog.Save();
            KLog.Info($"Added strike for {BotUtils.GetFullUsername(target)} in cell range D{pos}:F{pos}");

            return 1;
        }


        /// <summary>
        /// Generates the base for a user entry. Does not generate the strike, only columns A through C
        /// </summary>
        /// <param name="pos">Row in the spreadsheet</param>
        /// <param name="target">Target user for entry</param>
        private static void GenUserStrike(int pos, SocketUser target) {
            KLog.Info($"User {BotUtils.GetFullUsername(target)} doesn't have a strike entry, creating one...");
            List<string[]> entry = new List<string[]>();

            entry.Add(new string[] { target.Id.ToString(), BotUtils.GetFullUsername(target), "1" });

            StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells[$"A{pos}:C{pos}"].LoadFromArrays(entry);
        }
    }
}
