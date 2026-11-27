# 小 Base 資料模型

資料更新日期：2026-11-27

| 類別名稱 | 用途說明 |
| --- | --- |
| [BaseResDto](../Models/BaseResDto.cs) | Base 共用畫面的語系文字資源；以屬性供反射複製，文字預設為空字串。 |
| [BaseUserDto](../Models/BaseUserDto.cs) | 共用服務使用的目前使用者、部門與授權資料；由 IBaseUserSvc 提供。 |
| [CalendarEventDto](../Models/CalendarEventDto.cs) | 行事曆事件資料；start 與 end 由日期時間屬性即時計算。 |
| [ChangeValueDto](../Models/ChangeValueDto.cs) | 編輯資料的異動前後值。 |
| [ChartDto](../Models/ChartDto.cs) | 一般統計圖資料，屬性命名配合 Chart.js。 |
| [ChartGroupDto](../Models/ChartGroupDto.cs) | 分組統計圖資料，屬性命名配合 Chart.js。 |
| [ChartGroupItemDto](../Models/ChartGroupItemDto.cs) | 分組圖表中單一系列的標籤、顏色與整數資料。 |
| [ChartItemDto](../Models/ChartItemDto.cs) | 一般圖表的一組整數資料點及其顏色。 |
| [ConfigDto](../Models/ConfigDto.cs) | 應用載入後指派給 _Fun.Config 的共用設定；此模型不會自行讀取組態檔。 |
| [CountDto](../Models/CountDto.cs) | 以資料分類及列分類表示的整數彙總資料。 |
| [CrudTbarDto](../Models/CrudTbarDto.cs) | CRUD 工具列按鈕使用的前端呼叫字串。 |
| [DownFileDto](../Models/DownFileDto.cs) | 自訂檔案下載的結果，包含錯誤或檔案串流。 |
| [DtDto](../Models/DtDto.cs) | DataTables 列表查詢輸入；屬性名稱保留前端格式。 |
| [DtOrderDto](../Models/DtDto.cs) | DataTables 的單項排序設定。 宣告於 DtDto.cs。 |
| [DtSearchDto](../Models/DtDto.cs) | DataTables 的快速搜尋設定。 宣告於 DtDto.cs。 |
| [EasyDtDto](../Models/EasyDtDto.cs) | 保存簡化列表查詢的分頁、排序與查詢條件。 |
| [EmailDto](../Models/EmailDto.cs) | 寄送郵件的內容、收件者與附加檔案。 |
| [ErrorRowDto](../Models/ErrorRowDto.cs) | 將驗證錯誤定位至編輯區、資料列與欄位。 |
| [GroupIdStrsDto](../Models/GroupIdStrsDto.cs) | 具有群組標題的選項集合。 |
| [IdNumDto](../Models/IdNumDto.cs) | 字串識別碼搭配整數值的資料項目。 |
| [IdStrDto](../Models/IdStrDto.cs) | 識別碼與顯示文字的配對，常用於選項清單。 |
| [IdStrExt2Dto](../Models/IdStrExt2Dto.cs) | 附帶兩個擴充字串的選項資料。 |
| [IdStrExtDto](../Models/IdStrExtDto.cs) | 附帶一個擴充字串的選項資料。 |
| [LlmAskDto](../Models/LlmAskDto.cs) | 可由 JSON 載入的 LLM 提問內容。 |
| [LoginUserDto](../Models/LoginUserDto.cs) | LDAP 登入查得的基本使用者資料。 |
| [LoginVo](../Models/LoginVo.cs) | 登入表單的輸入及錯誤提示資料。 |
| [MenuDto](../Models/MenuDto.cs) | 可包含子選單的功能表項目。 |
| [PageIn](../Models/PageIn.cs) | 以頁碼描述的簡易分頁輸入；不代表現行 CRUD 已提供對應查詢入口。 |
| [PageOut&lt;T&gt;](../Models/PageOut.cs) | 簡易分頁結果容器；資料列可為 null。 |
| [PdfImageDto](../Models/PdfImageDto.cs) | 設定 PDF 疊加圖片的位置與尺寸。 |
| [RagChunkDto](../Models/RagChunkDto.cs) | RAG 問答片段資料，短屬性名配合來源 JSON。 |
| [ResultDto](../Models/ResultDto.cs) | 後端操作結果；_Result.HasError 依錯誤訊息與欄位錯誤判斷失敗。 |
| [ResultImportDto](../Models/ResultImportDto.cs) | 檔案匯入的統計及錯誤結果。 |
| [RowColNumDto](../Models/RowColNumDto.cs) | 分組統計圖使用的列分類、欄分類與整數值。 |
| [SignLineDto](../Models/SignLineDto.cs) | 簽核流程節點之間的連線、條件與簽核對象設定。 |
| [SignRowDto](../Models/SignRowDto.cs) | 供畫面顯示的簽核紀錄。 |
| [SmsDto](../Models/SmsDto.cs) | 保存企業簡訊請求的識別值、內容編碼與已編碼訊息。 |
| [SmtpDto](../Models/SmtpDto.cs) | 寄送郵件使用的 SMTP 連線與寄件者設定。 |
| [SnNumDto](../Models/SnNumDto.cs) | 整數序號與整數值的配對。 |
| [SnStrDto](../Models/SnStrDto.cs) | 整數序號與字串值的配對。 |
| [SqlDto](../Models/SqlDto.cs) | 拆分後的 SQL 區段，供組合查詢與分頁使用。 |
| [UploadDto](../Models/UploadDto.cs) | 上傳檔案的實體儲存位置與回傳網址前綴。 |
| [ValidateCodeDto](../Models/ValidateCodeDto.cs) | 產生數字驗證碼與圖片尺寸資訊；目前未啟用圖片產生方法。 |
