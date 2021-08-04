WITH sys_columns
     AS (SELECT c.[object_id]
                ,c.[name]
                ,c.[column_id]
                ,c.[user_type_id]
                ,c.[is_nullable]
                ,c.[default_object_id]
                ,CAST(CASE -- int/decimal/numeric/real/float/money  
                        WHEN c.[system_type_id] IN (48, 52, 56, 59,
                                                    60, 62, 106, 108,
                                                    122, 127) THEN c.[precision]
                      END AS TINYINT) AS NUMERIC_PRECISION
                ,CAST(CASE -- datetime/smalldatetime  
                        WHEN c.[system_type_id] IN (40, 41, 42, 43,
                                                    58, 61) THEN NULL
                        ELSE ODBCSCALE(c.[system_type_id], c.[scale])
                      END AS INT)     AS NUMERIC_SCALE
				,c.[is_computed]
           FROM sys.columns AS c
           JOIN   sys.objects AS o ON c.[object_id] = o.[object_id]
          WHERE o.[type] IN ('U', 'V')),
     columns_info
     AS (   SELECT c.[object_id]                                         AS [OBJECT_ID]
                   ,COLUMNPROPERTY(c.[object_id], c.[name], 'ColumnId')  AS [COLUMN_ID]
                   ,c.[name]/*QUOTENAME(c.[name])*/                    AS [NAME]
                   ,ISNULL(ty.[name], '')                              AS [TYPE_NAME]
                   ,CASE
                      WHEN ty.[name] IN ('decimal', 'numeric') THEN '(' + CAST(NUMERIC_PRECISION AS VARCHAR(5)) + ',' + CAST(NUMERIC_SCALE AS VARCHAR(5)) + ')'
                      WHEN COLUMNPROPERTY(c.[object_id], c.[name], 'charmaxlen') = -1 THEN 'max'
                      WHEN ISNULL(COLUMNPROPERTY(c.[object_id], c.[name], 'charmaxlen'), '') <> '' THEN '(' + CAST(COLUMNPROPERTY(c.[object_id], c.[name], 'charmaxlen') AS NVARCHAR(10)) + ')'
                      ELSE ''
                    END                                                AS [LENGTH]
                   ,CASE
                      WHEN kic.column_id IS NULL THEN CAST(0 AS BIT)
                      ELSE CAST(1 AS BIT)
                    END                                                AS [IS_PK]
					,c.[is_computed]								AS [IS_COMPUTED]
					,CASE	
						WHEN c.is_computed=1 THEN cc.definition
						ELSE ''
					END												AS [DEFINITION]
                   ,CASE
                      WHEN c.is_nullable = 1 THEN CAST(0 AS BIT)
                      ELSE CAST(1 AS BIT)
                    END                                                AS [DISALLOW_NULL]
                   ,ISNULL(OBJECT_DEFINITION(c.default_object_id), '') AS [DEFUALT]
              FROM sys_columns AS c
			  LEFT JOIN sys.computed_columns AS cc ON c.object_id=cc.object_id and c.column_id=cc.column_id
              LEFT JOIN sys.types AS ty ON ty.user_type_id = c.user_type_id
              LEFT JOIN (SELECT ic.column_id
                                ,k.parent_object_id
                           FROM sys.key_constraints k
                           JOIN   sys.index_columns ic ON ic.[object_id] = k.[parent_object_id]
                                                          AND ic.[index_id] = k.[unique_index_id]
                                                          AND k.[type] = 'PK') AS kic ON kic.[column_id] = c.[column_id]
                                                                                       AND kic.parent_object_id = c.[object_id])
SELECT *
  FROM columns_info
ORDER BY COLUMN_ID;
