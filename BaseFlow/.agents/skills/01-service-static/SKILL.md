---
name: 01-service-static
description: 協助引用 BaseFlow/BaseFlow.csproj 的 C# 應用選用 BaseFlow 的簽核靜態服務並定位原始碼。
---

# BaseFlow 靜態服務索引

依需求選擇類別，實作前核對應用所用版本的原始碼與呼叫條件。

| 類別 | 用途 | 原始碼 |
| --- | --- | --- |
| `_Flow` | 讀取簽核畫面所需選項，依流程設定建立簽核資料並處理同意或退回。 | [來源](../../../Services/_Flow.cs) |
| `_FlowCode` | 提供簽核設定與畫面的選項資料，包括流程、部門、角色、節點類型與簽核狀態。 | [來源](../../../Services/_FlowCode.cs) |
