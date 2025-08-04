using Base.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BaseWeb.Services
{
    public static class WebExt
    {
        public static WebApplicationBuilder SetBuilder(this WebApplicationBuilder builder, string origin)
        {
            //set builder
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
            var newId = _Str.NewId();
            _Fun.Nonce = newId; //set global
            /* temp remark
            app.Use(async (context, next) =>
            {
                //script, style使用nonce 讓前端使用 inline !!
                context.Response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; img-src 'self'; object-src 'none'; frame-ancestors 'self'; " +
                    $"script-src 'self' 'nonce-{newId}'; style-src 'self' 'nonce-{newId}'; ";

                context.Response.Headers["Referrer-Policy"] = "no-referrer";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";     //舊但仍有用
                context.Response.Headers.Remove("X-Powered-By");
                await next();
            });
            */
            app.UseCookiePolicy(); // 要啟用 cookie policy 才會生效
            return app;
        }
    }
}
