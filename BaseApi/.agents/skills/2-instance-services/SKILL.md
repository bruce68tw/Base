---
name: 02-instance-services
description: 在 BaseApi 查找後台管理 CRUD、異動紀錄或一次性驗證碼服務時，依檔案用途選擇並定位 C# 原始碼。
---

# BaseApi 實例服務索引

依用途選擇檔案，開啟原始碼核對實際宣告與使用方式。

| 檔案 | 用途 |
| --- | --- |
| [XgEasyRptEdit.cs](../../../Services/XgEasyRptEdit.cs) | 定義簡易報表資料表 XpEasyRpt 的 CRUD 維護設定。 |
| [XgEasyRptRead.cs](../../../Services/XgEasyRptRead.cs) | 查詢 XpEasyRpt 的簡易報表設定並回傳分頁結果。 |
| [XgFlowEdit.cs](../../../Services/XgFlowEdit.cs) | 維護簽核流程定義、節點及連線，並在儲存時處理連線對節點的外鍵。 |
| [XgFlowRead.cs](../../../Services/XgFlowRead.cs) | 查詢 XpFlow 的簽核流程定義並回傳分頁結果。 |
| [XgImportRead.cs](../../../Services/XgImportRead.cs) | 依建構時指定的匯入類型查詢 XpImportLog，並回傳分頁結果。 |
| [XgOtpSvc.cs](../../../Services/XgOtpSvc.cs) | 處理電子郵件與簡訊的一次性驗證碼、驗證器註冊 QR Code 及驗證器代碼檢查。 |
| [XgProgEdit.cs](../../../Services/XgProgEdit.cs) | 定義系統作業及其角色權限關聯的 CRUD 維護設定。 |
| [XgProgRead.cs](../../../Services/XgProgRead.cs) | 查詢 XpProg 的系統作業定義並回傳分頁結果。 |
| [XgRoleEdit.cs](../../../Services/XgRoleEdit.cs) | 定義角色、角色成員與作業權限關聯的 CRUD 維護設定。 |
| [XgRoleRead.cs](../../../Services/XgRoleRead.cs) | 查詢 XpRole 的角色資料並回傳分頁結果。 |
| [XgTranLogRead.cs](../../../Services/XgTranLogRead.cs) | 依查詢條件讀取 XpTranLog 的資料異動紀錄並回傳分頁結果。 |
| [XgUserEdit.cs](../../../Services/XgUserEdit.cs) | 定義使用者及其角色關聯的 CRUD 維護設定。 |
| [XgUserRead.cs](../../../Services/XgUserRead.cs) | 透過共用 CRUD 服務查詢使用者列表，支援帳號與姓名條件。 |
| [XgUserSvc.cs](../../../Services/XgUserSvc.cs) | 依帳號與姓名的前綴查詢 XpUser，回傳符合的使用者資料。 |
