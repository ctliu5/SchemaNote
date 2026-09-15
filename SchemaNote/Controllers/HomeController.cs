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

        [HttpPost]
        public ActionResult ExportExtendedPropScript()
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = Common.ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion

            DTO_Flag<System.Text.StringBuilder> Flag = DB_Access.ExportPropertiesScript(ConnectionString, _db_tool);
            if (Flag.ResultType != ExceResultType.Success)
            {
                TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                return RedirectToAction("Overview");
            }
            return Content(Flag.OBJ.ToString(), "text/plain", System.Text.Encoding.UTF8);
            //return Content(Flag.OBJ.ToString(), "text/plain", System.Text.Encoding.Unicode);
        }

        [HttpPost]
        public ActionResult ExportMarkdown()
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

            string markdown = BuildMarkdown(Flag.OBJ);
            return Content(markdown, "text/markdown", System.Text.Encoding.UTF8);
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
