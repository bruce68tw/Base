using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BaseWeb.Services
{
    public static class _Vite
    {
        private static Dictionary<string, JsonElement>? _manifest;

        public static string GetAssetPath(string entry, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // 開發模式直接讀原始 ts/css
                return $"/Base/BaseFront/{entry}";
            }

            if (_manifest == null)
            {
                var manifestPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Base/BaseFront/dist", "manifest.json");
                var json = File.ReadAllText(manifestPath);
                _manifest = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            }

            if (_manifest != null && _manifest.TryGetValue(entry, out var value))
            {
                return "/Base/BaseFront/" + value.GetProperty("file").GetString();
            }

            throw new Exception($"Asset {entry} not found in manifest.");
        }
    }
}