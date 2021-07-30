# 工具介紹
SchemaNote（以下簡稱本平台）是用來檢視、編輯SQL Server上自定義註記之Web平台

# 基本畫面

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
