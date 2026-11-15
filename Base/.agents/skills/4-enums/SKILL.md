---
name: 4-enums
description: 在 Base 查找授權、查詢、欄位驗證或簽核代碼時，依檔案用途選擇並定位 C# 原始碼。
---

# Base 列舉與常數索引

依用途選擇檔案，開啟原始碼核對實際宣告與使用方式。

| 檔案 | 用途 |
| --- | --- |
| [AuthRangeEnum.cs](../../../Enums/AuthRangeEnum.cs) | 表示單一 CRUD 功能的資料存取範圍，由 _Auth.GetAuthRange 解析權限字串取得。 |
| [AuthTypeEnum.cs](../../../Enums/AuthTypeEnum.cs) | 功能授權的檢查層級，由 _Fun.AuthType 設定。 |
| [BrFormatEstr.cs](../../../Enums/BrFormatEstr.cs) | 唯讀欄位使用的前端格式資源鍵，對應 _BR.js 欄位。 |
| [CheckTypeEstr.cs](../../../Enums/CheckTypeEstr.cs) | CRUD 編輯欄位的驗證種類；實際檢查依 CrudEditSvc 實作。 |
| [ContentTypeEstr.cs](../../../Enums/ContentTypeEstr.cs) | HTTP 回應使用的內容類型字串。 |
| [CrudEnum.cs](../../../Enums/CrudEnum.cs) | CRUD 功能及權限字串的位置索引；AuthRow 到 Other 的數值對應字串位置。 |
| [DbColTypeEnum.cs](../../../Enums/DbColTypeEnum.cs) | Db 讀取資料列時使用的欄位轉換分類；此型別是整數常數類別。 |
| [DbTypeEnum.cs](../../../Enums/DbTypeEnum.cs) | 列出初始化及 SQL 方言選擇使用的資料庫種類。 |
| [FlowStatusEstr.cs](../../../Enums/FlowStatusEstr.cs) | 簽核流程整體的狀態代碼。 |
| [InputPatternEstr.cs](../../../Enums/InputPatternEstr.cs) | 輸入欄位的 pattern 設定；None 由前端欄位產生器視為不設 pattern。 |
| [InputTypeEstr.cs](../../../Enums/InputTypeEstr.cs) | 輸入欄位的前端 data-type 識別字串。 |
| [LineOpEstr.cs](../../../Enums/LineOpEstr.cs) | 簽核流程連線條件的比較運算代碼。 |
| [LocaleEstr.cs](../../../Enums/LocaleEstr.cs) | 定義語系識別碼常數。 |
| [LoginTypeEstr.cs](../../../Enums/LoginTypeEstr.cs) | 登入方式代碼，供組態與登入流程判斷使用。 |
| [ModelTypeEstr.cs](../../../Enums/ModelTypeEstr.cs) | Excel 匯入時使用的模型欄位轉換分類。 |
| [NodeTypeEstr.cs](../../../Enums/NodeTypeEstr.cs) | 簽核流程節點的種類代碼。 |
| [OrderTypeEnum.cs](../../../Enums/OrderTypeEnum.cs) | DataTables 查詢使用的排序方向。 |
| [OtpTypeEstr.cs](../../../Enums/OtpTypeEstr.cs) | 一次性驗證碼的驗證管道代碼；各管道是否可用依服務設定。 |
| [PatternEstr.cs](../../../Enums/PatternEstr.cs) | 字元驗證使用的正規表示式常數。 |
| [QitemOpEstr.cs](../../../Enums/QitemOpEstr.cs) | 查詢欄位的運算代碼，由 CrudReadSvc 組成查詢條件。 |
| [QitemTypeEnum.cs](../../../Enums/QitemTypeEnum.cs) | 查詢欄位的型別提示；日期種類可改用專用條件處理。 |
| [RandomTypeEnum.cs](../../../Enums/RandomTypeEnum.cs) | 指定 _Str.RandomStr 使用的隨機字元集合。 |
| [SignerTypeEstr.cs](../../../Enums/SignerTypeEstr.cs) | 簽核對象的指定方式代碼。 |
| [SignStatusEstr.cs](../../../Enums/SignStatusEstr.cs) | 個別簽核紀錄的狀態代碼。 |
| [TextTypeEstr.cs](../../../Enums/TextTypeEstr.cs) | 定義文字輸入與前端驗證使用的類型字串。 |
| [UpFileTypeEnum.cs](../../../Enums/UpFileTypeEnum.cs) | 分類上傳檔案副檔名，供 _File.TypeToExts 產生清單。 |
| [ValidTypeEstr.cs](../../../Enums/ValidTypeEstr.cs) | 前端欄位驗證的類型標記。 |
