---
name: 02-instance-services
description: 在 BaseAI 查找語音回合、模型供應商或向量檢索服務時，依檔案用途選擇並定位 C# 原始碼。
---

# BaseAI 實例服務索引

依用途選擇檔案，開啟原始碼核對實際宣告與使用方式。

| 檔案 | 用途 |
| --- | --- |
| [AudioTurnBufferSvc.cs](../../../Services/AudioTurnBufferSvc.cs) | 暫存單一語音回合的輸入音訊及逐字稿，管理中斷與回合切換狀態。 |
| [CengSvc.cs](../../../Services/CengSvc.cs) | 定義上下文處理管線與資料結構，組合文件檢索、重排序、記憶、壓縮及 Token 預算。 |
| [CengSvcTest.cs](../../../Services/CengSvcTest.cs) | 以 RunA 示範上下文處理管線的組裝與執行。 |
| [ChatGptSvc.cs](../../../Services/ChatGptSvc.cs) | 透過 OpenAI HTTP API 處理文字請求與 WAV 轉錄。 |
| [ChromaSvc.cs](../../../Services/ChromaSvc.cs) | 透過 Chroma HTTP API 讀寫向量集合。 |
| [GeminiLiveSvc.cs](../../../Services/GeminiLiveSvc.cs) | 透過 WebSocket 對接 Gemini Live，轉換共用即時對話模型與供應商訊息。 |
| [GeminiSvc.cs](../../../Services/GeminiSvc.cs) | 透過 Gemini HTTP API 處理文字問答、音訊轉錄與文字嵌入。 |
| [QwenLiveSvc.cs](../../../Services/QwenLiveSvc.cs) | 透過 WebSocket 對接 Qwen Omni Realtime，轉換共用即時對話模型與供應商事件。 |
