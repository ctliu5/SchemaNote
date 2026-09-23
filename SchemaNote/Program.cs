using SchemaNote.Services;

namespace SchemaNote
{
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

            // 將 Session 存在 ASP.NET Core 記憶體中
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                //允許在 HTTP 連線的情況下，也使用 Session。建議只在受保護的網路內使用
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                //沒必要將 Server 或網站技術的資訊爆露在外面，所以預設 Session 名稱 .AspNetCore.Session 可以改掉。
                options.Cookie.Name = "SchemaNote";
                //修改合理的 Session 到期時間。預設是 20 分鐘沒有跟 Server 互動的 Request，就會將 Session 變成過期狀態。
                options.IdleTimeout = TimeSpan.FromMinutes(5);
            });

            //採Singleton模式，使用強型別
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ISessionWrapper, SessionWrapper>();
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
            app.UseSession();

            app.UseAuthorization();

            app.MapControllers();
            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
