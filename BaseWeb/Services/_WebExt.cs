using Base.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BaseWeb.Services
{
    public static class _WebExt
    {
        public static WebApplicationBuilder SetBuilder(this WebApplicationBuilder builder, string origin)
        {
            //set builder, 無作用!!
            //web.config 必須設定 <requestFiltering removeServerHeader="true" /> !!
            builder.WebHost.ConfigureKestrel(opts =>
            {
                opts.AddServerHeader = false;
            });

            //set services
            var services = builder.Services;
            services.Configure<CookiePolicyOptions>(opts =>
            {
                opts.MinimumSameSitePolicy = SameSiteMode.Strict;
                opts.Secure = CookieSecurePolicy.Always;
                opts.HttpOnly = HttpOnlyPolicy.Always;
            });

            //cors
            string[] origins = origin.Split(',');
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

        //資安: CSP, Anti-clickjacking
        public static IApplicationBuilder SetApp(this IApplicationBuilder app)
        {
            //var newId = _Str.NewId();
            var newId = Convert.ToBase64String(Guid.NewGuid().ToByteArray()); // 產生一次 nonce，兩者共用
            _Fun.Nonce = newId; //set global

            app.UseStaticFiles(new StaticFileOptions
            {
                /*
                OnPrepareResponse = ctx =>
                {
                    var headers = ctx.Context.Response.Headers;
                    headers["Referrer-Policy"] = "no-referrer";
                    headers["X-Content-Type-Options"] = "nosniff";
                    headers["X-Frame-Options"] = "DENY";
                    headers["X-XSS-Protection"] = "1; mode=block";     //舊但仍有用
                    headers.Remove("X-Powered-By");
                    headers.Remove("Server");   //無作用!!
                }
                */
            });

            /*
            app.Use(async (ctx, next) =>
            {
                //script, style使用nonce 讓前端使用 inline !!
                var headers = ctx.Response.Headers;
                headers["Content-Security-Policy"] =
                    "default-src 'self';" +
                    "connect-src 'self';" +
                    "font-src 'self';" +
                    "form-action 'self';" +
                    "frame-ancestors 'self';" +
                    "img-src 'self';" +
                    "media-src 'self';" +
                    "manifest-src 'self';" +
                    "object-src 'none';" +
                    "worker-src 'self';" + 
                    $"script-src 'self' 'nonce-{newId}';" +
                    $"style-src 'self' 'nonce-{newId}';";
                    //$"script-src 'self' 'unsafe-inline'; style-src 'self' 'nonce-{newId}'; ";

                headers["Referrer-Policy"] = "no-referrer";
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["X-XSS-Protection"] = "1; mode=block";     //舊但仍有用
                headers.Remove("X-Powered-By");
                headers.Remove("Server");
                await next();
            });
            */
            //
            app.UseCookiePolicy(); // 要啟用 cookie policy 才會生效
            return app;
        }
    }
}
