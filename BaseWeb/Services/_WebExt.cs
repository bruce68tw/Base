using Base.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BaseWeb.Services
{
    public static class _WebExt
    {
        public static WebApplicationBuilder SetBuilder(this WebApplicationBuilder builder)
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

        public static IServiceCollection SetServices(this IServiceCollection services, bool multiLang)
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

        public static WebApplication SetApp(this WebApplication app, bool isDev)
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
            _Fun.Nonce = newId; //set global

            app.UseStaticFiles(new StaticFileOptions
            {
                //temp remark
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
                //
            });

            //temp remark
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
            //

            app.UseCookiePolicy(); // 要啟用 cookie policy 才會生效
            return app;
        }
    }
}
