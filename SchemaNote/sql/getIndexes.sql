SELECT SysIndex.object_id     AS [OBJECT_ID]
       ,SysIndex.index_id     AS [INDEX_ID]
       ,SysIndex.name         AS [NAME]
       ,SysIndexCol.column_id AS [COLUMN_ID]
       ,type                  AS [TYPE]
       ,type_desc             AS [TYPE_DESC]
       ,is_unique             AS [IS_UNIQUE]
       ,is_primary_key        AS [IS_PK]
       ,fill_factor           AS [FILL_FACTOR]
  FROM sys.indexes AS SysIndex
  INNER JOIN sys.index_columns AS SysIndexCol ON SysIndex.object_id = SysIndexCol.object_id
                                                 AND SysIndex.index_id = SysIndexCol.index_id