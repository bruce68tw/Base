---
name: 01-service-static
description: 協助已引用或準備引用 BaseApi/BaseApi.csproj 的 ASP.NET Core 應用，依開發需求選用 BaseApi.Services 的底線服務並定位原始碼。
---

# BaseApi 底線服務索引

依需求選擇類別，實作前開啟來源核對應用所用版本的有效方法與呼叫條件。索引也包含提供靜態方法的一般類別。

| 類別 | 用途 | 原始碼 |
| --- | --- | --- |
| `_AuthFB` | 建立 Facebook OAuth 授權網址、以授權碼換取存取權杖並查詢使用者資料。 | [來源](../../../Services/_AuthFB.cs) |
| `_AuthGoogle` | 建立 Google OAuth 授權網址、以授權碼換取存取權杖並查詢使用者資料。 | [來源](../../../Services/_AuthGoogle.cs) |
| `_Device` | 目前為空類別，行動裝置判斷僅留在註解中。 | [來源](../../../Services/_Device.cs) |
| `_Fun2` | 提供網站目錄與上傳欄位常數，也可將支援定位的串流直接寫入目前的 HTTP 回應供下載。 | [來源](../../../Services/_Fun2.cs) |
| `_Html` | HTML 字元編碼、解碼，以及移除標籤與 script 區塊。 | [來源](../../../Services/_Html.cs) |
| `_Http` | 存取目前的 HTTP 請求、回應、Cookie、Session 與用戶端 IP，發送對外 HTTP 請求，或讀取 JWT 宣告。JWT 讀取方法只做解析，驗證由應用的驗證流程負責。 | [來源](../../../Services/_Http.cs) |
| `_HttpFile` | 讀取、檢查與儲存 `IFormFile` 上傳檔案，處理 CRUD 附件，或從既有檔案路徑建立供預覽或下載的 `FileResult`。 | [來源](../../../Services/_HttpFile.cs) |
| `_HttpSafe` | 將 `JObject` 轉為字串後進行 HTML 編碼。 | [來源](../../../Services/_HttpSafe.cs) |
| `_Locale` | 取得應用與目前使用者語系、設定 UI 語系，以及載入共用資源和查詢資源字串。 | [來源](../../../Services/_Locale.cs) |
| `_Login` | 處理帳密登入與登入紀錄，提供行動 App 的使用者 ID 與 IP 比對，也可簽發 JWT 並提供 JWT 驗證設定。 | [來源](../../../Services/_Login.cs) |
| `_XgFlow` | 依 `XpFlowNode` 與 `XpFlowLine` 定義建立簽核資料，並提供簽核紀錄的查詢、刪除與簽核處理。 | [來源](../../../Services/_XgFlow.cs) |
| `_XgFlow2` | 依 `XpUser.NextSignerId` 查找下一位簽核者並建立初始簽核資料。後續由啟用 `_Fun.UseNextSigner` 的 `_XgFlow.SignRowA` 接續處理。 | [來源](../../../Services/_XgFlow2.cs) |
