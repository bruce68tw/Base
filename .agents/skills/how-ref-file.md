# 解析參考檔案簡寫

- 有指定專案時，格式為 `專案名稱/檔案類別/檔案路徑`。以指定專案的根目錄為起點。
- 未指定專案時，格式為 `檔案類別/檔案路徑`。以目前要生成檔案的目標專案根目錄為起點。例如 `Controller/Column` 指該專案的 `Controllers/ColumnController.cs`。
- 依檔案類別套用下列規則，保留類別後方的子目錄順序。
  - `Controller` 對應 `Controllers/<名稱>Controller.cs`。例如 `DbAdm/Controller/Column` 對應 DbAdm 的 `Controllers/ColumnController.cs`。
  - `Service` 對應 `Services/<名稱>.cs`。例如 `DbAdm/Service/ColumnRead` 對應 DbAdm 的 `Services/ColumnRead.cs`。
  - `View` 對應 `Views/<子目錄>/<檔名>.cshtml`。例如 `DbAdm/View/Column/Read` 對應 DbAdm 的 `Views/Column/Read.cshtml`。
  - `ts` 對應 `_src/tsView/<名稱>.ts`。例如 `DbAdm/ts/Column` 對應 DbAdm 的 `_src/tsView/Column.ts`。
- 將解析後的相對路徑接到專案根目錄，確認檔案存在後再讀取。
- 類別未列出或找不到檔案時，先查該專案的實際目錄和檔名。
- 查找後仍無法定位唯一檔案時，請使用者提供實際路徑。
