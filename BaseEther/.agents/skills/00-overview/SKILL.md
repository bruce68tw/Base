---
name: baseether-overview
description: 在 Base 系列應用評估或整合 BaseEther，或修改其共用實作時，依任務查閱Ethereum 合約存取與 IPFS 上傳的專案入口。
---

# BaseEther 總覽

BaseEther 是提供Ethereum 合約存取與 IPFS 上傳的 .NET 類別庫。從 [BaseEther.csproj](../../../BaseEther.csproj) 核對目標框架、套件與專案參照。

- 從 [_Ether](../../../Services/_Ether.cs) 查合約讀取、簽署交易用合約與交易日誌，核對呼叫端提供的節點、合約地址與 ABI。
- 從 [_Ipfs](../../../Services/_Ipfs.cs) 查圖片與文字上傳，依部署環境確認服務端點與憑證來源。專案套件以 `.csproj` 的 Nethereum 與 IPFS 相依為準。

依任務選讀本總覽。共用開發慣例見 [shared-coding-rules](../../../../.agents/skills/shared-coding-rules/SKILL.md)。
