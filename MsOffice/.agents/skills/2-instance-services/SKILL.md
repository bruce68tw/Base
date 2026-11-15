---
name: instance-services
description: 在 BaseOffice 查找Excel 匯入或 Word 範本填值服務時，依檔案用途選擇並定位 C# 原始碼。
---

# BaseOffice 實例服務索引

依用途選擇檔案，開啟原始碼核對實際宣告與使用方式。

| 檔案 | 用途 |
| --- | --- |
| [ExcelImportSvc.cs](../../../Services/ExcelImportSvc.cs) | 依匯入設定讀取 Excel 工作表，轉換欄位並驗證及處理資料列。 |
| [WordImageSvc.cs](../../../Services/WordImageSvc.cs) | 讀取 Word 嵌入圖片，保留原始位元組並將指定尺寸轉成 EMU。 |
| [WordSetSvc.cs](../../../Services/WordSetSvc.cs) | 以 Open XML 對 Word 範本填入主從資料及圖片。 |
