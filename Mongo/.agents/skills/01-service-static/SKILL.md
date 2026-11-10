---
name: 01-service-static
description: 協助引用 Mongo/Mongo.csproj 的 C# 應用選用 Mongo 的 MongoDB 與 BSON靜態服務並定位原始碼。
---

# Mongo 靜態服務索引

依需求選擇類別，實作前核對應用所用版本的原始碼與呼叫條件。

| 類別 | 用途 | 原始碼 |
| --- | --- | --- |
| `_Bson` | 將 Newtonsoft JSON 物件或陣列轉成 BSON 文件，或將 BSON 文件反序列化為模型。 | [來源](../../../Services/_Bson.cs) |
| `_MgoDb` | 封裝 MongoDB 連線管理、文件查詢與更新，並將查詢項目或 JSON 條件轉成 MongoDB 篩選器。 | [來源](../../../Services/_MgoDb.cs) |
