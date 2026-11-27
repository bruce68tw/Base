# 小 Base 服務契約

資料更新日期：2026-11-27

| 類別名稱 | 用途說明 |
| --- | --- |
| [IBaseUserSvc](../Interfaces/IBaseUserSvc.cs) | 提供 Base 共用服務所需的目前使用者資料。 |
| [IPdfSvc](../Interfaces/IPdfSvc.cs) | 定義授權設定、Word 位元組轉 PDF，以及在 PDF 加入圖片的契約。 |
| [ISmsSvc](../Interfaces/ISmsSvc.cs) | 定義依電話號碼與訊息內容非同步傳送簡訊的契約。 |

契約的實作與應用接線由使用端提供；簡訊傳送回傳成功不代表手機已收到訊息。
