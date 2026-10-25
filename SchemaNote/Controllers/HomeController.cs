using Microsoft.AspNetCore.Mvc;
using SchemaNote.Constants;
using SchemaNote.DataAccess;
using SchemaNote.Services;
using SchemaNote.ViewModels;
using System.Diagnostics;
using static SchemaNote.Constants.Common;

namespace SchemaNote.Controllers;

public class HomeController(IUserContext userContext, ICryptoService cryptoService, IExportService exportService, IConnectionInfoService connectionInfoService) : Controller
{
    private readonly IUserContext _userContext = userContext;
    private readonly ICryptoService _cryptoService = cryptoService;
    private readonly IExportService _exportService = exportService;
    private readonly IConnectionInfoService _connectionInfoService = connectionInfoService;
    private readonly DB_tool _db_tool = DB_tool.ADO_dot_NET;

    // 共用的連線處理：儲存 Session、嘗試連線，成功則導向 GET Overview（PRG 模式）。
    private RedirectToActionResult ConnectAndRedirect(string ConnectionString)
    {
        try
        {
            if (_userContext.Init(ConnectionString) is string connectionString) ConnectionString = connectionString;
            DB_Access.GetTables_Columns(ConnectionString, _db_tool);
            // Post/Redirect/Get：連線成功後導向 GET Overview，避免重新整理時重複送出表單。
            return RedirectToAction("Overview");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = FormatErrorMessage(ex);
            return RedirectToAction("Index");
        }
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

    private static string FormatErrorMessage(Exception ex)
    {
        return ex.Message.Replace(Environment.NewLine, "<br />");
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // 由前端送來的連線字串，嘗試連線並導向 Overview。
    [HttpPost]
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
            return ConnectAndRedirect(ConnectionString);
        }
        else
        {
            TempData["ErrorMessage"] = ConnStringNoData;
            return RedirectToAction("Index");
        }
    }

    // 讓使用者只需填 server / database / uid / pwd 四個欄位，後端組回 SQL Server 連線字串。
    [HttpPost]
    public ActionResult OverviewByFields(string Server, string Database, string Uid, string Pwd)
    {
        if (!ModelState.IsValid
            || string.IsNullOrWhiteSpace(Server)
            || string.IsNullOrWhiteSpace(Database)
            || string.IsNullOrWhiteSpace(Uid)
            || string.IsNullOrWhiteSpace(Pwd))
        {
            TempData["ErrorMessage"] = ConnStringNoData;
            return RedirectToAction("Index");
        }
        Microsoft.Data.SqlClient.SqlConnectionStringBuilder builder = new()
        {
            DataSource = Server,
            InitialCatalog = Database,
            UserID = Uid,
            Password = Pwd
        };
        return ConnectAndRedirect(builder.ConnectionString);
    }

    [HttpGet]
    public ActionResult Overview()
    {
        #region check Connection
        string ConnectionString = _userContext.GetConnectionString(out bool found);
        if (!found)
        {
            TempData["ErrorMessage"] = ConnStringMissing;
            return RedirectToAction("Index");
        }
        #endregion

        try
        {
            OverviewViewModel model = DB_Access.GetTables_Columns(ConnectionString, _db_tool);
            SetCurrentConnectionViewData(ConnectionString);
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = FormatErrorMessage(ex);
            return View();
        }
    }

    #region 匯出相關
    [HttpPost]
    public ActionResult ExportExtendedPropScript(int[]? objectIds = null)
    {
        #region check Connection
        string ConnectionString = _userContext.GetConnectionString(out bool found);
        if (!found)
        {
            TempData["ErrorMessage"] = ConnStringMissing;
            return RedirectToAction("Index");
        }
        #endregion
        var (server, database) = _connectionInfoService.Parse(ConnectionString);

        try
        {
            System.Text.StringBuilder script = DB_Access.ExportPropertiesScript(ConnectionString, _db_tool, objectIds: objectIds);
            byte[] content = _exportService.Utf8WithBom(script.ToString());
            string fileName = $"{server}[{database}]{DateTime.Now:yyyyMMddHHmmss}.sql";
            return File(content, "text/plain; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = FormatErrorMessage(ex);
            return RedirectToAction("Overview");
        }
    }

    [HttpPost]
    public ActionResult DropAllExtendedProps(int[]? objectIds = null)
    {
        #region check Connection
        string ConnectionString = _userContext.GetConnectionString(out bool found);
        if (!found)
        {
            TempData["ErrorMessage"] = ConnStringMissing;
            return RedirectToAction("Index");
        }
        #endregion

        try
        {
            int deletedCount = DB_Access.DropAllProperties(ConnectionString, _db_tool, objectIds: objectIds);
            TempData["ErrorMessage"] = $"已刪除 {deletedCount} 筆擴充屬性";
            return RedirectToAction("Overview");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = FormatErrorMessage(ex);
            return RedirectToAction("Overview");
        }
    }

    [HttpPost]
    public ActionResult DropAllFlags(int[]? objectIds = null)
    {
        #region check Connection
        string ConnectionString = _userContext.GetConnectionString(out bool found);
        if (!found)
        {
            TempData["ErrorMessage"] = ConnStringMissing;
            return RedirectToAction("Index");
        }
        #endregion

        try
        {
            int deletedCount = DB_Access.DropAllProperties(ConnectionString, _db_tool, objectIds: objectIds, propName: Flags);
            TempData["ErrorMessage"] = $"已刪除 {deletedCount} 筆標籤";
            return RedirectToAction("Overview");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = FormatErrorMessage(ex);
            return RedirectToAction("Overview");
        }
    }

    [HttpPost]
    public ActionResult ExportMarkdown(int[]? objectIds = null)
    {
        #region check Connection
        string ConnectionString = _userContext.GetConnectionString(out bool found);
        if (!found)
        {
            TempData["ErrorMessage"] = ConnStringMissing;
            return RedirectToAction("Index");
        }
        #endregion
        var (server, database) = _connectionInfoService.Parse(ConnectionString);

        try
        {
            OverviewViewModel model = DB_Access.GetTables_Columns(ConnectionString, _db_tool);
            _exportService.FilterTablesByObjectIds(model, objectIds);

            string markdown = _exportService.BuildMarkdown(model);
            byte[] content = _exportService.Utf8WithBom(markdown);
            string fileName = $"{server}[{database}]{DateTime.Now:yyyyMMddHHmmss}.md";
            return File(content, "text/markdown; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = FormatErrorMessage(ex);
            return RedirectToAction("Overview");
        }
    }

    [HttpPost]
    public ActionResult ExportExcel(int[]? objectIds = null)
    {
        #region check Connection
        string ConnectionString = _userContext.GetConnectionString(out bool found);
        if (!found)
        {
            TempData["ErrorMessage"] = ConnStringMissing;
            return RedirectToAction("Index");
        }
        #endregion
        var (server, database) = _connectionInfoService.Parse(ConnectionString);

        try
        {
            OverviewViewModel model = DB_Access.GetTables_Columns(ConnectionString, _db_tool);
            _exportService.FilterTablesByObjectIds(model, objectIds);

            byte[] content = _exportService.BuildExcel(model);
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"{server}[{database}]{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(content, contentType, fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = FormatErrorMessage(ex);
            return RedirectToAction("Overview");
        }
    }
    #endregion

    // 關閉連線：清除 Session 中的連線資訊，導回首頁。
    [HttpPost]
    public ActionResult CloseConnection()
    {
        _userContext.Clear();
        // Post/Redirect/Get：清除連線後導向 GET Index。
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult Details(int? id)
    {
        #region check Connection
        string ConnectionString = _userContext.GetConnectionString(out bool found);
        if (!found)
        {
            TempData["ErrorMessage"] = ConnStringMissing;
            return RedirectToAction("Index");
        }
        #endregion
        else if (id == null)
        {
            return RedirectToAction("Index");
        }

        try
        {
            DetailsViewModel model = DB_Access.GetTable_Columns(ConnectionString, (int)id, _db_tool);
            SetCurrentConnectionViewData(ConnectionString);
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = FormatErrorMessage(ex);
            return RedirectToAction("Overview");
        }
    }

    [HttpPost]
    public ActionResult Details([FromRoute] int id, [FromForm] ICollection<VM_Property> model)
    {
        #region check Connection
        string ConnectionString = _userContext.GetConnectionString(out bool found);
        if (!found)
        {
            TempData["ErrorMessage"] = ConnStringMissing;
            return RedirectToAction("Index");
        }
        #endregion
        else if (model.Count == 0)
        {
            //沒有要新刪修的項目
            return RedirectToAction("Details", new { id });
        }
        else if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = ValidationMsg;
            return RedirectToAction("Details", new { id });
        }

        try
        {
            DB_Access.SaveProperties(ConnectionString, id, model, _db_tool);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = FormatErrorMessage(ex);
        }
        // Post/Redirect/Get：儲存後導向 GET Details，避免重新整理時重複送出表單。
        return RedirectToAction("Details", new { id });
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
