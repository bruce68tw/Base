---
name: smstaiwan-overview
description: 在 Base 系列應用評估或整合 SmsTaiwan，或修改其共用實作時，依任務查閱串接台灣大哥大企業簡訊的專案入口。
---

# SmsTaiwan 總覽

SmsTaiwan 是提供串接台灣大哥大企業簡訊的 .NET 類別庫。從 [SmsTaiwan.csproj](../../../SmsTaiwan.csproj) 核對目標框架、套件與專案參照。

- [SmsTwSvc](../../../SmsTwSvc.cs) 實作 Base 的 [ISmsSvc](../../../../Base/Interfaces/ISmsSvc.cs)。從 `SendA` 查帳號設定、訊息編碼與長訊息拆分，以及供應商回傳的處理方式。
- 部署時以實際使用的設定核對連線與回應。發送驗證需有可用的供應商環境與測試門號。
- 核對本模組的靜態服務盤點時，可查閱 [靜態服務索引](../01-service-static/SKILL.md)。

依任務選讀本總覽。共用開發慣例見 [shared-coding-rules](../../../../.agents/skills/shared-coding-rules/SKILL.md)。
