WITH qty
     AS (SELECT Sum (rows) AS [QTY]
                ,[object_id]
           FROM sys.partitions
          WHERE index_id < 2
          GROUP BY [object_id])
    SELECT o.[object_id]       AS [OBJECT_ID]
           ,o.[name]           AS [NAME]
           ,s.[name]           AS [SCHEMA_NAME]
           ,type               AS [TYPE]
           ,CAST(DATEPART(YY, [create_date]) AS CHAR(4)) + '-'
           + RIGHT(CAST(100 + DATEPART(MM, [create_date]) AS CHAR(3)), 2) + '-'
           + RIGHT(CAST(100 + DATEPART(DD, [create_date]) AS CHAR(3)), 2)/* + ' '
           + RIGHT(CAST(100 + DATEPART(HH, [create_date]) AS CHAR(3)), 2) + ':'
           + RIGHT(CAST(100 + DATEPART(MI, [create_date]) AS CHAR(3)), 2) + ':'
           + RIGHT(CAST(100 + DATEPART(SS, [create_date]) AS CHAR(3)), 2)*/ AS [CREATE_DATE]
           ,CAST(DATEPART(YY, [modify_date]) AS CHAR(4)) + '-'
           + RIGHT(CAST(100 + DATEPART(MM, [modify_date]) AS CHAR(3)), 2) + '-'
           + RIGHT(CAST(100 + DATEPART(DD, [modify_date]) AS CHAR(3)), 2)/* + ' '
           + RIGHT(CAST(100 + DATEPART(HH, [modify_date]) AS CHAR(3)), 2) + ':'
           + RIGHT(CAST(100 + DATEPART(MI, [modify_date]) AS CHAR(3)), 2) + ':'
           + RIGHT(CAST(100 + DATEPART(SS, [modify_date]) AS CHAR(3)), 2)*/ AS [MODIFY_DATE]
           ,ISNULL(qty.QTY, CAST(0 AS BIGINT)) AS [QTY]
      FROM sys.objects AS o
      INNER JOIN sys.schemas AS s ON o.[schema_id] = s.[schema_id]
      LEFT JOIN  qty ON qty.[object_id] = o.[object_id]
     WHERE o.[type] IN ('U', 'V')
     ORDER BY o.[name] 
