# 小 Base 靜態服務

資料更新日期：2026-11-27

| 類別名稱 | 用途說明 |
| --- | --- |
| [_Address](../Services/_Address.cs) | 轉換地址中段、巷、弄的中文與阿拉伯數字表示；來源註記尚未完成實際驗證。 |
| [_Array](../Services/_Array.cs) | 將陣列組成字串，或將識別碼與文字配對轉成選項清單。 |
| [_Chart](../Services/_Chart.cs) | 查詢資料庫整數統計值，組成 Chart.js 一般圖表與分組圖表資料。 |
| [_Datatable](../Services/_Datatable.cs) | 從 DataTables 查詢輸入的 JSON 取得指定欄位值。 |
| [_Date](../Services/_Date.cs) | 計算日期區間與時間差、轉換西元及民國日期，並產生小時與分鐘選項。 |
| [_Db](../Services/_Db.cs) | 包裝 Db 的 SQL 查詢與命令執行，管理自行建立的連線；GetIdStrA、GetIdStrsA 為實例方法。 |
| [_Email](../Services/_Email.cs) | 將郵件模型轉成郵件訊息，透過 SMTP 寄送單封、多封或系統管理者郵件。 |
| [_File](../Services/_File.cs) | 讀寫文字、列舉檔案、建立 ZIP，處理路徑、檔名與上傳副檔名。 |
| [_Fun](../Services/_Fun.cs) | 初始化全域設定與 DI 入口，提供應用路徑、目前使用者及登入設定存取。 |
| [_Guid](../Services/_Guid.cs) | 建立 GUID 字串，在 GUID 與 22 字元 URL 友善表示間轉換。 |
| [_Json](../Services/_Json.cs) | 操作 Newtonsoft JSON 物件與陣列，處理資料列、主從結構、排序、篩選與模型轉換。 |
| [_Ldap](../Services/_Ldap.cs) | 透過 LDAP 驗證 AD 帳號密碼，讀取使用者姓名與電子郵件。 |
| [_List](../Services/_List.cs) | 組合字串或整數清單，查找選項識別碼，建立月份選項及檢查集合空值。 |
| [_Log](../Services/_Log.cs) | 將一般訊息、除錯、SQL 與錯誤寫入依日期分類的日誌檔。 |
| [_LogTime](../Services/_LogTime.cs) | 記錄各步驟與總耗時；使用共用靜態狀態，不隔離並行操作。 |
| [_Model](../Services/_Model.cs) | 透過 Newtonsoft JSON 與公開屬性反射轉換模型、讀寫屬性，組成操作與驗證錯誤結果。 |
| [_ModeUR](../Services/_ModeUR.cs) | 依指定資料表、欄位與鍵值查詢使用者、角色等關聯資料的選項。 |
| [_Net](../Services/_Net.cs) | 透過 Windows mpr.dll 連接或中斷需要帳密的網路共用路徑。 |
| [_Num](../Services/_Num.cs) | 解析數值與整數清單、轉換上傳暫存列號及版面欄數，產生亂數。 |
| [_Object](../Services/_Object.cs) | 判斷物件空值，透過反射讀寫公開屬性及取得型別名稱。 |
| [_Page](../Services/_Page.cs) | 建立簡易分頁輸入、限制每頁筆數，並設定分頁錯誤結果。 |
| [_Pwd](../Services/_Pwd.cs) | 依全域設定的密碼強度等級檢查密碼。 |
| [_Result](../Services/_Result.cs) | 依錯誤訊息與欄位錯誤判斷後端操作結果是否失敗。 |
| [_Seo](../Services/_Seo.cs) | 依網址資料產生 sitemap XML 的 UTF-8 位元組內容。 |
| [_Str](../Services/_Str.cs) | 處理字串補字元、分隔、Base64、十六進位、雜湊與識別碼，並提供既有 AES 格式加解密。 |
| [_Test](../Services/_Test.cs) | 回傳固定測試文字；RunSql 只回傳固定結果，不執行 SQL。 |
| [_Time](../Services/_Time.cs) | 同步暫停目前執行緒指定的毫秒數。 |
| [_Valid](../Services/_Valid.cs) | 檢查電子郵件、數字、文字、JSON 與表單輸入，整理欄位驗證錯誤。 |
| [_Var](../Services/_Var.cs) | 依物件字串表示判斷空值，或將 1 與 true 轉為布林真值。 |
| [_Xml](../Services/_Xml.cs) | 將 XML 節點序列化為 JSON，依 XPath 讀取屬性。 |

本表包含提供靜態工具方法的一般類別。`_LogTime` 位於 `Base` 命名空間，`_Net` 位於全域命名空間，其餘服務位於 `Base.Services`。

`_Fun` 為處理程序共用狀態，應用需依實際使用功能提供設定與 DI；不要在每次請求或租戶切換時重新初始化。
