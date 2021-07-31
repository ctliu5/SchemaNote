﻿//#define SSL
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchemaNote.Models;
using System;

namespace SchemaNote {
  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
      services.Configure<CookiePolicyOptions>(options => {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
#if SSL
                options.CheckConsentNeeded = context => true;
#else
        options.CheckConsentNeeded = context => false;
#endif
        options.MinimumSameSitePolicy = SameSiteMode.None;
      });

      // 將 Session 存在 ASP.NET Core 記憶體中
      services.AddDistributedMemoryCache();

      services.AddSession(options => {
#if SSL
                //限制只有在 HTTPS 連線的情況下，才允許使用 Session。如此一來變成加密連線，就不容易被攔截。
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
#else
        //允許在 HTTP 連線的情況下，也使用 Session。建議只在受保護的網路內使用
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
#endif


        //沒必要將 Server 或網站技術的資訊爆露在外面，所以預設 Session 名稱 .AspNetCore.Session 可以改掉。
        options.Cookie.Name = "SchemaNote";
        //修改合理的 Session 到期時間。預設是 20 分鐘沒有跟 Server 互動的 Request，就會將 Session 變成過期狀態。
        options.IdleTimeout = TimeSpan.FromMinutes(5);
      });

      //採Singleton模式，使用強型別
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddSingleton<ISessionWrapper, SessionWrapper>();

      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }

      app.UseHttpsRedirection();

      app.UseStaticFiles();

      app.UseRouting();
      app.UseCookiePolicy();
      app.UseSession();

      app.UseAuthorization();

      app.UseEndpoints(endpoints => {
        endpoints.MapControllers();
        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
      });
    }
  }
}
