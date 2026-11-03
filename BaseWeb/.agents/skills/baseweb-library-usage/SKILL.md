---
name: baseweb-library-usage
description: 在 ASP.NET Core MVC 應用整合 BaseWeb 類別庫，使用或調整 Xi 輸入欄位、Xg 工具 ViewComponents、_Input HTML 與 _WebExt 啟動擴充時使用。依目前 C# 實作確認參數及前端契約；不涵蓋 Base 的資料庫 CRUD 或 BaseApi 的授權實作。
---

# BaseWeb 類別庫使用

本 skill 原始檔維護於 BaseWeb 子專案，不代表參照 BaseWeb 的應用會自動載入它。以下原始碼路徑均相對於 `BaseWeb.csproj` 所在目錄；先定位該檔，再按需開啟來源。

## 依任務載入

- 建立或調整表單欄位：讀取 [Xi 欄位](references/xi-inputs.md)，並核對 `Models/Xi…Dto.cs`、`ViewComponents/Xi…ViewComponent.cs` 與 `Services/_Input.cs` 對應方法。
- 查詢、儲存、列表操作與選單：讀取 [Xg 工具](references/xg-components.md)，直接以元件的 `Invoke` 簽章確認參數。
- MVC 服務、middleware、CSP 或腳本載入：讀取 [應用整合](references/web-integration.md)，核對 `Services/_WebExt.cs`、`Services/_Fun3.cs`。

## 工作邊界

- `BaseWeb.csproj` 採 Web SDK、`net8.0`、`OutputType=Library`，參照 BaseApi 與 Base；不可當成有 Program.cs 的獨立網站啟動。
- `Helpers_old/**` 已從專案移除，不以其 API 作為新實作依據。舊檔、註解和 DTO 說明均須與實際輸出核對。
- 元件多回傳 `HtmlString`，Xi 多委派給 `_Input`。修改共用輸出時檢查所有呼叫方，保留 `data-fid`、`data-type`、`data-init`、`data-edit` 與驗證定位 class 的用途。
- 本庫產生 HTML，但前端事件派送器、CRUD 物件、CSS 與日期等插件須到使用端確認。不可因生成按鈕便宣稱功能可運作，也不從未使用的 `zz_` 目錄推導現行前端契約。
- HTML 大量使用字串插值，不能假設 Razor 會再次編碼 `HtmlString`。新增動態值時區分文字、屬性與刻意允許的 HTML；檢查實際輸出，避免直接插入未編碼資料。

## 驗證

純 skill 文件更新檢查連結、原始碼依據與 skill 格式即可。程式變更在 BaseWeb 目錄執行 `dotnet build .\BaseWeb.csproj`，已有可用還原資料時可用 `--no-restore`；結果包括相依專案，應區分還原、建置及應用端問題。涉及 Razor 探索或前端互動時另在使用端確認渲染、事件與提交資料；類別庫建置不能證明這些行為。未執行的項目明確記錄。
