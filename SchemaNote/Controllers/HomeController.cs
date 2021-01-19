using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using SchemaNote.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;

namespace SchemaNote.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISessionWrapper _sessionWapper;
        private readonly DB_tool _db_tool;

        public HomeController(ISessionWrapper sessionWapper)
        {
            _sessionWapper = sessionWapper;
            _db_tool = DB_tool.ADO_dot_NET;
        }

        public IActionResult Test(int times = 100)
        {
            #region check Connection
            string ConnectionString = _sessionWapper.User.SessionInfo_MiddlewareValue;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                TempData["ErrorMessage"] = Common.ConnStringMissing;
                return View("Index");
            }
            #endregion

            long ADO_dot_NET = 0, ADO_dot_NET_old = 0, ADO_dot_NET_old2 = 0, Dapper = 0;

            Stopwatch sw = new Stopwatch();

            for (int i = 0; i < times; i++)
            {
                sw.Start();
                DB_Access.GetTables_Columns(ConnectionString, DB_tool.ADO_dot_NET);
                sw.Stop();
                ADO_dot_NET += sw.ElapsedMilliseconds;
                sw.Reset();

                sw.Start();
                DB_Access.GetTables_Columns(ConnectionString, DB_tool.Dapper);
                sw.Stop();
                Dapper += sw.ElapsedMilliseconds;
                sw.Reset();
            }
            ViewData["ADO_dot_NET"] = ADO_dot_NET;
            ViewData["Dapper"] = Dapper;

            return View();
        }

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
                UserModel userModel = new UserModel();
                userModel.SetMiddlewareValue(ConnectionString);
                _sessionWapper.User = userModel;

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
            string ConnectionString = _sessionWapper.User.SessionInfo_MiddlewareValue;
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

        [HttpGet]
        public ActionResult Details(int? id)
        {
            #region check Connection
            string ConnectionString = _sessionWapper.User.SessionInfo_MiddlewareValue;
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
            string ConnectionString = _sessionWapper.User.SessionInfo_MiddlewareValue;
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
