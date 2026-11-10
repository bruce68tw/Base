---
name: pdfspire-overview
description: 在 Base 系列應用評估或整合 PdfSpire，或修改其共用實作時，依任務查閱以 Spire 進行 Word 轉 PDF 與 PDF 圖片蓋印的專案入口。
---

# PdfSpire 總覽

PdfSpire 是提供以 Spire 進行 Word 轉 PDF 與 PDF 圖片蓋印的 .NET 類別庫。從 [PdfSpire.csproj](../../../PdfSpire.csproj) 核對目標框架、套件與專案參照。

- 從 [SpireSvc](../../../SpireSvc.cs) 與 Base 的 [IPdfSvc](../../../../Base/Interfaces/IPdfSvc.cs) 查 `WordToPdf`、`AddImages` 和授權檔的接線。
- 蓋印時核對頁索引、座標與頁面旋轉，並在目標部署平台以實際文件驗證輸出。
- 核對本模組的靜態服務盤點時，可查閱 [靜態服務索引](../01-service-static/SKILL.md)。

依任務選讀本總覽。共用開發慣例見 [shared-coding-rules](../../../../.agents/skills/shared-coding-rules/SKILL.md)。
