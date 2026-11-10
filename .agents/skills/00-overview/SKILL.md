---
name: base-repository-overview
description: 在大 Base 儲存庫定位模組、核對開發入口或為應用選用共用專案時，依十二個共用專案的用途與限制導向子專案文件。僅提供總覽導航，不維護詳細 API、初始化、版本或範例。
---

# 共用專案總覽

本 skill 維護第三主題「共用函數、模組、套件」的專案導航。依需求選用，不全數引用；詳細 API、初始化、版本、套件與範例由各共用專案自己的 `.agents/skills/` 維護，不在此複製。

大 Base 與各現用子專案的總覽入口皆為各自的 `.agents/skills/00-overview/SKILL.md`。根目錄的 `base-repository-overview` 提供模組選用與開發導航，依任務選讀。

從儲存庫的 [AGENTS.md](../../../AGENTS.md) 查工作約定、專案邊界與驗證方式，再按需求查閱目標子專案的工作指引、總覽與 `.csproj`。程式參照不代表應用已載入該 skill。

MVC 後台已確認使用 Base、BaseApi、BaseWeb；專案種類與環境見 [第一主題](../project-types-and-environments/SKILL.md)，不將三者強制推廣至純 API 或 Console。實作時沿用 [共用開發規則](../shared-coding-rules/SKILL.md) 的重用原則。

名稱以 `zz_` 開頭的目錄或來源目前不使用，全部略過，不修改或刪除。以下狀態依目前工作區來源整理，未完成整合驗證，不能視為全部可直接使用或已發佈。

## Base

- **專案名稱**：Base
- **用途與選用時機**：通用工具、共用模型、初始化、資料庫存取與 CRUD；需要底層共用處理或關聯式資料庫 CRUD 時選用。
- **使用 skill 入口**：[base-overview](../../../Base/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：為共用類別庫；應用端整合仍依模組契約與實際來源確認。

## BaseApi

- **專案名稱**：BaseApi
- **用途與選用時機**：Web／API 的 Controller 基底、登入權限、HTTP 與應用服務；需要這些後端共用能力時選用。
- **使用 skill 入口**：[baseapi-overview](../../../BaseApi/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：參照 Base；繼承 Controller 基底不代表已完成登入或授權整合。

## BaseWeb

- **專案名稱**：BaseWeb
- **用途與選用時機**：MVC 表單輸入與操作 ViewComponents、頁面工具；需要沿用共用表單與操作元件時選用。
- **使用 skill 入口**：[baseweb-overview](../../../BaseWeb/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：為類別庫，參照 BaseApi 與 Base；前端腳本及樣式接線仍需在應用確認。

## BaseAI

- **專案名稱**：BaseAI
- **用途與選用時機**：LLM、語音、嵌入向量與上下文處理；需要文字問答、語音或向量整合時選用。
- **使用 skill 入口**：[baseai-overview](../../../BaseAI/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：參照 BaseApi 與 Base；各 provider 能力依實際契約確認。

## BaseFlow

- **專案名稱**：BaseFlow
- **用途與選用時機**：流程設定、送簽、簽核處理及相關畫面；需要整合既有 Xp 簽核流程時選用。
- **使用 skill 入口**：[baseflow-overview](../../../BaseFlow/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：為參照 BaseWeb 的 Razor 類別庫；需核對應用資料與流程整合。

## BaseOffice

- **專案名稱**：BaseOffice
- **用途與選用時機**：Excel 匯入與資料填入、Word 套表及合併；需要 Office 資料交換或範本文件產製時選用。
- **使用 skill 入口**：[baseoffice-overview](../../../BaseOffice/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：來源使用 Open XML，套件與相依以目前 checkout 的 `.csproj` 為準。

## BaseEther

- **專案名稱**：BaseEther
- **用途與選用時機**：Ethereum 合約存取與 IPFS 圖片／文字上傳；需要鏈上合約或 IPFS 整合時選用。
- **使用 skill 入口**：[baseether-overview](../../../BaseEther/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：使用 Nethereum 與 IPFS 套件；外部服務整合尚未驗證。

## Mongo

- **專案名稱**：Mongo
- **用途與選用時機**：MongoDB／BSON 存取、查詢與 CRUD；資料儲存在 MongoDB 且需要共用 CRUD 模式時選用。
- **使用 skill 入口**：[mongo-overview](../../../Mongo/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：使用 MongoDB Driver；實際資料庫整合尚未驗證。

## PdfSpire

- **專案名稱**：PdfSpire
- **用途與選用時機**：Word 轉 PDF 與 PDF 加圖片；需要文件轉換或圖片蓋印時選用。
- **使用 skill 入口**：[pdfspire-overview](../../../PdfSpire/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：以 Spire 實作 Base 的 PDF 服務介面；部署與實際文件處理尚未驗證。

## Redis

- **專案名稱**：Redis
- **用途與選用時機**：Redis 共用快取；需要以 Redis 實作共用快取介面時選用。
- **使用 skill 入口**：[redis-overview](../../../Redis/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：以 StackExchange.Redis 實作 `ICacheSvc`，用途為一般快取；實際 Redis 整合尚未驗證。

## RedisStack

- **專案名稱**：RedisStack
- **用途與選用時機**：BaseAI 向量資料庫的實作方向；目前供評估參考，需要此方向時先補齊實作與驗證。
- **使用 skill 入口**：[redisstack-overview](../../../RedisStack/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：目前為未完成骨架，仍依賴 BaseAI 已排除編譯的舊介面，有空方法、`NotImplementedException` 與空字串回傳；不能當作可直接使用的向量資料庫，也不等同 Redis 快取模組。

## SmsTaiwan

- **專案名稱**：SmsTaiwan
- **用途與選用時機**：串接台灣大哥大企業簡訊；需要透過此供應商發送簡訊時選用。
- **使用 skill 入口**：[smstaiwan-overview](../../../SmsTaiwan/.agents/skills/00-overview/SKILL.md)
- **使用狀態與限制**：實作 Base 的簡訊服務介面；供應商連線與實際發送尚未驗證。
