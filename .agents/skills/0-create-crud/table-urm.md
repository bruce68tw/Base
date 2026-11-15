# URM 子表

以 `DbAdm/XpUser` 為例，用核取方塊維護使用者的角色關聯。主表為 `XpUser`，關聯表為 `XpUserRole`。

## 編輯 View

- Controller 載入可選項目。範例用 `_Code.RolesA()` 設定 `ViewBag.Roles`。
- 在 `Views/XpUser/Edit.cshtml` 的 `divRoles` 內，每個角色渲染一個 `xi-check`。`Fid` 和 `Value` 都用角色 ID，`Label` 用角色名稱。

## TypeScript

- 在 `_src/tsView/XpUser.ts` 建立 `mUserRole = new EditMany('Id', 'divRoles')`，再呼叫 `mUserRole.initUrm(['Id', 'RoleId'])`。
- `initUrm` 的第一個欄位是關聯表主鍵，第二個是選項鍵欄位。範例中 `Id` 識別既有關聯，`RoleId` 對應核取方塊。
- 用 `new CrudR(config, [null, _vo.mUserRole])` 接線。子表順序須與後端 `Childs` 一致。
- 載入時勾選既有關聯。儲存時新增新勾選的關聯，刪除取消勾選的既有關聯。

## Edit service

- `Controllers/XpUserController.cs` 的 `EditSvc()` 回傳 `new XpUserEdit(Ctrl)`。
- 在 `Services/XpUserEdit.cs` 的 `GetDto()` 中，將角色關聯表的 `EditDto` 放入主表的 `Childs`。
- 關聯表設定為 `Table = "dbo.XpUserRole"`、`PkeyFid = "Id"`、`FkeyFid = "UserId"`。
- `Items` 包含 `Id`、`UserId`、`RoleId`，其中 `RoleId` 為必填。共用 CRUD 依主表鍵值處理 `UserId`。
