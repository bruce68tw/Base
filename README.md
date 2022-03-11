# Base

裡面包含3個 project: 
(1).Base project: c# 基本的 library, 可以用在各種 project, 像是 windows、console、web application.
(2).BaseApi project: c# 基本的 library, 用於 Web API 專案.
(3).BaseWeb project: c# 基本的 library, 用於 Web MVC 專案.

## 介紹

這個目錄的內容是收集可以重複使用的公用程式和模組，來降低系統開發的時間和成本。

### 環境需求

這裡主要開發的是 Web MVC 系統, 使用的工具為: ASP.NET Core 6、jQuery 3.3、Bootstrap 4、Visual Studio 2022 Community

### 安裝

這些是基本的 library, 的用途是讓其他專案參照。下載檔案並且解壓縮之後, 將上面3個目錄名稱後面的-master文字移除即可, 跟其他的開發專案目錄放在同一層, 方案sln檔案會讀取這些專案的相對路徑。

## 作者

* **Bruce Chen** - *Initial work*

## 版權

MIT License