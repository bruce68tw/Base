using Base.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace BaseWeb.Services
{
    /// <summary>提供 ASP.NET Core builder 與 app 的 MVC、靜態檔案及中介軟體設定擴充方法。</summary>
    public static class _WebExt
    {
        /// <summary>
        /// 建立 WebApplication 前的共用入口：一次完成建置設定與 MVC 等服務註冊。
        /// </summary>
        /// <remarks>
        /// 呼叫前須先載入 _Fun.Config；multiLang 為 true 時也須提供 _Fun.Locales 與語系設定。
        /// 建議順序：載入設定 → SetMyBuilder(multiLang) → 註冊應用專屬服務 → Build。
        /// Build 後完成 _Fun.Init 與語系初始化，再呼叫 SetMyApp，最後由應用端 Run。
        /// 本入口依序整合原有兩步，呼叫後不要再呼叫無參數 SetMyBuilder 或 SetMyServices，以免重複註冊。
        /// 原有無參數 SetMyBuilder 與 SetMyServices 保留供其他應用相容使用，不應與本入口混用。
        /// 不在此呼叫 Build，讓應用端仍可註冊自己的服務；反向代理的現有設定亦由應用端保留。
        /// </remarks>
        /// <param name="builder">尚未 Build 且已載入共用設定的建置器。</param>
        /// <param name="multiLang">是否加入 MVC 視圖與資源在地化服務；沿用原 SetMyServices 的語意。</param>
        /// <returns>原建置器，供呼叫端繼續註冊應用服務及建立 WebApplication。</returns>
        public static WebApplicationBuilder SetMyBuilder(this WebApplicationBuilder builder, bool multiLang)
        {
            builder.SetMyBuilder2();
            builder.Services.SetMyServices(multiLang);
            return builder;
        }

        //因為是Ext函數, 所以名稱前面加上My方便辨視
        /// <summary>
        /// 建置前設定共用 Cookie 政策、回應壓縮與 CORS；只註冊選項及服務，不啟用請求管線。
        /// </summary>
        /// <remarks>
        /// 呼叫前須先載入 _Fun.Config（包含 AllowOrigins）；必須在 builder.Build() 前執行。
        /// 一般順序：載入設定 → SetMyBuilder → SetMyServices → 註冊應用專屬服務 → Build。
        /// 與 SetMyServices 同屬建置前階段，未來可整合；不要在此提早 Build，以保留應用端註冊機會。
        /// 若擴充反向代理，可在此讀取並驗證可信任 IP 清單及註冊選項；此函數目前尚未包含該功能。
        /// 相容入口：上列分開呼叫流程供既有專案使用；新專案可改用 SetMyBuilder(multiLang) 一次完成兩步。
        /// </remarks>
        /// <param name="builder">尚未呼叫 Build 的應用程式建置器。</param>
        /// <returns>原建置器，供呼叫端繼續設定。</returns>
        private static WebApplicationBuilder SetMyBuilder2(this WebApplicationBuilder builder)
        {
            //set builder, 無作用!!
            //web.config 必須設定 <requestFiltering removeServerHeader="true" /> !!
            /*
            builder.WebHost.ConfigureKestrel(opts =>
            {
                opts.AddServerHeader = false;
            });
            */

            //set services
            var services = builder.Services;
            services.Configure<CookiePolicyOptions>(opts =>
            {
                opts.MinimumSameSitePolicy = SameSiteMode.Strict;
                opts.Secure = CookieSecurePolicy.Always;
                opts.HttpOnly = HttpOnlyPolicy.Always;
            });

            //啟用壓縮
            services.AddResponseCompression(opts =>
            {
                opts.EnableForHttps = true;
                opts.Providers.Add<GzipCompressionProvider>();
            });
            services.Configure<GzipCompressionProviderOptions>(opts =>
            {
                opts.Level = System.IO.Compression.CompressionLevel.Fastest;
            });

            // proxy：讀取共用設定的反向代理 IP 清單
            if (_Fun.Config.ProxyIps.Count > 0)
            {
                services.Configure<ForwardedHeadersOptions>(opts =>
                {
                    opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    opts.ForwardLimit = 1;
                    foreach (var proxy in _Fun.Config.ProxyIps)
                    {
                        if (!IPAddress.TryParse(proxy, out var address))
                            throw new InvalidOperationException($"無效的代理 IP：{proxy}");

                        opts.KnownProxies.Add(address);
                    }
                });
            }

            //cors
            string[] origins = _Fun.Config.AllowOrigins.Split(',');
            services.AddCors(opts =>
            {
                opts.AddDefaultPolicy(a =>
                {
                    a.WithOrigins(origins);
                    a.AllowAnyHeader();
                    a.AllowAnyMethod();
                    a.AllowCredentials();
                });
            });

            return builder;
        }

        /// <summary>
        /// 建置前註冊 MVC、全域防偽驗證、JSON 格式與 HttpContext 存取服務，並選擇性加入在地化服務。
        /// </summary>
        /// <remarks>
        /// 與 SetMyBuilder 都須在 builder.Build() 前完成；呼叫後仍可註冊各應用專屬服務。
        /// 啟用多語系時會讀取 _Fun.Locales 與 _Fun.Config.Locale，呼叫端須先提供有效設定。
        /// 目前 RequestLocalizationOptions 僅建立於區域變數，尚未註冊該選項或啟用語系 middleware。
        /// 本函數負責服務註冊；Build 後的 middleware 與路由設定交由 SetMyApp。
        /// 相容入口：合併後的 SetMyBuilder(multiLang) 已包含本步驟，應用端無須再單獨呼叫。
        /// </remarks>
        /// <param name="services">建置器提供且尚未完成 Build 的服務集合。</param>
        /// <param name="multiLang">是否加入 MVC 視圖與資源在地化服務；不等同已啟用請求語系切換。</param>
        /// <returns>原服務集合，供呼叫端繼續註冊。</returns>
        private static IServiceCollection SetMyServices(this IServiceCollection services, bool multiLang)
        {
            //資安: controller 防止 CSRF
            var mvc = services.AddControllersWithViews(opts => { opts.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); })
                //services.AddControllersWithViews()
                //view Localization
                //.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                //use pascal for newtonSoft json
                .AddNewtonsoftJson(opts => { opts.UseMemberCasing(); })
                //use pascal for MVC json
                .AddJsonOptions(opts => { opts.JsonSerializerOptions.PropertyNamingPolicy = null; });

            //3.http context
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            if (multiLang)
            {
                //view Localization
                mvc.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

                //2.set Resources path
                services.AddLocalization(opts => opts.ResourcesPath = "Resources");

                //多國語初始化 if need
                //services.AddLocalization();

                List<CultureInfo> cultures = [];
                foreach (var item in _Fun.Locales)
                {
                    cultures.Add(new CultureInfo(item));
                }

                var options = new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture(_Fun.Config.Locale),
                    SupportedCultures = cultures,
                    SupportedUICultures = cultures,
                };
            }

            return services;
        }

        /// <summary>
        /// 建置後設定共用請求管線、預設 MVC 路由與安全標頭，不負責註冊 DI 服務或啟動主機。
        /// </summary>
        /// <remarks>
        /// 必須先完成所有服務註冊與 builder.Build()；依現有應用流程，再完成 _Fun.Init 及語系初始化後呼叫。
        /// 此處包含 HSTS、HTTPS 轉址與驗證；若部署於可信任反向代理後，須先呼叫 UseForwardedHeaders。
        /// 僅註冊 ForwardedHeadersOptions 不會啟用轉送處理；目前本函數未自行啟用該 middleware。
        /// 本函數呼叫完成後，由應用端呼叫 app.Run()。與建置前函數合併時，仍須保留 Build 前後的階段順序。
        /// </remarks>
        /// <param name="app">Build 後且已完成應用初始化的 WebApplication。</param>
        /// <param name="isDev">開發環境使用詳細錯誤頁；非開發環境使用錯誤路由與 HSTS。</param>
        /// <returns>原應用程式，供呼叫端繼續設定或啟動。</returns>
        public static WebApplication SetMyApp(this WebApplication app, bool isDev)
        {
            // Configure the HTTP request pipeline.
            if (isDev)
            {
                //app.UseMigrationsEndPoint();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();  //for https, default HSTS 30 days. for change see https://aka.ms/aspnetcore-hsts.
            }

            //反向代理
            if (_Fun.Config.ProxyIps.Count > 0)
                app.UseForwardedHeaders();

            //壓縮
            app.UseResponseCompression();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();      //加上後會套用到全域
            app.UseAuthentication();    //認証
            app.UseAuthorization();     //授權
            //app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Login}/{id?}");

            app.SetAppSafe();
            return app;
        }

        /// <summary>
        /// SetApp -> SetAppSafe
        /// 資安: CSP, Anti-clickjacking
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private static WebApplication SetAppSafe(this WebApplication app)
        {
            //var newId = _Str.NewId();
            var newId = Convert.ToBase64String(Guid.NewGuid().ToByteArray()); // 產生一次 nonce，兩者共用
            _Fun3.Nonce = newId; //set global

            //temp remark block for testing
            app.Use(async (ctx, next) =>
            {
                //script, style使用nonce 讓前端使用 inline !!
                //"img-src 'self' data:" bootstrap select 才能顯示右方箭頭 !!
                var headers = ctx.Response.Headers;
                headers.ContentSecurityPolicy =
                    "default-src 'self';" +
                    //"connect-src 'self';" +
                    "connect-src 'self' http://localhost:* ws://localhost:* wss://localhost:*;" + 
                    "font-src 'self';" +
                    "form-action 'self';" +
                    "frame-ancestors 'self';" +
                    "img-src 'self' data: blob:;" +
                    "media-src 'self';" +
                    "manifest-src 'self';" +
                    "object-src 'none';" +
                    "worker-src 'self';" + 
                    $"script-src 'self' 'nonce-{newId}';" +
                    $"style-src 'self' 'nonce-{newId}';";
                    //$"script-src 'self' 'unsafe-inline'; style-src 'self' 'nonce-{newId}'; ";

                headers["Referrer-Policy"] = "no-referrer";
                headers.XContentTypeOptions = "nosniff";
                headers.XFrameOptions = "DENY";
                headers.XXSSProtection = "1; mode=block";     //舊但仍有用
                headers.Remove("X-Powered-By");
                //headers.Remove("Server");
                await next();
            });

            //放後面, for 上面有設定 headers.XContentTypeOptions = "nosniff";
            app.UseStaticFiles(new StaticFileOptions
            {
                //temp remark block for testing
                OnPrepareResponse = ctx =>
                {
                    var headers = ctx.Context.Response.Headers;
                    headers["Referrer-Policy"] = "no-referrer";
                    headers.XContentTypeOptions = "nosniff";
                    headers.XFrameOptions = "DENY";
                    headers.XXSSProtection = "1; mode=block";     //舊但仍有用
                    headers.Remove("X-Powered-By");
                    //headers.Remove("Server");   //無作用!!
                }
            });

            // 要啟用 cookie policy 才會生效
            app.UseCookiePolicy(); 
            return app;
        }
    }
}
