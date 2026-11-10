---
name: baseapi-overview
description: 協助 C# ASP.NET Core 應用使用此 Base 儲存庫的 BaseApi/BaseApi.csproj 類別庫，整合 Controller、目前使用者、登入權限、HTTP 與檔案服務。適用於已引用或準備引用此特定程式庫的應用；不涵蓋其他同名 API 套件、BaseWeb 元件或 BaseAI 功能。
---

# BaseApi 程式庫應用端整合

以應用端正確使用現有 API 為目標，保留應用既有啟動、路由與驗證設計。溝通與新增文件使用繁體中文，識別名稱維持原樣。

## 定位與版本

本 skill 維護於大 Base 儲存庫的 `BaseApi/.agents/skills/00-overview/`；從 skill 目錄往上三層是 BaseApi 子專案，往上四層是儲存庫根目錄。本機建立位置為 `C:/_project/Base/BaseApi`，搬移後以實際 checkout 為準。下列 references 的文件連結相對於文件；反引號內的來源路徑相對於大 Base 儲存庫根目錄。

先核對應用 `.csproj`、組件版本、啟動程式與呼叫點。`BaseApi.csproj` 為 `Microsoft.NET.Sdk` 的 `net8.0` 類別庫，參照 `../Base/Base.csproj`，沒有自動啟動應用或註冊服務的入口。依應用位置設定 ProjectReference；未確認 NuGet 發佈名稱，勿自行猜測套件。若只有 DLL，先以參考文件掌握契約，再用實際組件中繼資料或編譯確認簽章，不假設應用有來源路徑。

目前 csproj 沒有 `Compile Remove`，但 `.cs_old`、`.cs_op` 等不是預設 C# 編譯來源；`None Remove` 也不會把它們變成現行程式。另須排除 `.cs` 內註解掉的舊方法。後續以實際 csproj 為準，略過根目錄 `zz_` 目錄。

## 按需求讀取

- 新接入、DI、初始化、請求或登入資料缺失：讀取 [初始化與目前使用者](references/integration.md)。
- Controller 回傳、登入、JWT、權限或模型驗證：讀取 [控制器與驗證授權](references/controllers-and-auth.md)。
- 上下載、語系、Xp 系列服務、簽核與驗證方式：讀取 [服務與整合檢查](references/services-and-validation.md)。
- 開發 ASP.NET Core 應用時，依需求選用 BaseApi 的既有底線服務並查找原始碼，可查閱 [底線服務索引](../01-service-static/SKILL.md)。

## 使用原則

先完成應用實際需要的接線，不因引用 BaseApi 就全面啟用 Session、JWT、資料庫或簽核。`_Fun` 是處理程序共用靜態狀態，不要在每個請求或租戶重設。HTTP 工具需要有效 HttpContext，不能直接搬到背景工作使用。

`BaseCtrl` 不會自動加上登入或授權限制；`JwtToBr` 等讀取 JWT 的方法也不代表已驗證權杖。依實際來源確認身份信任與錯誤回應契約。Xp 系列服務內建特定資料表與欄位，不將它們視為任意 schema 的通用服務。

Base 的 DB、快取與 CRUD 細節可在來源存在時按需讀取相鄰 `Base/.agents/skills/00-overview/`，但本 skill 的必要整合契約已收於 references。程式參照不代表已載入 skill；從大 Base 工作時需按需取得本檔。此目錄是共用原始檔，不包含安裝、發佈或插件設定。
