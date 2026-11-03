# 服務與整合檢查

來源以大 Base 儲存庫根目錄為基準。以下是查找與整合決策線索，新增呼叫仍需核對實際簽章與註解區塊。

## HTTP、檔案與語系

來源：`BaseApi/Services/_Http.cs`、`_HttpFile.cs`、`_Fun2.cs`、`_Locale.cs`。

- `_Http.GetRequest/GetResponse/GetSession` 都依賴目前請求；GetWebUrl 包含 Scheme、Host、Path、QueryString，GetWebPath 為 Path + QueryString。GetUrlResultA 是對外 HTTP 呼叫入口，應依目前實作確認參數格式、錯誤及資源釋放，不從註解中的舊同步方法推定功能。
- `_HttpFile.CheckFileSize(IFormFile file, int size)` 的 size 單位為 MB；CheckFileExt 使用不含句點、逗號分隔的副檔名清單。SaveFileA 會建立目錄並用 FileMode.Create 寫入，失敗記錄後回傳 false，不會先自動執行上述大小／副檔名檢查。應用先提供受控儲存位置與檔案驗證，核對既有檔案覆寫行為。
- `SaveFileByNewIdA` 使用新 ID 加原副檔名，回傳檔名，失敗為空字串。SaveFileAsync 的舊範例在註解中，不應引用為現行入口。
- `SaveCrudFilesA` 要求 inputJson 的 `_fileJson`、serverFid 的 `txx_fid` 格式與 Base CRUD 回傳的 newKey 對應；isMulti 預設 true，會依序在 serverFid 後加序號。單檔 wrapper 使用 false。新資料鍵從 `newKey[txx][fN]` 取得。先核對應用 JSON 與前端欄位命名，勿自行猜測範例結構。
- SaveCrudFilesA 逐檔呼叫 SaveFileA 卻未檢查其 bool；整體 true 不足以證明所有檔案寫入成功，也沒有 DB／檔案系統的統一交易。檔案流程須驗證實際寫入與失敗清理行為。
- `ViewFile(string path, string downFileName = "")` 可配合 BaseCtrl.FileToResult 使用；傳入路徑由應用控制。`_Fun2.ExportByStreamA(Stream stream, string fileName)` 直接寫目前 Response，將 stream.Position 設為 0，需可 seek 的 stream；先完成文件產生再匯出。
- `_Locale.SetCultureA(string locale)` 讀取 `_Fun2.DirWeb + "locale/{locale}/BR.json"` 至 BaseResDto，失敗回傳 false。MultiLang 時設定 CurrentUICulture，否則設定 DefaultThreadCurrentUICulture；不設定 CurrentCulture。應用需提供實際 BR.json、語系選擇及請求流程，呼叫此方法不等於已更新使用者的 Locale 或語系 Cookie。

## Xp 與其他服務

| 需求 | 查證入口與邊界 |
| --- | --- |
| 使用者 | `BaseApi/Services/XgUserSvc.cs` 的 FindUsersA 查 dbo.XpUser，帳號／姓名採前綴查詢、不分頁。XgUserRead 使用 CrudReadSvc；XgUserEdit 定義 XpUser 與 XpUserRole，Pwd 設為空字串，使用前核對建立／更新是否會改寫密碼。 |
| 作業、角色、匯入、報表、交易紀錄 | 同目錄 XgProgRead/Edit、XgRoleRead/Edit、XgImportRead、XgEasyRptRead/Edit、XgTranLogRead。Read 類別多委派 Base CrudReadSvc，仍依 controller 對應的讀取設定；EditDto 綁定既有資料表，不是自動產生 schema 的工具。 |
| 簽核 | `_XgFlow.cs`、`_XgFlow2.cs`、XgFlowRead/Edit。兩種流程同時存在，第二種註解要求 XpUser.NextSignerId，且在 Init 後設 `_Fun.UseNextSigner = true`；需核對應用呼叫點如何選擇流程，不假設旗標會自動路由所有呼叫。BaseFlow 的 Razor 簽核 UI 屬另一模組。 |
| OTP | XgOtpSvc 的 SendEmailA、SendSmsA、SendAuthA、CheckAuthA；涉及 XpServer、XpOtpLog、Config 的 OTP 設定與模板，SMS 需 ISmsSvc 實作。不能只靠 Otp.NET／QRCoder 的套件參照宣稱寄送及驗證流程已完成。 |
| Google／Facebook | `_AuthGoogle.cs`、`_AuthFB.cs` 提供 Init、GetAuthUrl、CodeToTokenA、GetUserA 等外部 OAuth helper。應用自行維護回呼、狀態驗證與登入映射；有程式碼不代表外部端點與供應商設定現今可用，實際接入時查供應商官方文件並驗證。 |

簽核尚需確認 XpFlow、XpFlowNode、XpFlowLine、XpFlowMap、XpFlowSign 等 schema、作業代碼及使用者資料。`_XgFlow.SqlDeptRole` 在靜態欄位初始化時讀 `_Fun.DeptId()`，會捕捉當時部門，不能假定每次請求自動更新。DeleteSignA 來源包含 `and m SourceId=@SourceId`，呼叫前須處理此 SQL 語法疑點並實測；本知識整理不修改程式碼或宣稱流程可直接執行。

## 驗證與回報

本庫未有已確認的統一宿主或自動測試命令。純文件變更檢查來源、連結與 skill 格式即可；應用接入或程式變更時以受影響的應用 csproj 及 BaseApi/Base 參照範圍建置。從 BaseApi 目錄可用 `dotnet build ./BaseApi.csproj`，具備相符還原資產時才考慮 `--no-restore`；不推定其他專案已驗證的建置結果適用此處。

依變更驗證可觀察行為：請求能解析目前使用者、Cookie 登入與失效、JWT 拒絕錯誤簽章／逾期、未登入／無權限的實際 status 與內容、代理後 IP／快取一致性、上下載及語系檔讀取。DB、OTP、外部 OAuth 與簽核只在對應環境備妥且已授權時進行整合測試。

回報分開列出實際執行的格式／文件檢查、編譯、請求測試與外部整合；未執行的項目明確標記。編譯成功不能證明登入、SQL 或外部服務正確。
