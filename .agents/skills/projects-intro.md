# 共用專案總覽

本文件維護第三主題「共用函數、模組、套件」的專案導航。依需求選用，不全數引用；詳細 API、初始化、版本、套件與範例由各共用專案自己的 `.agents/skills/` 維護，不在此複製。

從儲存庫的 [AGENTS.md](../../AGENTS.md) 查工作約定、專案邊界與驗證方式，再按需求查閱目標子專案的工作指引、總覽與 `.csproj`。程式參照不代表應用已載入該 skill。

MVC 後台已確認使用 Base、BaseApi、BaseWeb；查閱專案種類與環境時，請讀取 `project-types.md`，不將三者強制推廣至純 API 或 Console。實作時請讀取 `coding-rules.md` 的重用原則。

名稱以 `zz_` 開頭的目錄或來源目前不使用，全部略過，不修改或刪除。以下狀態依目前工作區來源整理，未完成整合驗證，不能視為全部可直接使用或已發佈。

## 依需求查找共用服務契約

業務程式透過介面或抽象基底呼叫服務。依下表進入契約索引，再開啟 C# 原始碼核對方法簽章。任務涉及 DI 註冊、切換供應商或排查實作時，從目標應用的註冊位置查實際使用的服務。

| 開發需求 | 查閱入口 |
| --- | --- |
| Word 轉 PDF、在 PDF 加入圖片 | Base 的 IPdfSvc，見 [Base 服務契約索引](../../Base/.agents/skills/5-contracts/SKILL.md)。 |
| 依使用者與鍵讀寫或刪除字串快取 | Base 的 ICacheSvc，見 [Base 服務契約索引](../../Base/.agents/skills/5-contracts/SKILL.md)。 |
| 發送簡訊 | Base 的 ISmsSvc，見 [Base 服務契約索引](../../Base/.agents/skills/5-contracts/SKILL.md)。 |
| 儲存向量、讀取或刪除向量資料、找相似文件 ID | BaseAI 的 AbEmbedDbSvc，見 [BaseAI 服務契約索引](../../BaseAI/.agents/skills/5-contracts/SKILL.md)。 |

## Base

- 提供全域設定與初始化、應用路徑、目前使用者資料及登入設定的存取。
- 提供資料庫 SQL 查詢、命令執行與交易管理，並將查詢結果轉為 JSON、模型或識別碼／文字選項。
- 提供字串與識別碼轉換、日期與時間計算、西元與民國日期轉換、集合操作，以及 JSON、XML、模型與公開屬性的轉換和存取。
- 提供文字檔讀寫、檔案列舉、ZIP 壓縮、路徑與上傳檔名處理，以及需帳密的 Windows 網路共用連線。
- 提供 SMTP 郵件寄送、LDAP 帳密驗證、密碼強度及欄位輸入檢查、日誌與步驟耗時記錄。
- 提供資料庫統計值轉圖表資料、簡易分頁條件與錯誤結果處理，以及 sitemap XML 產生。
- 定義目前使用者資料、Word 轉 PDF、PDF 圖片疊加與簡訊傳送的服務契約。

## BaseApi

- **專案名稱**：BaseApi
- **用途與選用時機**：Web／API 的 Controller 基底、登入權限、HTTP 與應用服務；需要這些後端共用能力時選用。
- **使用 skill 入口**：[baseapi-overview](../../BaseApi/.agents/skills/0-overview/SKILL.md)
- **使用狀態與限制**：參照 Base；繼承 Controller 基底不代表已完成登入或授權整合。

## BaseWeb

- **專案名稱**：BaseWeb
- **用途與選用時機**：MVC 表單輸入與操作 ViewComponents、頁面工具；需要沿用共用表單與操作元件時選用。
- **使用 skill 入口**：[baseweb-overview](../../BaseWeb/.agents/skills/0-overview/SKILL.md)
- **使用狀態與限制**：為類別庫，參照 BaseApi 與 Base；前端腳本及樣式接線仍需在應用確認。

## BaseAI

- **專案名稱**：BaseAI
- **用途與選用時機**：LLM、語音、嵌入向量與上下文處理；需要文字問答、語音或向量整合時選用。
- **使用 skill 入口**：[baseai-overview](../../BaseAI/.agents/skills/0-overview/SKILL.md)
- **使用狀態與限制**：參照 BaseApi 與 Base；各 provider 能力依實際契約確認。

## BaseFlow

- **專案名稱**：BaseFlow
- **用途與選用時機**：流程設定、送簽、簽核處理及相關畫面；需要整合既有 Xp 簽核流程時選用。
- **使用 skill 入口**：[baseflow-overview](../../BaseFlow/.agents/skills/0-overview/SKILL.md)
- **使用狀態與限制**：為參照 BaseWeb 的 Razor 類別庫；需核對應用資料與流程整合。

## MsOffice

- **專案名稱**：MsOffice
- **用途與選用時機**：Excel 匯入與資料填入、Word 套表及合併；需要 Office 資料交換或範本文件產製時選用。
- **使用 skill 入口**：[MsOffice-overview](../../MsOffice/.agents/skills/0-overview/SKILL.md)
- **使用狀態與限制**：來源使用 Open XML，套件與相依以目前 checkout 的 `.csproj` 為準。

## BaseEther

- **專案名稱**：BaseEther
- **用途與選用時機**：Ethereum 合約存取與 IPFS 上傳。
- **使用 skill 入口**：[baseether-overview](../../BaseEther/.agents/skills/0-overview/SKILL.md)

## Redis

- **專案名稱**：Redis
- **用途與選用時機**：以 Redis 讀寫或刪除使用者字串快取。
- **使用 skill 入口**：[redis-overview](../../Redis/.agents/skills/0-overview/SKILL.md)。現行實作見 [RedisSvc](../../Redis/RedisSvc.cs)。

## RedisStack

- **專案名稱**：RedisStack
- **用途與選用時機**：Redis Stack 嵌入向量資料庫整合。
- **使用 skill 入口**：[redisstack-overview](../../RedisStack/.agents/skills/0-overview/SKILL.md)。現行實作見 [RedisStackSvc](../../RedisStack/RedisStackSvc.cs)。
- **使用狀態與限制**：參照 BaseAI 與 Base，目前為向量存取的實作骨架。

## PdfSpire

- **專案名稱**：PdfSpire
- **用途與選用時機**：使用 Spire 將 Word 轉成 PDF，或在 PDF 加入圖片。
- **使用 skill 入口**：[pdfspire-overview](../../PdfSpire/.agents/skills/0-overview/SKILL.md)

## SmsTw

- **專案名稱**：SmsTw
- **用途與選用時機**：串接台灣大哥大企業簡訊。
- **使用 skill 入口**：[smstaiwan-overview](../../SmsTw/.agents/skills/0-overview/SKILL.md)。現行專案設定見 [SmsTw.csproj](../../SmsTw/SmsTw.csproj)。
