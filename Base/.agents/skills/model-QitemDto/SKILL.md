---
name: model-qitem-dto
description: 在 Base 系列 MVC 查詢設定條件時，閱讀 QitemDto 及相關列舉與註解，理解它與 ReadDto.Items 的關係。不取代 CRUD 流程或模組 library-usage。
---

# Model 說明—QitemDto

本指引聚焦 `QitemDto`，每個實例描述一項查詢條件。[ReadDto](../model-ReadDto/SKILL.md) 的 `Items` 宣告為 `QitemDto[]?`，是可為 null 的陣列，不是單一 QitemDto；Items 的可省略及用途規則集中於 ReadDto 指引。

先依 [Base library-usage](../library-usage/SKILL.md) 資料存取導引，閱讀 `Models/QitemDto.cs`、`Enums/QitemOpEstr.cs`、`Enums/QitemTypeEnum.cs` 及既有註解。來源路徑相對於 `Base/Base.csproj` 所在目錄。

由類別與註解理解設定，不複製整份屬性文件；不清楚的屬性先討論確認，再補來源註解。未討論的完整屬性留待逐欄確認，不自行推增設定規則。SQL 與前端列表及虛擬 render 欄位的對應規則見 ReadDto 指引；開發流程由使用者設計，沿用 [mvc-crud-query](../../../../.agents/skills/mvc-crud-query/SKILL.md)。