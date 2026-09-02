# SchemaNote

簡介

SchemaNote 是針對 Microsoft SQL Server 所開發的輕量 Web 工具（ASP.NET Core, Controllers + Razor Views），用來檢視與編輯資料表/欄位的說明（Extended Properties），並能匯出更新的 T‑SQL 腳本。

主要功能

- 列出資料庫的 Table、Column、Index 與建立/修改時間
- 顯示與編輯 Table 與 Column 的擴充屬性（例如 MS_Description / REMARK）
- 匯出整個資料庫或單一表格的擴充屬性更新腳本（T‑SQL）
- 使用 /sql 目錄下的 .sql 檔案作為查詢與產生腳本的樣板（以 EmbeddedResource 嵌入）

注意：本專案僅操作 extended properties，不會變更資料表結構或欄位型別。

需求

- .NET 10
- Microsoft SQL Server 2008 或更新版本（建議）

快速啟動（Visual Studio）

1. 以 Visual Studio 開啟 SchemaNote.sln
2. 設定 SchemaNote 為啟動專案，執行（F5）
3. 打開首頁，於輸入欄填入資料庫連線字串，例如：

   Server=.;Database=YourDatabase;Trusted_Connection=True;

4. 點選 Overview 取得資料庫物件清單；點選某一 Table 的 Details 可檢視/編輯欄位說明，編輯後按儲存以寫入 extended properties。

快速啟動（命令列）

1. 在專案根目錄執行：

   dotnet run --project SchemaNote

2. 開啟瀏覽器並前往顯示的 URL（例如 https://localhost:5001）

設定與實作重點

- 連線字串由使用者於首頁輸入，會儲存在伺服器記憶體的 Session（SchemaNote 的 SessionWrapper）中。
- 資料存取層提供 ADO.NET 與 Dapper 兩種實作（SchemaNote/Models/DB_Tools）。
- SQL 查詢與產生腳本置於 SchemaNote/sql/*.sql，程式以嵌入資源讀取。

檔案重點

- Controllers/HomeController.cs — 使用者流程與路由
- Models/DB_Access.cs — 讀取 metadata、產生與儲存 extended properties 的核心邏輯
- Models/DB_Tools/* — ADO.NET 與 Dapper 的具體實作
- sql/*.sql — 查詢與產生腳本模板（已作為 EmbeddedResource）
- Views/* — 使用者介面 (Razor Views)

安全性與限制

- 目前連線字串在傳輸過程未另外加密，請僅在受保護的網路環境中使用（開發 / 內網）。
- 執行修改 extended properties 的帳號需具有相應權限。