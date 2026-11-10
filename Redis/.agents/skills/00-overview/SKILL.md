---
name: redis-overview
description: 在 Base 系列應用評估或整合 Redis，或修改其共用實作時，依任務查閱使用 StackExchange.Redis 的一般快取的專案入口。
---

# Redis 總覽

Redis 是提供使用 StackExchange.Redis 的一般快取的 .NET 類別庫。從 [Redis.csproj](../../../Redis.csproj) 核對目標框架、套件與專案參照。

- [CacheRedisSvc](../../../Services/CacheRedisSvc.cs) 實作 Base 的 [ICacheSvc](../../../../Base/Interfaces/ICacheSvc.cs)，建構時接收 `IConnectionMultiplexer`。應用需核對連線註冊、使用者與鍵的組合方式，以及連線失敗時的回傳值。
- `ResetDbA` 涉及目前資料庫索引的清除。呼叫前核對環境與清除範圍。
- 核對本模組的靜態服務盤點時，可查閱 [靜態服務索引](../01-service-static/SKILL.md)。

依任務選讀本總覽。共用開發慣例見 [shared-coding-rules](../../../../.agents/skills/shared-coding-rules/SKILL.md)。
