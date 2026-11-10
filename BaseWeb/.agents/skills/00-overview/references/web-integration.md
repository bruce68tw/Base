# MVC 應用整合

來源：`Services/_WebExt.cs`、`Services/_Fun3.cs`、`BaseWeb.csproj`。先讀應用既有 Program.cs，依實際需求使用擴充，避免重複註冊整段服務或 middleware。

## 啟動擴充的實際範圍

- `builder.SetMyBuilder()`：設定 cookie policy、HTTPS gzip 壓縮及 CORS；直接讀取 `_Fun.Config.AllowOrigins.Split(',')`，呼叫前須已有可用的 Base 設定。Base 初始化的參數與成功條件以 `Base/Services/_Fun.cs` 為準，不在此臆造。
- `services.SetMyServices(bool multiLang)`：AddControllersWithViews 加全域 AutoValidateAntiforgeryTokenAttribute，設定 Newtonsoft member casing 與 System.Text.Json PropertyNamingPolicy=null，並註冊 IHttpContextAccessor。表單或 AJAX 寫入須依應用的 antiforgery 設定送 token。
- `multiLang=true` 時加入 view localization 與 Resources 路徑；目前方法只建立區域變數 RequestLocalizationOptions，沒有註冊它，也沒有呼叫 UseRequestLocalization。因此不能宣稱呼叫後已完成 request culture 設定。
- `app.SetMyApp(bool isDev)`：開發例外頁或 `/Home/Error` 加 HSTS、壓縮、HTTPS 導向、靜態檔案、routing、CORS、authentication、authorization，並映射 `{controller=Home}/{action=Login}/{id?}`；最後呼叫私有 SetAppSafe。不包含完整的應用認證方案註冊。

若元件探索失敗，先查使用端的 MVC application parts、BaseWeb 參照、元件命名與 TagHelper imports；不要一律新增重複服務註冊。類別庫專案本身沒有可直接執行的網站入口。

## CSP 與腳本

`SetAppSafe` 產生一次 nonce 存到靜態 `_Fun3.Nonce`，並在該次設定的 middleware 閉包共用；目前不是每個 request 產生 nonce。它設定 CSP 等 headers、再次 UseStaticFiles，最後 UseCookiePolicy。前面已註冊一次 UseStaticFiles，靜態檔案可能提前完成回應；修改 header 行為時須檢查 middleware 順序及實際回應。

`_Fun3.Script(string prog)` 回傳 script 的屬性片段（不是完整 script 標籤），含 module、nonce、`/jsView/{prog}.js?v=HHmmssfff`：

```cshtml
@using BaseWeb.Services
<script @_Fun3.Script("Order")></script>
```

確認應用有對應 JS 檔及 Razor 輸出。這是目前實作契約，並非安全設定已完整驗證的保證；調整 nonce 或 CSP 時需同步檢查 layout、inline script/style、前端事件派送器與回應標頭。不要為排除錯誤直接放寬為 unsafe-inline。
