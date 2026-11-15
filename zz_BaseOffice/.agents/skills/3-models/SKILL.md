---
name: 03-models
description: 在 BaseOffice 查找Excel 欄位對應或 Word 範本與圖片模型時，依檔案用途選擇並定位 C# 原始碼。
---

# BaseOffice 資料模型索引

依用途選擇檔案，開啟原始碼核對實際宣告與使用方式。

| 檔案 | 用途 |
| --- | --- |
| [ExcelImportDto.cs](../../../Models/ExcelImportDto.cs) | 定義 Excel 工作表匯入時使用的欄位對應及資料處理設定。 |
| [ExcelImportFieldDto.cs](../../../Models/ExcelImportFieldDto.cs) | 描述一個 Excel 儲存格對應的匯入欄位。 |
| [WordImageDto.cs](../../../Models/WordImageDto.cs) | 描述 Word 套表時要填入的圖片及其尺寸。 |
| [WordSetTplDto.cs](../../../Models/WordSetTplDto.cs) | 保存範本文字的命中片段及其起訖位置。 |
