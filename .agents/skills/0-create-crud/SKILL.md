---
name: create-crud
description: 生成 MVC CRUD 功能的程式檔案到用戶指定的應用專案的路徑。
---

# 產生 MVC CRUD 程式

## 規則
  1. 先讀取 `../how-ask-question.md`，依其中的流程收集下列答案，經使用者確認需求摘要後生成檔案。
  2. 遇到參考檔案簡寫時，先讀取 `../how-ref-file.md`，依其中的規則定位並讀取參考檔案。

## 問題

### 總覽1. 要產生程式的 MVC 專案名稱? 例如: OfficialAdm

- **問題處理規則**　
  1. Codex 必須存在此專案，否則顯示訊息，請用戶重填。

- **程式實作規則**　
  1. 該專案在 Codex 的第一個 "來源資料夾" 為專案路徑。

### 總覽2. 請提供功能名稱、中文名稱? 例如: CustMsg、客戶留言

- **問題處理規則**
  1. 功能名稱即為資料表名稱，必填。
  2. 功能中文名可空白，用來增加判斷後面題目的欄位中文名。
  3. 後面的問題中如果出現 xxx, 則將此功能名稱代入。

- **程式實作規則**　
  1. 以功能名稱 `xxx` 為例，固定生成 Controller（`Controllers/xxxController.cs`）、查詢 Service（`Services/xxxRead.cs`）、查詢 View（`Views/xxx/Read.cshtml`）及 TypeScript（`_src/tsView/xxx.ts`）。若功能包含編輯，再生成編輯 Service（`Services/xxxEdit.cs`）及編輯 View（`Views/xxx/Edit.cshtml`）。
  2. 最後生成檔案時，如果原檔案已經存在，則原檔名後面加上 "-x", 其中 "x" 為不重複的序號，例如: ItemController.cs-2

### 總覽3. 要產生的那一種查詢功能種類(單選)? 1.包含編輯, 2.單純查詢, 3.統計圖

- **問題處理規則**
  1. 必須正確回答 1、2、3
  
### 查詢1. 請輸入查詢欄位清單(只寫名稱, 逗號分隔)? 例如: CustName, Email, Phone, Created

- **問題處理規則**
  1. 先讀取 `../get-field-sname.md`，依其中的規則查找欄位顯示名稱。
  2. 將用戶輸入的每個欄位的轉成格式: 欄位名稱-中文名-欄位種類，例如: CustName-客戶名稱-文字，做為下個題目的內容。
  3. 欄位種類清單為: 文字、數字、下拉、日期。
  4. 如果是下拉欄位，後面要加上額外資訊，參考 `../get-source-items.md`
  5. 如果沒有查詢欄位，則省略後面的查詢2、3題

### 查詢2. 請調整以下查詢欄位的內容和先後位置，每行一個欄位。
CustName-客戶名稱-文字
Email-電子信箱-文字 
Phone-聯絡電話-文字
Created-建檔日期-日期

- **問題處理規則**
  1. AI 自動根據前面的回答內容，自動帶出欄位清單，以上清單內容僅為示意。

- **程式實作規則**　
  1. 查詢欄位對應 Service/Read ReadDto 的 Items 欄位，以及 View/Read 在 div divRead 裡面的欄位(參考 DbAdm/Column)。
  2. View/Read 的第一個查詢欄位有邊有工具列，必須放在 div row 裡面，同時設定 InRow = true。
  3. 如果查詢 2b 指定進階查詢欄位，將這些欄位移入 `<form id="formRead2">`。
  
### 查詢2b. 請輸入第一個要放入進階查詢表單的欄位名稱，例如 Phone。該欄位及其後所有欄位都會移入進階查詢表單。不使用進階查詢表單時，請輸入 X（不限大小寫）。

- **提問條件**  查詢欄位數目 >= 5 才提問。

- **問題處理規則**
  1. 輸入任何否定文字都是表示不必放入進階查詢表單。
  2. 欄位名稱須在查詢 2 確認的清單中。依該清單的順序，將指定欄位及其後所有欄位移入進階查詢表單。

- **程式實作規則**　
  1. 使用進階查詢時，在 View/Read 建立 `<form id="formRead2">`，並設定 `XgFindTbarDto.HasFind2 = true`。參考 DbAdm/Column。

### 查詢3. 是否需要匯出 Excel 功能? 

- **提問條件**  有查詢欄位時。

- **程式實作規則**　
  1. 如果是，則在 View/Read 加上 xg-export 元件，Controller 加上 Export()，參考 DbAdm/Table
  2. 如果有查詢欄位，則 View/Read XgFindTbarDto 設定 HasReset = true。

### 查詢3a. 請提供匯出 Excel 的 SQL 內容，另外需自行建立 _template\xxx.xlsx

- **提問條件**  工具列有匯出 Excel。

- **程式實作規則**　
  1. 此 SQL 內容對應 Service/Read ReadDto 的 ExportSql 欄位。

### 查詢4. 請輸入查詢結果的欄位名稱，以逗號分隔。例如 CustName, MsgNo, Email, Phone, Created

- **問題處理規則**
  1. 依照 `../get-field-sname.md`，查找欄位顯示名稱。
  2. 系統自動補上顯示名稱，以 `欄位名稱-顯示名稱` 格式帶入查詢 5，例如 `CustName-客戶名稱`。

### 查詢5. 請確認以下查詢結果欄位的先後順序，並進行調整。
CustName-客戶名稱 
MsgNo-訊息編號 
Email-電子信箱 
Phone-聯絡電話 
Created-建檔日期

- **程式實作規則**　
  1. 欄位名稱對應 Typescript 的 _me.init() columns[] 的 data 欄位。

### 編輯

讀取 `question-edit.md`，依其中的題目與提問條件收集答案。 

- **提問條件**  查詢功能包含編輯時。

### 統計圖

讀取 `question-chart.md`，依其中的題目與提問條件收集答案。

- **提問條件**  查詢功能為統計圖時。

### 結論. 可以依這份摘要產生程式嗎？

