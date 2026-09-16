using Microsoft.AspNetCore.Mvc;
using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using SchemaNote.ViewModels;
using System.Diagnostics;

namespace SchemaNote.Controllers
{
    public class HomeController(ISessionWrapper sessionWapper) : Controller
    {
        private readonly ISessionWrapper _sessionWapper = sessionWapper;
        private readonly DB_tool _db_tool = DB_tool.ADO_dot_NET;

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Overview(string ConnectionString)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(ConnectionString))
                {
                    TempData["ErrorMessage"] = Common.ConnStringMissing;
                    return RedirectToAction("Index");
                }
                UserModel userModel = new();
                userModel.SetConnectionString(ConnectionString);
                _sessionWapper.User = userModel;
                if (_sessionWapper.User.ConnectionString is not null) ConnectionString = _sessionWapper.User.ConnectionString;
                DTO_Flag<OverviewViewModel> Flag = DB_Access.GetTables_Columns(ConnectionString, _db_tool);
                if (Flag.ResultType != ExceResultType.Success)
                {
                    TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                    return RedirectToAction("Index");
                }
                return View(Flag.OBJ);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Overview()
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = Common.ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion

            DTO_Flag<OverviewViewModel> Flag = DB_Access.GetTables_Columns(ConnectionString, _db_tool);
            if (Flag.ResultType != ExceResultType.Success)
            {
                TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                return View();
            }
            return View(Flag.OBJ);
        }

        #region 匯出相關
        [HttpPost]
        public ActionResult ExportExtendedPropScript(int[]? objectIds = null)
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = Common.ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion

            DTO_Flag<System.Text.StringBuilder> Flag = DB_Access.ExportPropertiesScript(ConnectionString, _db_tool, objectIds: objectIds);
            if (Flag.ResultType != ExceResultType.Success)
            {
                TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                return RedirectToAction("Overview");
            }
            byte[] content = Utf8WithBom(Flag.OBJ.ToString());
            string fileName = $"ExtendedPropScript_urlencoded_{DateTime.Now:yyyyMMddHHmmss}.sql";
            return File(content, "text/plain; charset=utf-8", fileName);
        }

        private static byte[] Utf8WithBom(string text)
        {
            // 前置 UTF-8 BOM，確保 Windows 上開啟檔案時能正確辨識為 UTF-8。
            var encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            return encoding.GetPreamble().Concat(encoding.GetBytes(text)).ToArray();
        }

        // 依前端傳入的 OBJECT_ID 清單（搜尋/標籤過濾後仍顯示的項目）過濾要匯出的 Table/View。
        // 未提供清單時（例如直接呼叫或無過濾），維持匯出全部。
        private static void FilterTablesByObjectIds(OverviewViewModel model, int[]? objectIds)
        {
            if (model is null || objectIds is null || objectIds.Length == 0)
            {
                return;
            }

            var idSet = new HashSet<int>(objectIds);
            model.Tables = [.. model.Tables.Where(t => idSet.Contains(t.OBJECT_ID))];
        }

        [HttpPost]
        public ActionResult ExportMarkdown(int[]? objectIds = null)
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = Common.ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion

            DTO_Flag<OverviewViewModel> Flag = DB_Access.GetTables_Columns(ConnectionString, _db_tool);
            if (Flag.ResultType != ExceResultType.Success)
            {
                TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                return RedirectToAction("Overview");
            }

            FilterTablesByObjectIds(Flag.OBJ, objectIds);

            string markdown = BuildMarkdown(Flag.OBJ);
            byte[] content = Utf8WithBom(markdown);
            string fileName = $"Overview_{DateTime.Now:yyyyMMddHHmmss}.md";
            return File(content, "text/markdown; charset=utf-8", fileName);
        }

        private static string BuildMarkdown(OverviewViewModel model)
        {
            static string MdEscape(string? value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return string.Empty;
                }
                return value.Replace("|", "\\|").Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");
            }

            System.Text.StringBuilder sb = new();
            sb.AppendLine("# Overview");
            sb.AppendLine();

            foreach (Table item in model.Tables)
            {
                sb.AppendLine($"## {MdEscape(item.NAME)}");
                sb.AppendLine();
                sb.AppendLine($"> {MdEscape(item.MS_Description)}");
                sb.AppendLine();

                sb.AppendLine("| 物件類型 | 結構描述名稱 | 物件創建日期 | 物件修改日期 | 筆數 |");
                sb.AppendLine("| --- | --- | --- | --- | --- |");
                sb.AppendLine($"| {MdEscape(item.TYPE_NAME)} | {MdEscape(item.SCHEMA_NAME)} | {MdEscape(item.CREATE_DATE)} | {MdEscape(item.MODIFY_DATE)} | {item.QTY} |");
                sb.AppendLine();

                sb.AppendLine($"**備註：** {MdEscape(item.REMARK)}");
                sb.AppendLine();

                sb.AppendLine("| 欄位名稱 | 中文名稱 | 資料型態 | 主鍵 | 不為Null | 預設值 | 備註 |");
                sb.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
                foreach (Column col in item.Columns)
                {
                    sb.AppendLine($"| {MdEscape(col.NAME)} | {MdEscape(col.MS_Description)} | {MdEscape(col.TYPE)} | {(col.IS_PK ? "✔" : string.Empty)} | {(col.DISALLOW_NULL ? "✔" : string.Empty)} | {MdEscape(col.DEFUALT)} | {MdEscape(col.REMARK)} |");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        [HttpPost]
        public ActionResult ExportExcel(int[]? objectIds = null)
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = Common.ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion

            DTO_Flag<OverviewViewModel> Flag = DB_Access.GetTables_Columns(ConnectionString, _db_tool);
            if (Flag.ResultType != ExceResultType.Success)
            {
                TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                return RedirectToAction("Overview");
            }

            FilterTablesByObjectIds(Flag.OBJ, objectIds);

            byte[] content = BuildExcel(Flag.OBJ);
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"Overview_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(content, contentType, fileName);
        }

        private static byte[] BuildExcel(OverviewViewModel model)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();

            // 樣式常數
            var headerBackColor = ClosedXML.Excel.XLColor.FromHtml("#4472C4");
            var headerFontColor = ClosedXML.Excel.XLColor.White;
            var infoBackColor = ClosedXML.Excel.XLColor.FromHtml("#D9E1F2");

            // 用於工作表名稱唯一與長度限制（Excel 上限 31 字元）
            var usedSheetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Table table in model.Tables)
            {
                string sheetName = BuildSheetName(table.NAME, usedSheetNames);
                var ws = workbook.Worksheets.Add(sheetName);

                int row = 1;

                // 物件標題
                ws.Cell(row, 1).Value = table.NAME;
                ws.Range(row, 1, row, 7).Merge();
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Font.FontSize = 14;
                row++;

                // 物件中文說明
                ws.Cell(row, 1).Value = table.MS_Description;
                ws.Range(row, 1, row, 7).Merge();
                ws.Cell(row, 1).Style.Font.Italic = true;
                row++;

                // 物件資訊表頭
                string[] infoHeaders = ["物件類型", "結構描述名稱", "物件創建日期", "物件修改日期", "筆數"];
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
                ws.Cell(row, 1).Value = "備註";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Fill.BackgroundColor = infoBackColor;
                ws.Cell(row, 2).Value = table.REMARK;
                ws.Range(row, 2, row, 7).Merge();
                row += 2;

                // 欄位表頭
                string[] colHeaders = ["欄位名稱", "中文名稱", "資料型態", "主鍵", "不為Null", "預設值", "備註"];
                int headerRow = row;
                for (int c = 0; c < colHeaders.Length; c++)
                {
                    var cell = ws.Cell(headerRow, c + 1);
                    cell.Value = colHeaders[c];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = headerFontColor;
                    cell.Style.Fill.BackgroundColor = headerBackColor;
                    cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                }
                row++;

                // 欄位內容
                foreach (Column col in table.Columns)
                {
                    ws.Cell(row, 1).Value = col.NAME;
                    ws.Cell(row, 2).Value = col.MS_Description;
                    ws.Cell(row, 3).Value = col.TYPE;
                    ws.Cell(row, 4).Value = col.IS_PK ? "✔" : string.Empty;
                    ws.Cell(row, 5).Value = col.DISALLOW_NULL ? "✔" : string.Empty;
                    ws.Cell(row, 6).Value = col.DEFUALT;
                    ws.Cell(row, 7).Value = col.REMARK;
                    row++;
                }

                // 欄位資料範圍加上框線
                if (table.Columns.Count > 0)
                {
                    var dataRange = ws.Range(headerRow, 1, row - 1, colHeaders.Length);
                    dataRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                    // 主鍵/不為Null 置中
                    ws.Range(headerRow + 1, 4, row - 1, 5).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                }

                // 凍結表頭列並自動調整欄寬
                ws.SheetView.FreezeRows(headerRow);
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
            string cleaned = string.IsNullOrWhiteSpace(name) ? "Sheet" : name;
            foreach (char invalid in new[] { '\\', '/', '?', '*', '[', ']', ':' })
            {
                cleaned = cleaned.Replace(invalid, '_');
            }

            if (cleaned.Length > 31)
            {
                cleaned = cleaned[..31];
            }

            // 確保唯一
            string candidate = cleaned;
            int suffix = 1;
            while (usedNames.Contains(candidate))
            {
                string suffixText = $"_{suffix++}";
                int maxBase = 31 - suffixText.Length;
                candidate = (cleaned.Length > maxBase ? cleaned[..maxBase] : cleaned) + suffixText;
            }

            usedNames.Add(candidate);
            return candidate;
        }
        #endregion

        [HttpGet]
        public ActionResult Details(int? id)
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = Common.ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion
            else if (id == null)
            {
                return RedirectToAction("Index");
            }

            DTO_Flag<DetailsViewModel> Flag = DB_Access.GetTable_Columns(ConnectionString, (int)id, _db_tool);
            if (Flag.ResultType != ExceResultType.Success)
            {
                TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                return RedirectToAction("Overview");
            }
            return View(Flag.OBJ);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Details([FromRoute] int id, [FromForm] ICollection<VM_Property> model)
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = Common.ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion
            else if (model.Count == 0)
            {
                //沒有要新刪修的項目
                return Details(id);
            }
            else if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = Common.ValidationMsg;
                return Details(id);
            }

            DTO_Flag<int> Flag_prop = DB_Access.SaveProperties(ConnectionString, id, model, _db_tool);
            DTO_Flag<DetailsViewModel> Flag = DB_Access.GetTable_Columns(ConnectionString, id, _db_tool);

            if (Flag.ResultType != ExceResultType.Success)
            {
                if (Flag_prop.ResultType != ExceResultType.Success)
                {
                    TempData["ErrorMessage"] = Flag_prop.ErrorMessagesHtmlString() + "<br />" + Flag.ErrorMessagesHtmlString();
                }
                else
                {
                    TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                }
                return RedirectToAction("Overview");
            }
            else if (Flag_prop.ResultType != ExceResultType.Success)
            {
                ViewData["ErrorMessage"] = Flag_prop.ErrorMessagesHtmlString();
            }
            return View(Flag.OBJ);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
