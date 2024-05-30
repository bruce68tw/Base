其他語系：[簡中](Readme-CN.md)、[英文](Readme.md)

### 1.介紹

Base Repo 的性質是公用程式，在開發各類 c# 專案時可以參照，節省開發的時間，它裡面包含4個 project，各自使用在不同的場合: 
 - Base：最底層的公用程式，主要的功能是：各種基本資料的處理、檔案存取、CRUD服務。所有專案像是Console、API、Web MVC都必須參照Base專案。
 - BaseApi：在開發API、MVC專案時需要參照，主要的功能有：權限控制、HTTP檔案輸出、CRUD功能、報表、簽核流程、Excel匯入、交易記錄…等。
 - BaseEther：存取以太坊智能合約、區塊鏈和IPFS（分散式檔案系統）。
 - BaseWeb：在開發 MVC 專案時需要參照，內容主要是SSR（Server Side Rendering）的自訂輸入欄位。

### 2.下載 & 執行
從 GitHub 下載 Base Repo 檔案，解壓縮到本機目錄，其他專案可以正確參照 Base 相關專案即可。在系統啟動必須呼叫 _Base/Services/_Fun.Init 函數來進行初始化，以確保功能正常運作，它包含以下傳入參數：
 * isDev：是否為開發模式，系統會自動讀取目前的執行模式然後傳入。
 * diBox：ASP.NET Core使用許多DI（dependency injection依賴注入）服務，將這個服務供應者（service provider）以參數傳入，可以方便我們在公用程式裡面使用這些DI服務。
 * dbType：表示系統所使用的資料庫種類，目前的選擇有MS SQL、Oracle、MySQL。
 * authType：表示系統可以處理的權限等級，按照寬鬆程度依次分成4個等級：
    1. 沒有任何權限限制，表示所有人可以執行所有的作業。
    2. Controller等級：只控制能不能執行某個完整的作業，當使用者進入該作業之後，他可以執行裡面所有的按鈕或是子功能。
    3. Action等級：例如系統可以針對某個作業，設定使用者能否執行其中的新增、查詢、修改、刪除等CRUD子功能的權限。
    4. 資料等級：使用者在執行作業內的每個子功能時，只能存取權限範圍內的資料，例如當使用者在某個作業的修改權限被設定為＂個人＂，則無法修改別人擁有的記錄。

### 3.作者

 - Bruce Chen - *Initial work*

### 4.版權說明

本專案使用 [MIT 授權許可](https://zh.wikipedia.org/zh-tw/MIT許可證)。