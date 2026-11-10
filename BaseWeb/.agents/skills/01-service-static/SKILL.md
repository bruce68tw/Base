---
name: 01-service-static
description: 協助已引用或準備引用 BaseWeb/BaseWeb.csproj 的應用，依MVC 欄位、頁面腳本與應用啟動設定需求選用 BaseWeb.Services 的靜態服務並定位原始碼。
---

# BaseWeb 靜態服務索引

依需求選擇類別，實作前核對應用所用版本的原始碼與呼叫條件。

| 類別 | 用途 | 原始碼 |
| --- | --- | --- |
| `_Fun3` | 產生 Razor 頁面載入 `/jsView/` 模組腳本所用的屬性，包含 nonce 與時間戳記查詢參數。 | [來源](../../../Services/_Fun3.cs) |
| `_Input` | 讀取 MVC 欄位中繼資料與值，依 Xi DTO 產生輸入欄位、選項及驗證屬性的 HTML。 | [來源](../../../Services/_Input.cs) |
| `_WebExt` | 提供 ASP.NET Core builder 與 app 擴充方法，集中設定 MVC 服務、選用的本地化及應用的 middleware 與路由。 | [來源](../../../Services/_WebExt.cs) |
