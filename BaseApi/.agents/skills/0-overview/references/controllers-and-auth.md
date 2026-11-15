# 控制器與驗證授權

來源：`BaseApi/Controllers/BaseCtrl.cs`、`BaseApi/Attributes/`、`BaseApi/Services/_Login.cs`、`_Http.cs`。路由、登入頁與錯誤 View 由宿主提供。

## Controller 與回傳

BaseCtrl 繼承 MVC `Controller`，建構時用類別名稱移除 `Controller` 字串設定公開 `Ctrl` 欄位；不是 ControllerBase，也不自帶 ApiController、路由或授權屬性。以下 helper 都標示 NonAction：

| 方法 | 實際契約 |
| --- | --- |
| `JsonToCnt(JObject? json, bool emptyToMsg = true)` | null 預設轉為 `_Json.GetBrError("FindNone")`；false 時為空 JObject。回傳 JSON ContentResult，未自行設定錯誤 HTTP status。 |
| `JsonsToCnt(JArray? rows)` | null 變成 `[]`，以 JSON ContentResult 回傳。 |
| `FileDtoToResult(DownFileDto file)` | Error 空字串時使用 Stream、ContentType、FileName 回傳檔案，否則 BadRequest(Error)。 |
| `FileToResult(FileResult? file)` | null 為 NotFound，否則原樣回傳。 |

JSON 使用 Newtonsoft.Json.Linq；接收 JObject 或沿用現有 MVC JSON 契約時核對宿主 formatter，不假設預設 System.Text.Json 可直接替代。

## 登入與 JWT

`_Login.LoginByVoA(LoginVo vo, string sql = "", bool encodePwd = true, string extCol = "", string extCol2 = "", string extCol3 = "")` 回傳 bool，失敗訊息寫回 vo。預設 SQL 查 dbo.XpUser、XpDept，讀 UserId、UserName、Pwd、DeptId、DeptName；自訂 SQL 也需提供對應欄位與 @Account。LoginType 決定 AD 或密碼比對；密碼模式預設為 MD5 比對，不能將 encodePwd 誤稱為現代密碼雜湊策略。

成功後讀 `_Auth.GetAuthStrA(userId)`，建立 BaseUserDto，用隨機 clientKey 加 IP 寫快取，再寫 `_jwt` Cookie。SetCookie 固定 HttpOnly、Secure、SameSite.Strict，期限為 `_Fun.TimeOut` 分鐘；HTTP 開發站或跨站流程需實測 Cookie 是否傳送。此方法不自動 redirect，也不自動呼叫 LogOkA／LogFailA。

額外欄位參數會被組進 SQL，目前實作卻把參數字串本身指派到 BaseUserDto.ExtCol 等屬性，第三欄 SQL 別名還使用 ExtCol2。使用前核對實際版本與需求，勿承諾會傳回資料列的額外欄位值。

`LoginByUidA(string userId, Db? db = null)` 只查 dbo.UserApp 的 IP，回傳 1（相符）、0（不同）、-1（空值或不存在）；不建立 Cookie／快取，也不自行執行 2FA。LogOkA／LogFailA 使用 dbo.Login，鎖定分支更新 dbo.[User]；與 XpUser 命名不同，須先核對 schema。

`GetJwtAuthStr(string userId)` 建立 HMAC SHA256 token，Name claim 放入參數，期限依 TimeOut；GetJwtKey 使用 `_Fun.JwtKey`。GetJwtArg 開啟簽章及期限驗證，但不驗 issuer／audience，ClockSkew 也是 TimeOut 分鐘。應用需依自己的信任邊界配置並實際執行驗證，不將此 helper 當成完整 middleware。

`_Http.JwtToBr(string token = "")` 與 `JwtToUserId()` 使用 **ReadJwtToken 解析，沒有驗證簽章或期限**；預設從 Authorization 移除 `Bearer ` 後讀取 Name claim。非空但格式錯誤或缺 claim 可拋例外。JwtToBr 再將 claim 作為快取鍵的一部分，因此呼叫 GetJwtAuthStr 時的參數必須與快取寫入契約相符；單獨產生 token 不會建立登入資料。先由應用完成可靠驗證，再使用讀取功能。

## 權限與模型驗證

- `XgLoginAttribute`：UserId 空字串時 redirect 至 Home/Login，路由參數 `url` 為目前 Path + QueryString；不回傳標準 API 401。
- `XgProgAuthAttribute(CrudEnum crudEnum = CrudEnum.Empty, string ctrl = "")`：預設以路由 Controller 名稱配合 ProgAuthStrs 呼叫 `_Auth.CheckAuth`。有登入不等於有作業權限。
- XgProgAuth 依 action 宣告回傳型別選擇失敗結果，並會拆開 Task<T>：ActionResult 為登入 redirect 或 `~/Views/Shared/_Error.cshtml`；JsonResult 為含 `_ErrorMsg` 的 JSON；IActionResult 為 BadRequestObjectResult；其他型別為錯誤 ContentResult。泛型 ActionResult<T> 不應推定與 ActionResult 分支相同。調整 action 回傳型別可能改變前端收到的內容與狀態碼。
- `XgServerAttribute` 用 `_Http.GetIp()` 與 `_Auth.CheckClientIp`；JsonResult 分支包在 `Value._ErrorMsg`，且不像 XgProgAuth 會拆 Task<T>。不要假定兩個 filter 的錯誤契約一致。
- `XgReqAttribute` 繼承 RequiredAttribute，但 IsValid 在呼叫基底後才可能改寫 ErrorMessage，不能承諾自動提供完整多語系必填訊息。`XgStrLenAttribute(int maxLen)` 固定訊息 `max length={0}`。

模型驗證是否自動回應由宿主決定；繼承 BaseCtrl 或加上這些 DataAnnotations 不會自動檢查所有 action 的 ModelState。依應用實際 API／MVC 使用方式驗證成功與拒絕路徑。
