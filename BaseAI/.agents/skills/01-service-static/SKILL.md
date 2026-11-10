---
name: 01-service-static
description: 協助已引用或準備引用 BaseAI/BaseAI.csproj 的應用，依音訊處理與語音指令提示詞需求選用 BaseAI.Services 的靜態服務並定位原始碼。
---

# BaseAI 靜態服務索引

依需求選擇類別，實作前核對應用所用版本的原始碼與呼叫條件。

| 類別 | 用途 | 原始碼 |
| --- | --- | --- |
| `_Voice` | 將音訊轉為 WAV、將 16 kHz 單聲道 PCM16 封裝為 WAV，或檢查音訊格式。部分輸入格式需由 FFmpeg 轉換。 | [來源](../../../Services/_Voice.cs) |
| `_VoiceProg` | 提供兩階段語音指令提示詞。第一階段用於轉寫與辨識開啟或查詢意圖，第二階段用於將查詢條件對應至畫面欄位與選項。 | [來源](../../../Services/_VoiceProg.cs) |
