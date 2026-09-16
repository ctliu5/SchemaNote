# Copilot Instructions

## 專案指導方針
- 在 SchemaNote 專案中，CSS 變更只能修改 wwwroot/css/site.css，不可直接手改 wwwroot/css/site.min.css；site.min.css 會由 bundleconfig.json 於建置時自動根據 site.css 產生。同理 JS 只改 site.js，site.min.js 自動產生。