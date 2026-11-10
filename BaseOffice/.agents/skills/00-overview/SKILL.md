---
name: baseoffice-overview
description: 在 Base 系列應用評估或整合 BaseOffice，或修改其共用實作時，依任務查閱Excel 匯入及 Word 範本文件處理的專案入口。
---

# BaseOffice 總覽

BaseOffice 是提供Excel 匯入及 Word 範本文件處理的 .NET 類別庫。從 [BaseOffice.csproj](../../../BaseOffice.csproj) 核對目標框架、套件與專案參照。

- 從 [ExcelImportSvc](../../../Services/ExcelImportSvc.cs)、[ExcelImportDto](../../../Models/ExcelImportDto.cs) 及 [_Excel](../../../Services/_Excel.cs) 查欄位對應、工作表與匯入入口。
- 從 [WordSetSvc](../../../Services/WordSetSvc.cs) 與 [_Word](../../../Services/_Word.cs) 查範本填值與文件處理。涉及上下載時查 [_HttpExcel](../../../Services/_HttpExcel.cs)、[_HttpWord](../../../Services/_HttpWord.cs) 及 [baseapi-overview](../../../../BaseApi/.agents/skills/00-overview/SKILL.md)。
- 原始碼採用 Open XML。還原與部署相依依目前 checkout 的 `.csproj` 核對。

依任務選讀本總覽。共用開發慣例見 [shared-coding-rules](../../../../.agents/skills/shared-coding-rules/SKILL.md)。
