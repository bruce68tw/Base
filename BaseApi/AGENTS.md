# BaseApi 工作入口

- 先遵守儲存庫上層 [AGENTS.md](../AGENTS.md)，查看本庫 git 狀態並保留既有修改。
- 本子專案負責HTTP／MVC、登入權限與應用服務，沒有應用啟動入口。
- 職責、來源與公共契約入口見 [docs/architecture.md](docs/architecture.md)；整合流程沿用 [.agents/skills/00-overview/SKILL.md](.agents/skills/00-overview/SKILL.md)，按任務讀 references。
- 修改公共 API 先查呼叫端與衍生實作；應用特有提示、功能代碼與執行政策由應用維護。
- 以實際 csproj、排除項目與來源為準；文檔修改查連結，程式修改依架構文件建置，回報已驗證與未驗證範圍。不得將未提交契約說成已發佈版本。
