---
name: 01-service-static
description: 協助已引用或準備引用 Base/Base.csproj 的 C# 應用，依需求選用小 Base 的 Services 靜態服務並定位原始碼。
---

# 小 Base 靜態服務索引

依需求選擇類別，實作前查閱應用所用版本的原始碼。索引涵蓋 `Base.csproj` 編譯的公開靜態類別與提供公開靜態方法的一般類別。

| 類別 | 用途 | 原始碼 |
| --- | --- | --- |
| `_Address` | 轉換地址中段、巷、弄的數字表示。 | [來源](../../../Services/_Address.cs) |
| `_Array` | 將陣列組成字串，或依索引轉為 ID／文字項目。 | [來源](../../../Services/_Array.cs) |
| `_Auth` | 檢查用戶端 IP、作業與資料權限，取得權限範圍及選單。 | [來源](../../../Services/_Auth.cs) |
| `_Cache` | 透過應用設定的快取服務讀寫字串、刪除鍵與重載資料庫快取。 | [來源](../../../Services/_Cache.cs) |
| `_Chart` | 執行查詢並整理圖表資料與分組資料。 | [來源](../../../Services/_Chart.cs) |
| `_Code` | 將 SQL、代碼表或型別資料轉為選項，也提供使用者、角色及作業等既有選項。 | [來源](../../../Services/_Code.cs) |
| `_Datatable` | 從 DataTables 查詢的 JSON 輸入取得指定欄位值。 | [來源](../../../Services/_Datatable.cs) |
| `_Date` | 計算日期與時間差、檢查日期範圍，以及轉換日期與時間格式。 | [來源](../../../Services/_Date.cs) |
| `_Db` | 執行 SQL 查詢與命令，並管理由工具開啟的資料庫連線。 | [來源](../../../Services/_Db.cs) |
| `_Email` | 將郵件 DTO 轉成郵件訊息並寄送，亦提供寄信給系統管理者的入口。 | [來源](../../../Services/_Email.cs) |
| `_File` | 處理檔名與路徑、讀寫文字、列舉檔案及建立 ZIP。 | [來源](../../../Services/_File.cs) |
| `_Fun` | 初始化共用環境，取得全域設定、應用路徑與目前使用者資料。 | [來源](../../../Services/_Fun.cs) |
| `_Guid` | 建立 GUID 字串，並在 GUID 與 22 字元的 URL 友善表示之間轉換。 | [來源](../../../Services/_Guid.cs) |
| `_Json` | 操作 Newtonsoft JSON 物件與陣列，處理分頁資料、篩選及欄位轉換。 | [來源](../../../Services/_Json.cs) |
| `_Ldap` | 透過 LDAP 驗證帳號密碼。 | [來源](../../../Services/_Ldap.cs) |
| `_List` | 產生月份清單、串接集合內容及處理 ID 清單。 | [來源](../../../Services/_List.cs) |
| `_Log` | 記錄一般訊息、除錯、SQL 與錯誤資訊。 | [來源](../../../Services/_Log.cs) |
| `_LogTime` | 以共用靜態狀態記錄各步驟與總耗時。類別位於 `Base` 命名空間。 | [來源](../../../Services/_LogTime.cs) |
| `_Model` | 以 Newtonsoft JSON 或公開屬性反射轉換模型，並讀取模型驗證錯誤。 | [來源](../../../Services/_Model.cs) |
| `_ModeUR` | 依指定鍵值查詢使用者與角色等關聯資料的 ID／文字選項。 | [來源](../../../Services/_ModeUR.cs) |
| `_Net` | 連接或中斷需要帳密的 Windows 網路共用路徑。類別位於全域命名空間。 | [來源](../../../Services/_Net.cs) |
| `_Num` | 轉換數值表示、解析數字、處理上傳列號及產生亂數。 | [來源](../../../Services/_Num.cs) |
| `_Object` | 判斷物件空值，並依欄位名讀寫值或取得型別名稱。 | [來源](../../../Services/_Object.cs) |
| `_Page` | 從列表查詢輸入取得分頁條件，並組裝分頁資料。 | [來源](../../../Services/_Page.cs) |
| `_Pwd` | 檢查密碼是否符合程式庫的強度條件。 | [來源](../../../Services/_Pwd.cs) |
| `_Result` | 判斷操作結果是否帶有錯誤。 | [來源](../../../Services/_Result.cs) |
| `_Seo` | 產生網站地圖檔案。 | [來源](../../../Services/_Seo.cs) |
| `_Sql` | 轉換 SQL 與查詢設定，組合權限條件、IN 條件及串接表達式。 | [來源](../../../Services/_Sql.cs) |
| `_Str` | 處理字串格式、分隔符號、補字元、編碼及常用文字轉換。 | [來源](../../../Services/_Str.cs) |
| `_Test` | 提供回傳固定文字的測試佔位方法，`RunSql` 僅回傳固定結果。 | [來源](../../../Services/_Test.cs) |
| `_Time` | 同步暫停目前執行緒。 | [來源](../../../Services/_Time.cs) |
| `_Valid` | 檢查電子郵件、數字、文字、JSON 或表單輸入，並整理驗證錯誤。 | [來源](../../../Services/_Valid.cs) |
| `_Var` | 依物件的字串表示判斷空值，或將 `1` 與 `true` 轉成布林真值。 | [來源](../../../Services/_Var.cs) |
| `_Xml` | 將 XML 節點序列化為 JSON，或依 XPath 讀取屬性。 | [來源](../../../Services/_Xml.cs) |
