# 小 Base 列舉與常數

資料更新日期：2026-11-27

| 類別名稱 | 用途說明 |
| --- | --- |
| [AuthRangeEnum](../Enums/AuthRangeEnum.cs) | 定義無權限、個人、部門與全部資料的存取範圍代碼。 |
| [AuthTypeEnum](../Enums/AuthTypeEnum.cs) | 功能授權的檢查層級，由 _Fun.AuthType 設定。 |
| [BrFormatEstr](../Enums/BrFormatEstr.cs) | 唯讀欄位使用的前端格式資源鍵，對應 _BR.js 欄位。 |
| [CheckTypeEstr](../Enums/CheckTypeEstr.cs) | 定義編輯欄位的電子郵件、網址及數值範圍等驗證種類代碼。 |
| [ContentTypeEstr](../Enums/ContentTypeEstr.cs) | HTTP 回應使用的內容類型字串。 |
| [CrudEnum](../Enums/CrudEnum.cs) | CRUD 功能及權限字串的位置索引；AuthRow 到 Other 的數值對應字串位置。 |
| [DbColTypeEnum](../Enums/DbColTypeEnum.cs) | Db 讀取資料列時使用的欄位轉換分類；此型別是整數常數類別。 |
| [DbTypeEnum](../Enums/DbTypeEnum.cs) | 資料庫種類，供初始化及 SQL 方言選擇使用；列舉存在不代表全部功能均已支援。 |
| [InputPatternEstr](../Enums/InputPatternEstr.cs) | 輸入欄位的 pattern 設定；None 由前端欄位產生器視為不設 pattern。 |
| [InputTypeEstr](../Enums/InputTypeEstr.cs) | 輸入欄位的前端 data-type 識別字串。 |
| [LocaleEstr](../Enums/LocaleEstr.cs) | 語系識別碼常數；不表示應用已提供相應翻譯。 |
| [LoginTypeEstr](../Enums/LoginTypeEstr.cs) | 登入方式代碼，供組態與登入流程判斷使用。 |
| [ModelTypeEstr](../Enums/ModelTypeEstr.cs) | Excel 匯入時使用的模型欄位轉換分類。 |
| [OrderTypeEnum](../Enums/OrderTypeEnum.cs) | DataTables 查詢使用的排序方向。 |
| [OtpTypeEstr](../Enums/OtpTypeEstr.cs) | 一次性驗證碼的驗證管道代碼；各管道是否可用依服務設定。 |
| [PatternEstr](../Enums/PatternEstr.cs) | 字元驗證使用的正規表示式常數。 |
| [QitemOpEstr](../Enums/QitemOpEstr.cs) | 定義等值、前綴、包含、清單、空值及區間等查詢運算代碼。 |
| [QitemTypeEnum](../Enums/QitemTypeEnum.cs) | 查詢欄位的型別提示；日期種類可改用專用條件處理。 |
| [RandomTypeEnum](../Enums/RandomTypeEnum.cs) | _Str.RandomStr 使用的隨機字元集合，並非密碼學安全亂數設定。 |
| [TextTypeEstr](../Enums/TextTypeEstr.cs) | 文字輸入與前端驗證使用的類型字串；不保證後端套用同名驗證。 |
| [UpFileTypeEnum](../Enums/UpFileTypeEnum.cs) | 上傳檔案的副檔名分類，供 _File.TypeToExts 產生清單；不驗證檔案內容。 |
| [ValidTypeEstr](../Enums/ValidTypeEstr.cs) | 前端欄位驗證的類型標記。 |
