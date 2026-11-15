---
name: 03-models
description: 在 BaseAI 查找即時對話事件、工具呼叫、向量檢索或語音指令模型時，依檔案用途選擇並定位 C# 原始碼。
---

# BaseAI 資料模型索引

依用途選擇檔案，開啟原始碼核對實際宣告與使用方式。

| 檔案 | 用途 |
| --- | --- |
| [EmbedFindResultDto.cs](../../../Models/EmbedFindResultDto.cs) | 向量檢索結果的資料模型。 |
| [EmbedRowDto.cs](../../../Models/EmbedRowDto.cs) | 向量資料庫讀回的單筆文件資料。 |
| [LiveLlmOptDto.cs](../../../Models/LiveLlmOptDto.cs) | 建立 Live LLM 工作階段時使用的連線、生成與對話設定。 |
| [LlmHistoryDto.cs](../../../Models/LlmHistoryDto.cs) | 即時對話歷史中的一則文字訊息。 |
| [LlmRespDto.cs](../../../Models/LlmRespDto.cs) | 上游即時模型事件的共用資料，依 Type 填入相應欄位。 |
| [LlmToolsDto.cs](../../../Models/LlmToolsDto.cs) | 定義即時模型的工具宣告、工具呼叫與工具回應資料。 |
| [LlmTurnResultDto.cs](../../../Models/LlmTurnResultDto.cs) | 提供應用處理或保存的即時對話回合快照。 |
| [SttOptDto.cs](../../../Models/SttOptDto.cs) | 提供單次語音轉錄所需的辨識提示。 |
| [VoiceProgArgDto.cs](../../../Models/VoiceProgArgDto.cs) | 舊版語音查詢條件模型，保留原始文字和值，供分階段對應畫面欄位。 |
| [VoiceProgArgVo.cs](../../../Models/VoiceProgArgVo.cs) | 由前端傳入的第二階段欄位語意對應請求。 |
| [VoiceProgDto.cs](../../../Models/VoiceProgDto.cs) | 第一階段語音辨識結果，包含逐字稿、動作與功能名稱。 |
