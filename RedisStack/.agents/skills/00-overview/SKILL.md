---
name: redisstack-overview
description: 在 Base 系列應用評估或整合 RedisStack，或修改其共用實作時，依任務查閱BaseAI 嵌入向量資料庫的 Redis Stack 實作評估的專案入口。
---

# RedisStack 總覽

RedisStack 是提供BaseAI 嵌入向量資料庫的 Redis Stack 實作評估的 .NET 類別庫。從 [RedisStack.csproj](../../../RedisStack.csproj) 核對目標框架、套件與專案參照。

- 從 [RedisStackSvc](../../../RedisStackSvc.cs) 與 [BaseAI 向量與上下文說明](../../../../BaseAI/.agents/skills/00-overview/references/vectors-context.md) 核對介面與應用端需求。
- 實作目前仍在骨架階段，且使用 BaseAI 專案已排除編譯的舊介面 `IEmbedDbSvc`。現行向量資料庫契約見 [AbEmbedDbSvc](../../../../BaseAI/Interfaces/AbEmbedDbSvc.cs)。`GetA`、`DeleteA`、`ClearA` 會擲出 `NotImplementedException`，`VectorToTextA` 回傳空字串。整合前需補齊實作並驗證 Redis Stack 環境。
- 核對本模組的靜態服務盤點時，可查閱 [靜態服務索引](../01-service-static/SKILL.md)。

依任務選讀本總覽。共用開發慣例見 [shared-coding-rules](../../../../.agents/skills/shared-coding-rules/SKILL.md)。
