using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SchemaNote.Constants;

namespace SchemaNote.Filters;

// 防偽權杖驗證失敗（常見原因：登入 Cookie 到期，權杖綁定的身分與目前身分不一致）時，
// 不回傳 400，改以 PRG 方式導回首頁並顯示提示訊息。
public class AntiforgeryRedirectFilter(ITempDataDictionaryFactory tempDataFactory) : IAlwaysRunResultFilter
{
    private readonly ITempDataDictionaryFactory _tempDataFactory = tempDataFactory;

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is IAntiforgeryValidationFailedResult)
        {
            ITempDataDictionary tempData = _tempDataFactory.GetTempData(context.HttpContext);
            tempData["ErrorMessage"] = Common.ConnStringMissing;
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
