---
name: mongo-overview
description: 在 Base 系列應用評估或整合 Mongo，或修改其共用實作時，依任務查閱MongoDB 文件存取、BSON 與 CRUD的專案入口。
---

# Mongo 總覽

Mongo 是提供MongoDB 文件存取、BSON 與 CRUD的 .NET 類別庫。從 [Mongo.csproj](../../../Mongo.csproj) 核對目標框架、套件與專案參照。

- 從 [MgoDb](../../../Services/MgoDb.cs) 與 [_MgoDb](../../../Services/_MgoDb.cs) 核對連線、集合與查詢條件，從 [_Bson](../../../Services/_Bson.cs) 查 BSON 處理。
- 分頁先查 [MgoReadSvc](../../../Services/MgoReadSvc.cs) 與 [MgoReadDto](../../../Models/MgoReadDto.cs)。維護先查 [MgoEditSvc](../../../Services/MgoEditSvc.cs) 與 [MgoEditDto](../../../Models/MgoEditDto.cs)，並核對呼叫端使用的識別鍵與文件結構。
- 選用 MongoDB 文件存取或 BSON 轉換的靜態方法時，可查閱 [靜態服務索引](../01-service-static/SKILL.md)。

依任務選讀本總覽。共用開發慣例見 [shared-coding-rules](../../../../.agents/skills/shared-coding-rules/SKILL.md)。
