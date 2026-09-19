using Microsoft.AspNetCore.Mvc;
using SchemaNote.Constants;
using SchemaNote.DataAccess;
using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using SchemaNote.Services;
using SchemaNote.ViewModels;
using System.Diagnostics;
using static SchemaNote.Constants.Common;

namespace SchemaNote.Controllers
{
    public class HomeController(ISessionWrapper sessionWapper, ICryptoService cryptoService, IExportService exportService, IConnectionInfoService connectionInfoService) : Controller
    {
        private readonly ISessionWrapper _sessionWapper = sessionWapper;
        private readonly ICryptoService _cryptoService = cryptoService;
        private readonly IExportService _exportService = exportService;
        private readonly IConnectionInfoService _connectionInfoService = connectionInfoService;
        private readonly DB_tool _db_tool = DB_tool.ADO_dot_NET;

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Overview(string ConnectionString, bool encrypted = false)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(ConnectionString))
                {
                    TempData["ErrorMessage"] = ConnStringMissing;
                    return RedirectToAction("Index");
                }
                // 由已儲存連線（localStorage）送來的字串為 AES 密文，需先在後端解密。
                if (encrypted)
                {
                    if (!_cryptoService.TryDecrypt(ConnectionString, out string decrypted) || string.IsNullOrEmpty(decrypted))
                    {
                        TempData["ErrorMessage"] = ConnStringMissing;
                        return RedirectToAction("Index");
                    }
                    ConnectionString = decrypted;
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
                // Post/Redirect/Get：連線成功後導向 GET Overview，避免重新整理時重複送出表單。
                return RedirectToAction("Overview");
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
                TempData["ErrorMessage"] = ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion

            DTO_Flag<OverviewViewModel> Flag = DB_Access.GetTables_Columns(ConnectionString, _db_tool);
            if (Flag.ResultType != ExceResultType.Success)
            {
                TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                return View();
            }
            SetCurrentConnectionViewData(ConnectionString);
            return View(Flag.OBJ);
        }

        // 從連線字串解析出 Server Address 與 Database Name（供前端比對 localStorage），
        // 並提供 AES 加密後的連線字串（前端只儲存密文，明文不外洩）。
        private void SetCurrentConnectionViewData(string connectionString)
        {
            var (server, database) = _connectionInfoService.Parse(connectionString);
            ViewData["CurrentServer"] = server;
            ViewData["CurrentDatabase"] = database;
            ViewData["CurrentConnectionString"] = _cryptoService.Encrypt(connectionString);
        }

        #region 匯出相關
        [HttpPost]
        public ActionResult ExportExtendedPropScript(int[]? objectIds = null)
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion

            DTO_Flag<System.Text.StringBuilder> Flag = DB_Access.ExportPropertiesScript(ConnectionString, _db_tool, objectIds: objectIds);
            if (Flag.ResultType != ExceResultType.Success)
            {
                TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                return RedirectToAction("Overview");
            }
            byte[] content = _exportService.Utf8WithBom(Flag.OBJ.ToString());
            string fileName = $"ExtendedPropScript_urlencoded_{DateTime.Now:yyyyMMddHHmmss}.sql";
            return File(content, "text/plain; charset=utf-8", fileName);
        }

        [HttpPost]
        public ActionResult DropAllExtendedProps(int[]? objectIds = null)
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion

            DTO_Flag<int> Flag = DB_Access.DropAllProperties(ConnectionString, _db_tool, objectIds: objectIds);
            if (Flag.ResultType != ExceResultType.Success)
            {
                TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                return RedirectToAction("Overview");
            }
            TempData["ErrorMessage"] = $"已刪除 {Flag.OBJ} 筆擴充屬性";
            return RedirectToAction("Overview");
        }

        [HttpPost]
        public ActionResult ExportMarkdown(int[]? objectIds = null)
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion

            DTO_Flag<OverviewViewModel> Flag = DB_Access.GetTables_Columns(ConnectionString, _db_tool);
            if (Flag.ResultType != ExceResultType.Success)
            {
                TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                return RedirectToAction("Overview");
            }

            _exportService.FilterTablesByObjectIds(Flag.OBJ, objectIds);

            string markdown = _exportService.BuildMarkdown(Flag.OBJ);
            byte[] content = _exportService.Utf8WithBom(markdown);
            string fileName = $"Overview_{DateTime.Now:yyyyMMddHHmmss}.md";
            return File(content, "text/markdown; charset=utf-8", fileName);
        }

        [HttpPost]
        public ActionResult ExportExcel(int[]? objectIds = null)
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = ConnStringMissing;
                return RedirectToAction("Index");
            }
            #endregion

            DTO_Flag<OverviewViewModel> Flag = DB_Access.GetTables_Columns(ConnectionString, _db_tool);
            if (Flag.ResultType != ExceResultType.Success)
            {
                TempData["ErrorMessage"] = Flag.ErrorMessagesHtmlString();
                return RedirectToAction("Overview");
            }

            _exportService.FilterTablesByObjectIds(Flag.OBJ, objectIds);

            byte[] content = _exportService.BuildExcel(Flag.OBJ);
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"Overview_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(content, contentType, fileName);
        }
        #endregion

        [HttpGet]
        public ActionResult Details(int? id)
        {
            #region check Connection
            string? ConnectionString = _sessionWapper.User.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = ConnStringMissing;
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
            SetCurrentConnectionViewData(ConnectionString);
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
                TempData["ErrorMessage"] = ConnStringMissing;
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
                TempData["ErrorMessage"] = ValidationMsg;
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
                TempData["ErrorMessage"] = Flag_prop.ErrorMessagesHtmlString();
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
