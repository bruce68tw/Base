---
name: model-eitem-dto
description: 在 Base 系列 MVC 編輯功能設定欄位時，閱讀 EitemDto 及註解，理解它與 EditDto.Items 的關係。未確認的屬性留待討論，不取代 CRUD 流程或模組總覽。
---

# Model 說明—EitemDto

本指引聚焦 `EitemDto`：單一編輯欄位的對應、讀寫與驗證設定。[EditDto](../model-EditDto/SKILL.md) 的 `Items` 宣告為 `EitemDto[]`，目前以 `null!` 初始化；這是陣列，不是單一 Model，也不能套用 ReadDto.Items 可省略的規則。

先依 [base-overview](../00-overview/SKILL.md) 資料存取與 CRUD 導引，閱讀 `Models/EitemDto.cs` 及既有註解；來源路徑相對於 `Base/Base.csproj` 所在目錄。依使用到的驗證設定閱讀其相關型別，不由名稱猜測契約。

由類別與註解理解設定，不複製整份屬性文件；不清楚的屬性先討論確認，再補來源註解。完整屬性規則留待逐欄討論，不將預設值或個案選擇升為通用要求。

DbAdm 的 `Services/ColumnEdit.cs` 是設定參考，目前本機位置為 `C:/_project/DbAdm`，搬移後重新定位；欄位與其他設定屬個案。編輯畫面範圍及 ReadSql／資料權限規則見 EditDto 指引，需求確認與生成流程見 [mvc-crud-query](../../../../.agents/skills/mvc-crud-query/SKILL.md)，不據 Model 欄位擴增使用者未確認的操作。