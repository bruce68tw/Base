---
name: 5-contracts
description: 在 BaseAI 查找模型、語音轉錄或向量資料庫的抽象基底時，依檔案用途選擇並定位 C# 原始碼。
---

# BaseAI 服務契約索引

依用途選擇檔案，開啟原始碼核對實際宣告與使用方式。

| 檔案 | 用途 |
| --- | --- |
| [AbEmbedDbSvc.cs](../../../Interfaces/AbEmbedDbSvc.cs) | 用 `CreateA` 和 `UpdateA` 寫入向量，`GetA` 讀取，`DeleteA` 或 `DeleteByFileIdA` 刪除，`VectorToIdsA` 查詢相似文件 ID。 |
| [AbLiveLlmSvc.cs](../../../Interfaces/AbLiveLlmSvc.cs) | 定義即時模型工作階段的連線、音訊與文字收送、工具回應及事件接收，並提供 WebSocket 橋接流程。 |
| [AbLlmSvc.cs](../../../Interfaces/AbLlmSvc.cs) | 提供文字問答、音訊辨識與文字嵌入的抽象基底，並管理供衍生實作使用的 HTTP 用戶端。 |
| [AbSttSvc.cs](../../../Interfaces/AbSttSvc.cs) | 定義獨立語音轉文字服務的抽象契約。 |
