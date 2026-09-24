using Microsoft.AspNetCore.Authentication.Cookies;
using SchemaNote.Constants;
using SchemaNote.Services;

namespace SchemaNote;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => false;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        // 改用 Cookie-Authentication，連線資訊（ConnectionString）以 Claims 形式存放在加密的登入 Cookie 中。
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                //允許在 HTTP 連線的情況下，也使用 Cookie。建議只在受保護的網路內使用
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                //沒必要將 Server 或網站技術的資訊爆露在外面，所以自訂 Cookie 名稱。
                options.Cookie.Name = "SchemaNote";

                //修改合理的到期時間。滑動到期：只要在時間內有互動就會自動延長。
                options.ExpireTimeSpan = Common.SessionLifetime;
                options.SlidingExpiration = true;
            });

        //採Singleton模式，使用強型別
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IUserContext, CookieUserContext>();
        services.AddSingleton<ICryptoService, AesCryptoService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<IConnectionInfoService, ConnectionInfoService>();

        #region CSRF 防護
        services.AddControllersWithViews(options =>
        {
            // 全站啟用 CSRF 防護，所有 POST/PUT/PATCH/DELETE 都會自動驗證 AntiForgeryToken
            options.Filters.Add<Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute>();
        });
        builder.Services.AddAntiforgery(options =>
        {
            // 自訂驗證用的表單欄位名稱（前端 site.js 會透過 <meta name="csrf-form-field-name"> 動態讀取此值）
            options.FormFieldName = "__RequestVerificationToken";
            // 自訂驗證用的標頭名稱（前端 site.js 會透過 <meta name="csrf-header-name"> 動態讀取此值）
            options.HeaderName = "RequestVerificationToken";
        });
        #endregion

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();
        app.UseCookiePolicy();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
