using ClosedXML.Excel;
using SchemaNote.ViewModels;
using System.Text;
using static SchemaNote.Models.Common;

namespace SchemaNote.Services
{
    public interface IExportService
    {
        /// <summary>依前端傳入的 OBJECT_ID 清單過濾要匯出的 Table/View；未提供時維持全部。</summary>
        void FilterTablesByObjectIds(OverviewViewModel model, int[]? objectIds);

        /// <summary>將 Overview 內容組成 Markdown 文字。</summary>
        string BuildMarkdown(OverviewViewModel model);

        /// <summary>將 Overview 內容組成 Excel（xlsx）位元組。</summary>
        byte[] BuildExcel(OverviewViewModel model);

        /// <summary>將文字轉為含 UTF-8 BOM 的位元組。</summary>
        byte[] Utf8WithBom(string text);
    }

    /// <summary>
    /// 匯出相關的商業邏輯：Markdown / Excel 產生、標籤分組、工作表命名、物件過濾與 UTF-8 BOM 編碼。
    /// </summary>
    public class ExportService : IExportService
    {
        // 依前端傳入的 OBJECT_ID 清單（搜尋/標籤過濾後仍顯示的項目）過濾要匯出的 Table/View。
        // 未提供清單時（例如直接呼叫或無過濾），維持匯出全部。
        public void FilterTablesByObjectIds(OverviewViewModel model, int[]? objectIds)
        {
            if (model is null || objectIds is null || objectIds.Length == 0)
            {
                return;
            }

            var idSet = new HashSet<int>(objectIds);
            model.Tables = [.. model.Tables.Where(t => idSet.Contains(t.OBJECT_ID))];
        }

        public byte[] Utf8WithBom(string text)
        {
            // 前置 UTF-8 BOM，確保 Windows 上開啟檔案時能正確辨識為 UTF-8。
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            return [.. encoding.GetPreamble(), .. encoding.GetBytes(text)];
        }

        // 依標籤（FLAGS）將 Table/View 分組；相同標籤集中於同一個群組。
        // 沒有標籤（或標籤即為 "-none-"）的物件集中於名稱為 "-none-" 的群組。
        // 以標籤第一次出現的順序保持群組順序。
        private static (List<string> Order, Dictionary<string, List<Table>> Groups) GroupTablesByFlag(OverviewViewModel model)
        {
            var groups = new Dictionary<string, List<Table>>(StringComparer.Ordinal);
            var groupOrder = new List<string>();

            void AddToGroup(string flag, Table table)
            {
                if (!groups.TryGetValue(flag, out var list))
                {
                    list = [];
                    groups[flag] = list;
                    groupOrder.Add(flag);
                }
                list.Add(table);
            }

            foreach (Table table in model.Tables)
            {
                if (table.FlagList.Count == 0)
                {
                    AddToGroup(NoneSheetName, table);
                }
                else
                {
                    foreach (string flag in table.FlagList)
                    {
                        AddToGroup(string.IsNullOrEmpty(flag) ? NoneSheetName : flag, table);
                    }
                }
            }

            return (groupOrder, groups);
        }

        public string BuildMarkdown(OverviewViewModel model)
        {
            static string MdEscape(string? value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return string.Empty;
                }
                return value.Replace("|", "\\|").Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");
            }

            // <summary> 內容為 HTML 內文，需轉義 HTML 特殊字元避免破版。
            static string HtmlEscape(string? value)
            {
                return string.IsNullOrEmpty(value)
                    ? string.Empty
                    : System.Net.WebUtility.HtmlEncode(value);
            }

            StringBuilder sb = new();
            sb.AppendLine("# Overview");
            sb.AppendLine();

            var (groupOrder, groups) = GroupTablesByFlag(model);

            foreach (string flag in groupOrder)
            {
                // 以 <details>/<summary> 產生可摺疊的 Accordion 區塊。
                // <summary> 為 HTML 內文，需 HTML 轉義；空一行讓內部 Markdown 正確解析。
                sb.AppendLine("<details>");
                sb.AppendLine($"<summary>{HtmlEscape(flag)}</summary>");
                sb.AppendLine();

                foreach (Table item in groups[flag])
                {
                    sb.AppendLine($"### {MdEscape(item.NAME)}");
                    sb.AppendLine();
                    sb.AppendLine($"> {MdEscape(item.MS_Description)}");
                    sb.AppendLine();

                    sb.AppendLine($"| {OBJ_Type} | {OBJ_SchemaName} | {OBJ_CreateDate} | {OBJ_ModifyDate} | {OBJ_RowCount} |");
                    sb.AppendLine("| --- | --- | --- | --- | --- |");
                    sb.AppendLine($"| {MdEscape(item.TYPE_NAME)} | {MdEscape(item.SCHEMA_NAME)} | {MdEscape(item.CREATE_DATE)} | {MdEscape(item.MODIFY_DATE)} | {item.QTY} |");
                    sb.AppendLine();

                    sb.AppendLine($"**{OBJ_COL_Remark}：** {MdEscape(item.REMARK)}");
                    sb.AppendLine();

                    sb.AppendLine($"| {COL_Name} | {OBJ_COL_ChineseName} | {COL_Type} | {P_Key} | {COL_NotNull} | {COL_DefaultVal} | {OBJ_COL_Remark} |");
                    sb.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
                    foreach (Column col in item.Columns)
                    {
                        sb.AppendLine($"| {MdEscape(col.NAME)} | {MdEscape(col.MS_Description)} | {MdEscape(col.TYPE)} | {(col.IS_PK ? "✔" : string.Empty)} | {(col.DISALLOW_NULL ? "✔" : string.Empty)} | {MdEscape(col.DEFUALT)} | {MdEscape(col.REMARK)} |");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("</details>");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public byte[] BuildExcel(OverviewViewModel model)
        {
            using var workbook = new XLWorkbook();

            // 樣式常數
            var headerBackColor = XLColor.FromHtml("#4472C4");
            var headerFontColor = XLColor.White;
            var infoBackColor = XLColor.FromHtml("#D9E1F2");

            // 匯出 Excel 時，不存在的擴充屬性呈現空白（而非 "null"）。
            static string Blank(string? value) =>
                string.IsNullOrEmpty(value) || value == DefaultValue ? string.Empty : value;

            var (groupOrder, groups) = GroupTablesByFlag(model);

            // 用於工作表名稱唯一與長度限制（Excel 上限 31 字元）
            var usedSheetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string flag in groupOrder)
            {
                string sheetName = BuildSheetName(flag, usedSheetNames);
                var ws = workbook.Worksheets.Add(sheetName);

                int row = 1;

                foreach (Table table in groups[flag])
                {
                    // 物件標題
                    ws.Cell(row, 1).Value = table.NAME;
                    ws.Range(row, 1, row, 7).Merge();
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 1).Style.Font.FontSize = 14;
                    row++;

                    // 物件中文說明
                    ws.Cell(row, 1).Value = Blank(table.MS_Description);
                    ws.Range(row, 1, row, 7).Merge();
                    ws.Cell(row, 1).Style.Font.Italic = true;
                    row++;

                    // 物件資訊表頭
                    string[] infoHeaders = [OBJ_Type, OBJ_SchemaName, OBJ_CreateDate, OBJ_ModifyDate, OBJ_RowCount];
                    for (int c = 0; c < infoHeaders.Length; c++)
                    {
                        var cell = ws.Cell(row, c + 1);
                        cell.Value = infoHeaders[c];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = infoBackColor;
                    }
                    row++;

                    // 物件資訊內容
                    ws.Cell(row, 1).Value = table.TYPE_NAME;
                    ws.Cell(row, 2).Value = table.SCHEMA_NAME;
                    ws.Cell(row, 3).Value = table.CREATE_DATE;
                    ws.Cell(row, 4).Value = table.MODIFY_DATE;
                    ws.Cell(row, 5).Value = table.QTY;
                    row++;

                    // 備註
                    ws.Cell(row, 1).Value = OBJ_COL_Remark;
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 1).Style.Fill.BackgroundColor = infoBackColor;
                    ws.Cell(row, 2).Value = Blank(table.REMARK);
                    ws.Range(row, 2, row, 7).Merge();
                    row += 2;

                    // 欄位表頭
                    string[] colHeaders = [COL_Name, OBJ_COL_ChineseName, COL_Type, P_Key, COL_NotNull, COL_DefaultVal, OBJ_COL_Remark];
                    int headerRow = row;
                    for (int c = 0; c < colHeaders.Length; c++)
                    {
                        var cell = ws.Cell(headerRow, c + 1);
                        cell.Value = colHeaders[c];
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontColor = headerFontColor;
                        cell.Style.Fill.BackgroundColor = headerBackColor;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    row++;

                    // 欄位內容
                    foreach (Column col in table.Columns)
                    {
                        ws.Cell(row, 1).Value = col.NAME;
                        ws.Cell(row, 2).Value = Blank(col.MS_Description);
                        ws.Cell(row, 3).Value = col.TYPE;
                        ws.Cell(row, 4).Value = col.IS_PK ? "✔" : string.Empty;
                        ws.Cell(row, 5).Value = col.DISALLOW_NULL ? "✔" : string.Empty;
                        ws.Cell(row, 6).Value = Blank(col.DEFUALT);
                        ws.Cell(row, 7).Value = Blank(col.REMARK);
                        row++;
                    }

                    // 欄位資料範圍加上框線
                    if (table.Columns.Count > 0)
                    {
                        var dataRange = ws.Range(headerRow, 1, row - 1, colHeaders.Length);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        // 主鍵/不為Null 置中
                        ws.Range(headerRow + 1, 4, row - 1, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    // 各物件之間空一列
                    row++;
                }

                // 自動調整欄寬
                ws.Columns().AdjustToContents();
            }

            // 若沒有任何資料表，至少建立一個空白工作表避免例外
            if (workbook.Worksheets.Count == 0)
            {
                workbook.Worksheets.Add("Overview");
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static string BuildSheetName(string? name, HashSet<string> usedNames)
        {
            // 移除 Excel 工作表名稱不允許的字元： \ / ? * [ ] :
            string cleaned = string.IsNullOrWhiteSpace(name) ? NoneSheetName : name;
            foreach (char invalid in SheetNameInvalidChars)
            {
                cleaned = cleaned.Replace(invalid, '_');
            }

            if (cleaned.Length > SheetNameMaxLen)
            {
                cleaned = cleaned[..SheetNameMaxLen];
            }

            // 確保唯一
            string candidate = cleaned;
            int suffix = 1;
            while (usedNames.Contains(candidate))
            {
                string suffixText = $"_{suffix++}";
                int maxBase = SheetNameMaxLen - suffixText.Length;
                candidate = (cleaned.Length > maxBase ? cleaned[..maxBase] : cleaned) + suffixText;
            }

            usedNames.Add(candidate);
            return candidate;
        }
    }
}
