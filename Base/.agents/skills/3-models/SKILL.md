---
name: 3-models
description: 在 Base 查找CRUD、分頁、圖表、簽核或共用傳輸模型時，依檔案用途選擇並定位 C# 原始碼。
---

# Base 資料模型索引

依用途選擇檔案，開啟原始碼核對實際宣告與使用方式。

| 檔案 | 用途 |
| --- | --- |
| [BaseResDto.cs](../../../Models/BaseResDto.cs) | Base 共用畫面的語系文字資源；以屬性供反射複製，文字預設為空字串。 |
| [BaseUserDto.cs](../../../Models/BaseUserDto.cs) | 共用服務使用的目前使用者、部門與授權資料；由 IBaseUserSvc 提供。 |
| [CalendarEventDto.cs](../../../Models/CalendarEventDto.cs) | 行事曆事件資料；start 與 end 由日期時間屬性即時計算。 |
| [ChangeValueDto.cs](../../../Models/ChangeValueDto.cs) | 編輯資料的異動前後值。 |
| [ChartDto.cs](../../../Models/ChartDto.cs) | 一般統計圖資料，屬性命名配合 Chart.js。 |
| [ChartGroupDto.cs](../../../Models/ChartGroupDto.cs) | 分組統計圖資料，屬性命名配合 Chart.js。 |
| [ChartGroupItemDto.cs](../../../Models/ChartGroupItemDto.cs) | 分組圖表中單一系列的標籤、顏色與整數資料。 |
| [ChartItemDto.cs](../../../Models/ChartItemDto.cs) | 一般圖表的一組整數資料點及其顏色。 |
| [ConfigDto.cs](../../../Models/ConfigDto.cs) | 應用載入後指派給 _Fun.Config 的共用設定；此模型不會自行讀取組態檔。 |
| [CountDto.cs](../../../Models/CountDto.cs) | 以資料分類及列分類表示的整數彙總資料。 |
| [CrudTbarDto.cs](../../../Models/CrudTbarDto.cs) | CRUD 工具列按鈕使用的前端呼叫字串。 |
| [DownFileDto.cs](../../../Models/DownFileDto.cs) | 自訂檔案下載的結果，包含錯誤或檔案串流。 |
| [DtDto.cs](../../../Models/DtDto.cs) | 定義 DataTables 查詢輸入，並提供排序項目 DtOrderDto 與搜尋資料 DtSearchDto。 |
| [EasyDtDto.cs](../../../Models/EasyDtDto.cs) | 簡化列表查詢輸入，供 CrudReadSvc 內部條件處理使用。 |
| [EditDto.cs](../../../Models/EditDto.cs) | CRUD 主表或子表的讀寫設定，以及儲存流程的回呼與暫存狀態。 |
| [EitemDto.cs](../../../Models/EitemDto.cs) | 單一 CRUD 編輯欄位的讀寫及驗證設定。 |
| [EmailDto.cs](../../../Models/EmailDto.cs) | 寄送郵件的內容、收件者與附加檔案。 |
| [ErrorRowDto.cs](../../../Models/ErrorRowDto.cs) | 將驗證錯誤定位至編輯區、資料列與欄位。 |
| [GroupIdStrsDto.cs](../../../Models/GroupIdStrsDto.cs) | 具有群組標題的選項集合。 |
| [IdNumDto.cs](../../../Models/IdNumDto.cs) | 字串識別碼搭配整數值的資料項目。 |
| [IdStrDto.cs](../../../Models/IdStrDto.cs) | 識別碼與顯示文字的配對，常用於選項清單。 |
| [IdStrExt2Dto.cs](../../../Models/IdStrExt2Dto.cs) | 附帶兩個擴充字串的選項資料。 |
| [IdStrExtDto.cs](../../../Models/IdStrExtDto.cs) | 附帶一個擴充字串的選項資料。 |
| [LlmAskDto.cs](../../../Models/LlmAskDto.cs) | 可由 JSON 載入的 LLM 提問內容。 |
| [LoginUserDto.cs](../../../Models/LoginUserDto.cs) | LDAP 登入查得的基本使用者資料。 |
| [LoginVo.cs](../../../Models/LoginVo.cs) | 登入表單的輸入及錯誤提示資料。 |
| [MenuDto.cs](../../../Models/MenuDto.cs) | 可包含子選單的功能表項目。 |
| [PageIn.cs](../../../Models/PageIn.cs) | 以頁碼與每頁筆數描述簡易分頁輸入。 |
| [PageOut.cs](../../../Models/PageOut.cs) | 簡易分頁結果容器；資料列可為 null。 |
| [PdfImageDto.cs](../../../Models/PdfImageDto.cs) | 設定 PDF 疊加圖片的位置與尺寸。 |
| [QitemDto.cs](../../../Models/QitemDto.cs) | 單一查詢輸入與資料欄位的對應及條件設定。 |
| [RagChunkDto.cs](../../../Models/RagChunkDto.cs) | RAG 問答片段資料，短屬性名配合來源 JSON。 |
| [ReadDto.cs](../../../Models/ReadDto.cs) | CRUD 列表與匯出使用的 SQL 及查詢條件設定。 |
| [ResultDto.cs](../../../Models/ResultDto.cs) | 後端操作結果；_Result.HasError 依錯誤訊息與欄位錯誤判斷失敗。 |
| [ResultImportDto.cs](../../../Models/ResultImportDto.cs) | 檔案匯入的統計及錯誤結果。 |
| [RowColNumDto.cs](../../../Models/RowColNumDto.cs) | 分組統計圖使用的列分類、欄分類與整數值。 |
| [SignLineDto.cs](../../../Models/SignLineDto.cs) | 簽核流程節點之間的連線、條件與簽核對象設定。 |
| [SignRowDto.cs](../../../Models/SignRowDto.cs) | 供畫面顯示的簽核紀錄。 |
| [SmtpDto.cs](../../../Models/SmtpDto.cs) | 寄送郵件使用的 SMTP 連線與寄件者設定。 |
| [SnNumDto.cs](../../../Models/SnNumDto.cs) | 整數序號與整數值的配對。 |
| [SnStrDto.cs](../../../Models/SnStrDto.cs) | 整數序號與字串值的配對。 |
| [SqlDto.cs](../../../Models/SqlDto.cs) | 拆分後的 SQL 區段，供組合查詢與分頁使用。 |
| [UploadDto.cs](../../../Models/UploadDto.cs) | 上傳檔案的實體儲存位置與回傳網址前綴。 |
| [ValidateCodeDto.cs](../../../Models/ValidateCodeDto.cs) | 保存數字驗證碼及產生驗證碼時使用的圖片尺寸資訊。 |
