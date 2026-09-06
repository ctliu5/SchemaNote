# 工具介紹
SchemaNote（以下簡稱本平台）是用來檢視、編輯SQL Server上自定義註記之Web平台

# 基本畫面

![未命名](https://user-images.githubusercontent.com/29647567/127631405-e55df674-f299-4906-b072-db0a16e0a5ab.png)

### 物件層

```
  1. 物件名稱：資料表(Table)或檢視表(View)的名稱
  2. 物件說明：資料表或檢視表的中文解釋名稱；系可編輯的擴充屬性之值（對應擴充屬性的Key為：MS_Description）
  3. 物件類型：只有兩種，資料表或檢視表，也就是說其他的資料庫物件，例如預存程序、函數……先不考慮
  4. 結構描述名稱：當前物件的結構描述名稱
  5. 物件創建日期：當前物件創建日期
  6. 物件修改日期：當前物件修改日期
  7. 備註：資料表或檢視表的中文補充說明、備註；系可編輯的擴充屬性之值（對應擴充屬性的Key為：REMARK）
  8. 筆數：當前物件之資料總筆數
```

## Docker 建置與部署

以下示範如何在本機建置、匯出並執行映像（範例皆為開發/測試用途）：

範例指令：

- 從 SchemaNote 資料夾（建議）：
  docker build --no-cache --build-arg INSECURE=1 -f Dockerfiles/Dockerfile -t schemanote:local .

- 從倉儲根目錄並指定上下文為 SchemaNote：
  docker build --no-cache --build-arg INSECURE=1 -f SchemaNote/Dockerfiles/Dockerfile -t schemanote:local SchemaNote

- 將本機映像匯出為 tar：
  docker save schemanote:local -o schemanote_local.tar

- 在本機啟動容器（範例綁定 host 5005 到 container 80）：
  docker run -d --name schemanote_insec -p 5005:80 schemanote:local

注意事項：
- `INSECURE=1` 會修改映像內的 OpenSSL 設定以降低 TLS 要求，僅能在受控的測試環境使用，切勿在生產環境或公開網路使用。
- 建議使用 `.dockerignore` 排除 bin/ obj/ .vs/ 等，以減少 build context 大小並加速建置。
- 若要在 Docker 映像中啟用對外連線，請確保容器執行時設定 `ASPNETCORE_URLS=http://+:80`（Dockerfile 已設定）。

  ### 欄位層
```
  A. 欄位名稱：欄位(Column)的名稱
  B. 欄位說明：欄位的中文解釋名稱；系可編輯的擴充屬性之值（對應擴充屬性的Key為：MS_Description）
  C. 資料型態：欄位的資料型態，請注意格式應比照範例畫面，也就是要和T-SQL語法中，宣告該資料型態寫法一致，大小寫無所謂
  D. 主鍵：欄位是否為Primary Key，使用核取方塊(checkbox)表現
  E. 不為Null：欄位是否不允許NULL；換句話說，欄位是否為必填
  F. 預設值：列出完整預設值表達式
  G. 備註：欄位的中文補充說明、備註；系可編輯的擴充屬性之值（對應擴充屬性的Key為：REMARK）
```

# 原理說明
本平台註記原理在於利用資料庫物件的[擴充屬性](https://docs.microsoft.com/sql/relational-databases/system-catalog-views/extended-properties-catalog-views-sys-extended-properties)

# 注意事項
- 建議使用SQL Server版本2008(含)以上，版本2005以下可使用另一套開源工具[DDC](https://blog.miniasp.com/post/2008/05/30/Useful-tools-Data-Dictionary-Creator)
- 本平台僅異動擴充屬性，不能控制其他資料庫物件、結構
- 目前只支援資料表(Table)、檢視表(View)這兩種物件
- 目前尚未支援SSL的版本，連線字串傳送過程亦無受到保護，請於受保護的網路環境（e.g. 安全內網環境）使用
