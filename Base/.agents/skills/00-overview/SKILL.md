---
name: base-overview
description: 協助 C# 應用專案使用此 Base 儲存庫中 Base/Base.csproj 共用程式庫，處理初始化、DI、資料存取、CRUD 與既有工具 API 整合。適用於已引用或準備引用此特定程式庫的應用；不涵蓋其他同名 Base 套件或 BaseApi、BaseWeb 的專屬功能。
---

# Base 程式庫應用端整合

目標是讓應用端正確使用 `Base` 類別庫，不重寫程式庫，也不將整個 Base 儲存庫當成單一應用。溝通與新增文件使用繁體中文，程式識別名稱維持原樣。

本 skill 原始檔由小 Base 子專案維護，位置為儲存庫根目錄下的 `Base/.agents/skills/00-overview/`。文件連結相對於各文件位置；以反引號標示的來源路徑則相對於大 Base 儲存庫根目錄，不相對於 skill 目錄。從大 Base 工作時按需讀取本檔，不假設會自動遞迴探索子目錄 skills；跨應用的發現與使用留待未來插件封裝處理。

## 先確認引用與版本

- 查看應用的 `.csproj`、套件／組件參照、既有啟動程式及呼叫位置，確認是本庫的 `Base.Services`、`Base.Models` 等 API。已核對來源的目標為 `net8.0`，沒有確認 NuGet 發佈名稱、套件版本或其他目標框架相容性，勿自行指定套件。
- 若有原始碼，找出真正的 `Base/Base.csproj`，依其編譯排除項目判讀 API。略過儲存庫根目錄 `zz_` 前綴目錄，不將舊版或排除的檔案當成現行功能。
- 若應用端只有 DLL／套件，使用下列自足參考文件起步，並以應用實際引用的組件中繼資料、可用文件或編譯結果確認簽章。參考文件中的來源位置皆相對於 Base 儲存庫根目錄，是查證線索；不假設應用中存在這些路徑。若版本不同或證據不足，指出缺口後取得相符文件／來源，勿猜測額外 API。
- 程式的 `ProjectReference`／套件參照不等於 Codex 已載入本 skill；應用端使用本指引時仍須讓其環境取得 skill。此次檔案是團隊共用原始檔，沒有安裝、發佈或插件設定。

## 依工作載入知識

- 新接入、啟動錯誤、DI 缺漏、使用者或快取問題：讀取 [初始化與相依](references/integration.md)。
- SQL 查詢、交易、CRUD 列表或主從表編輯：讀取 [資料存取與 CRUD](references/data-access.md)。
- 既有工具 API 選擇、JSON／結果判讀及查證入口：讀取 [工具與驗證](references/api-and-validation.md)。
- 依開發需求選用小 Base 的靜態服務並查找原始碼，可查閱 [靜態服務索引](../01-service-static/SKILL.md)。

- 撰寫列表查詢設定：按需讀 [ReadDto](../model-ReadDto/SKILL.md) 與 [QitemDto](../model-QitemDto/SKILL.md)。
- 撰寫維護設定：按需讀 [EditDto](../model-EditDto/SKILL.md) 與 [EitemDto](../model-EitemDto/SKILL.md)。

## 整合決策

保留應用既有的設定載入、登入與 DI 設計；先補齊實際會用到的服務。`_Fun` 是全域靜態狀態，不應為每個請求或租戶重設。只呼叫純工具函數時，先核對該函數是否讀取全域設定，勿無條件導入資料庫或登入服務。

CRUD 設定須來自應用實際資料表、主鍵、欄位與 JSON 契約。沒有 schema 時先完成可確認的接線並清楚列出缺漏，勿複製範例中的 `Xp*` 資料表當作應用需求。BaseApi 控制器、BaseWeb 元件、Office、Redis 實作及 AI 功能屬其他模組；需要時另外核對其參照，勿宣稱只引用 Base 即可使用。

驗證應用受影響的專案與真實呼叫情境，分開回報編譯、無外部服務檢查、資料庫整合測試。資料庫尚未備妥時，不將編譯成功描述為 CRUD 已可執行。具體檢查方法見工具與驗證參考文件。
