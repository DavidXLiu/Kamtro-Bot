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
    public static class AdminDataManager {
        public const string StrikeLogPath = @"Admin\strikelog.xlsx";
        public const string SettingsJsonPath = @"Config\settings.json";
        private const string StrikeLogPage = "Strike Log";

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
                    "Ban Date", "Ban Moderator", "Ban Reason"
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

            // Set auto fit
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Save the file
            FileInfo file = new FileInfo(StrikeLogPath);
            ex.SaveAs(file);
        }

        /// <summary>
        /// Saves the strike log file
        /// </summary>
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

                if (Convert.ToUInt64(test.ToString()) == targetId) {
                    // Add the srike to this row.

                    // First, check the username. 
                    if(cells["B" + pos].Text != BotUtils.GetFullUsername(target)) {
                        // if it doesn't check out, update it.
                        cells["B" + pos].Value = BotUtils.GetFullUsername(target);
                    }

                    // now for the strike address. This will be based off of the number of strikes.
                    // This is in column C
                    int strikes = cells["C" + pos].GetValue<int>();

                    if (strikes == 2) return 4;  // 4 is the signal

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
            // Set auto fit
            cells[StrikeLog.Workbook.Worksheets[StrikeLogPage].Dimension.Address].AutoFitColumns();
            StrikeLog.Save();
            KLog.Info($"Added strike for {BotUtils.GetFullUsername(target)} in cell range D{pos}:F{pos}");

            return 1;
        }

        public static void AddBan(SocketUser target, BanDataNode ban) {
            int pos = 2;
            ExcelRange cells = StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells;

            while (cells["A" + pos].Value != null) {
                if (Convert.ToUInt64(cells["A" + pos].Value.ToString()) == target.Id) {
                    cells[$"J{pos}:L{pos}"].LoadFromArrays(ban.GetBanForExcel());
                    KLog.Info($"Banned user {BotUtils.GetFullUsername(target)} by {ban.Moderator} for reason: {ban.Reason}. Ban added in cell range J{pos}:L{pos}.");
                    SaveExcel();
                    return;
                }
                pos++;
            }

            // User doesn't have an entry, so is likely just a troll.
            GenUserStrike(pos, target);
            cells[$"J{pos}:L{pos}"].LoadFromArrays(ban.GetBanForExcel());
            // Set auto fit
            cells[StrikeLog.Workbook.Worksheets[StrikeLogPage].Dimension.Address].AutoFitColumns();
            KLog.Info($"Banned user {BotUtils.GetFullUsername(target)} by {ban.Moderator} for reason: {ban.Reason}. Ban added in cell range J{pos}:L{pos}.");
            SaveExcel();
        }

        public static void AddBan(ulong id, BanDataNode ban, string username = "") {
            int pos = 2;
            ExcelRange cells = StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells;

            while (cells["A" + pos].Value != null) {
                if (Convert.ToUInt64(cells["A" + pos].Value.ToString()) == id) {
                    cells[$"J{pos}:L{pos}"].LoadFromArrays(ban.GetBanForExcel());
                    KLog.Info($"Banned user {(string.IsNullOrWhiteSpace(username) ? id.ToString() : username)} by {ban.Moderator} for reason: {ban.Reason}. Ban added in cell range J{pos}:L{pos}.");
                    SaveExcel();
                    return;
                }
                pos++;
            }

            // User doesn't have an entry, so is likely just a troll.
            GenUserStrike(pos, id);
            cells[$"J{pos}:L{pos}"].LoadFromArrays(ban.GetBanForExcel());
            // Set auto fit
            cells[StrikeLog.Workbook.Worksheets[StrikeLogPage].Dimension.Address].AutoFitColumns();
            KLog.Info($"Banned user {(string.IsNullOrWhiteSpace(username) ? id.ToString() : username)} by {ban.Moderator} for reason: {ban.Reason}. Ban added in cell range J{pos}:L{pos}.");
            SaveExcel();
        }

        public static int AddStrike(ulong targetId, StrikeDataNode strike, string username = "", bool mod = false) {
            ExcelRange cells = StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells;
            int row = GetEntryPos(targetId);
            
            if (!IsRowNew(row)) {
                // Add the srike to this row.

                if(!string.IsNullOrEmpty(username)) cells["B" + row].Value = username;

                // now for the strike address. This will be based off of the number of strikes.
                // This is in column C
                int strikes = cells["C" + row].GetValue<int>();

                string rr;

                if (!mod) {
                    if (strikes == 2) return 4;  // 4 is the signal

                    // now to get the column. Fun ascii math.
                    // 68 = ASCII for capital D. 
                    rr = char.ConvertFromUtf32(68 + strikes * 3) + row + ":" + char.ConvertFromUtf32(70 + strikes * 3) + row;

                    cells[rr].LoadFromArrays(strike.GetStrikeForExcel());

                    cells[$"C:{row}"].Value = (Convert.ToInt32(cells[$"C{row}"].Text) + 1).ToString();
                } else {
                    // This does nothing really. Probably for the best.
                    strikes--;

                    rr = char.ConvertFromUtf32(68 + strikes * 3) + row + ":" + char.ConvertFromUtf32(70 + strikes * 3) + row;
                    cells[rr].LoadFromArrays(strike.GetStrikeForExcel());
                }

                StrikeLog.Save();

                KLog.Info($"Added strike {cells[$"C:{row}"].Value.ToString()} for {(string.IsNullOrEmpty(username) ? targetId.ToString() : username)} in cell range {rr}");

                return Convert.ToInt32(cells[$"C{row}"].Text);
            }

            // The user doesn't have an entry. So make one.
            GenUserStrike(row, targetId, username);

            // Now add the strike
            // 68 = ASCII for   capital D. 
            string range = char.ConvertFromUtf32(68 + GetStrikes(targetId) * 3) + row + ":" + char.ConvertFromUtf32(70 + GetStrikes(targetId) * 3) + row;
            cells[range].LoadFromArrays(strike.GetStrikeForExcel());
            // Set auto fit
            cells[StrikeLog.Workbook.Worksheets[StrikeLogPage].Dimension.Address].AutoFitColumns();
            StrikeLog.Save();
            KLog.Info($"Added strike for {(string.IsNullOrEmpty(username) ? targetId.ToString() : username)} in cell range D{row}:F{row}");

            return 1;
        }

        /// <summary>
        /// Generates the base for a user entry. Does not generate the strike, only columns A through C
        /// </summary>
        /// <param name="pos">Row in the spreadsheet</param>
        /// <param name="target">Target user for entry</param>
        private static void GenUserStrike(int pos, SocketUser target, int strikes = 1) {
            KLog.Info($"User {BotUtils.GetFullUsername(target)} doesn't have a strike entry, creating one...");
            List<string[]> entry = new List<string[]>();

            entry.Add(new string[] { target.Id.ToString(), BotUtils.GetFullUsername(target), strikes.ToString() });

            StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells[$"A{pos}:C{pos}"].LoadFromArrays(entry);
        }

        private static void GenUserStrike(int pos, ulong target, string username = "", int strikes = 1) {
            KLog.Info($"User {(string.IsNullOrEmpty(username) ? target.ToString() : username)} doesn't have a strike entry, creating one...");
            List<string[]> entry = new List<string[]>();

            entry.Add(new string[] { target.ToString(), username, strikes.ToString() });

            StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells[$"A{pos}:C{pos}"].LoadFromArrays(entry);
        }

        /// <summary>
        /// Gets the reason for the user's strike
        /// </summary>
        /// <param name="id">ID of the user</param>
        /// <param name="strike">Number of the strike (3 for ban)</param>
        /// <returns>The reason for the strike/ban</returns>
        public static string GetStrikeReason(ulong id, int strike) {
            if (strike < 1 || strike > 3) return "";

            ExcelRange cells = StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells;
            if (strike == 1) {
                return cells[$"F{GetEntryPos(id)}"].Text;
            } else if (strike == 2) {
                return cells[$"I{GetEntryPos(id)}"].Text;
            } else {
                return cells[$"L{GetEntryPos(id)}"].Text;
            }
        }

        /// <summary>
        /// Sets the reason for a strike for a user
        /// </summary>
        /// <param name="id">ID of the user</param>
        /// <param name="strike">The number of the strike (3 for ban)</param>
        /// <param name="reason">New reason for the strike</param>
        public static void SetStrikeReason(ulong id, int strike, string reason) {
            if (strike < 1 || strike > 3) return;

            ExcelRange cells = StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells;

            if (strike == 1) {
                cells[$"F{GetEntryPos(id)}"].Value = reason;
            } else if (strike == 2) {
                cells[$"I{GetEntryPos(id)}"].Value = reason;
            } else {
                cells[$"L{GetEntryPos(id)}"].Value = reason;
            }

            // Fix the count
            cells[$"C{GetEntryPos(id)}"].Value = Math.Max(Convert.ToInt32(cells[$"C{GetEntryPos(id)}"].Value), strike);
            
            // Set auto fit
            cells[StrikeLog.Workbook.Worksheets[StrikeLogPage].Dimension.Address].AutoFitColumns();
            SaveExcel();
        }

        /// <summary>
        /// Returns the number of strikes a user currently has. If the user does not have an entry, this creates one.
        /// </summary>
        /// <param name="user">The user to check</param>
        /// <returns>The number of strikes the user has</returns>
        public static int GetStrikes(SocketUser user) {
            ulong id = user.Id;
            return GetStrikes(id);
        }

        public static int GetStrikes(ulong id) {
            int pos = 2;
            ulong target;
            ExcelRange cells = StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells;

            while (cells["A" + pos].Value != null) {
                target = Convert.ToUInt64(cells["A" + pos].Value.ToString());
                if (target == id) {
                    int strikes = Convert.ToInt32(cells["C" + pos].Value);
                    return strikes;
                }

                pos++;
            }

            // Don't create new logs for those without strikes/bans - Arcy
            // GenUserStrike(pos, id, "", 0);
            // SaveExcel();
            return 0;
        }

        /// <summary>
        /// Method for getting the correct position for the user's entry. 
        /// First available one if the user has no entry already.
        /// </summary>
        /// <param name="id">The ID of the user</param>
        /// <returns>The row number for the entry</returns>
        public static int GetEntryPos(ulong id) {
            int i = 2;
            ExcelRange cells = StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells;

            while(cells["A"+i].Value != null) {
                if(Convert.ToUInt64(cells[$"A{i}"].Value.ToString()) == id) {
                    return i;
                }

                i++;
            }

            return i;
        }

        /// <summary>
        /// Method for getting the total entries within the Strike Log file.
        /// Used for checking if there is no current entry in the log for a user.
        /// Author: Arcy
        /// </summary>
        /// <returns></returns>
        public static int GetEntryCount()
        {
            ExcelRange cells = StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells;
            int rows = 0;
            object r = cells[$"A2"].Value;

            while(r != null) {
                rows++;
                r = cells[$"A{rows+2}"].Value;
            }

            return rows;
        }

        /// <summary>
        /// Mehtod used to check if a row is newly added (as in, is not pre-existing
        /// </summary>
        /// <param name="row">The row number</param>
        /// <returns>True if the row is new, false otherwise.</returns>
        public static bool IsRowNew(int row) {
            return row == GetEntryCount() + 2;
        }

        /// <summary>
        /// Method for checking if the current user has an entry in the Strike Log file.
        /// Author: Arcy
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool HasEntry(ulong userId)
        {
            return GetEntryPos(userId) >= GetEntryCount();
        }

        public static void DeleteStrike(ulong id, int strike) {
            if (GetStrikes(id) < strike || !HasEntry(id)) return;

            int pos = GetEntryPos(id);
            ExcelRange cells = StrikeLog.Workbook.Worksheets[StrikeLogPage].Cells;

            switch(strike) {
                case 1:
                    cells[$"D{pos}:F{pos}"].Clear();
                    break;

                case 2:
                    cells[$"G{pos}:I{pos}"].Clear();
                    break;

                case 3:
                    cells[$"J{pos}:L{pos}"].Clear();
                    break;
            }

            cells[$"C:{pos}"].Value = (Convert.ToInt32(cells[$"C{pos}"].Text) - 1).ToString();
            // Set auto fit
            cells[StrikeLog.Workbook.Worksheets[StrikeLogPage].Dimension.Address].AutoFitColumns();
            KLog.Info($"Removed entry for user {id}: {(strike == 3 ? "Ban" : $"Strike {strike}")}");

            SaveExcel();
        }
    }
}
