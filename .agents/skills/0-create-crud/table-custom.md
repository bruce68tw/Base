# 自訂子表

- **Edit View** 由程式員自訂畫面與操作。
- **TypeScript** 參考 `DbAdm/ts/GenCrud`，建立子表的 `EditMany`，並綁定以下三個函數。實際邏輯留給程式員填寫。
- **Edit service** 沿用其他子表的設定。在主表的 `EditDto.Childs` 加入子表的 `EditDto`，設定 `Table`、`PkeyFid`、`FkeyFid` 和 `Items`。

```ts
editMany.fnLoadRows = (rows: Json[]): void => {
    //todo
};

editMany.fnGetUpdJson = (upKey: StrNum): Json => {
    //todo
    return {};
};

editMany.fnValid = (): boolean => {
    //todo
    return true;
};
```
