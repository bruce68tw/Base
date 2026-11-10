---
name: baseflow-overview
description: 在 Base 系列應用評估或整合 BaseFlow，或修改其共用實作時，依任務查閱簽核流程、流程設定及 XpFlow MVC 畫面的專案入口。
---

# BaseFlow 總覽

BaseFlow 是提供簽核流程、流程設定及 XpFlow MVC 畫面的 .NET 類別庫。從 [BaseFlow.csproj](../../../BaseFlow.csproj) 核對目標框架、套件與專案參照。

- 從 [XpFlow 畫面](../../../Areas/XpFlow) 查看 Razor 入口。流程設定的分頁與維護分別由 [XgFlowR](../../../Services/XgFlowR.cs) 和 [XgFlowE](../../../Services/XgFlowE.cs) 提供，畫面選項可查 [_Flow](../../../Services/_Flow.cs) 與 [_FlowCode](../../../Services/_FlowCode.cs)。
- 此 Razor 類別庫參照 BaseWeb。整合時核對使用端的 Area 路由、簽核資料表與權限，並依畫面需要查閱 [baseweb-overview](../../../../BaseWeb/.agents/skills/00-overview/SKILL.md)。
- 選用簽核處理或流程選項的靜態方法時，可查閱 [靜態服務索引](../01-service-static/SKILL.md)。

依任務選讀本總覽。共用開發慣例見 [shared-coding-rules](../../../../.agents/skills/shared-coding-rules/SKILL.md)。
