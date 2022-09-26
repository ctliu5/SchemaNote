DECLARE @qty BIGINT, @id INT= @OBJECT_ID;
SELECT @qty = Sum (rows)
  FROM sys.partitions 
 WHERE index_id < 2
 /*
 sys.partitions.index_id
 0 = 堆積（實測MSSQL14，狀況是無索引或無叢集索引的非叢集索引）
 1 = 叢集索引
 2 或以上 = 非叢集索引（實測MSSQL14，狀況是有叢集索引的非叢集索引）
 */
   AND [object_id] = @id
SELECT o.[object_id] AS [OBJECT_ID]
       ,o.[name]     AS [NAME]
       ,s.[name]     AS [SCHEMA_NAME]
       ,type         AS [TYPE]
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
	  ,ISNULL(@qty, CAST(0 AS BIGINT)) AS [QTY]
  FROM sys.objects AS o
  INNER JOIN sys.schemas AS s ON o.[schema_id] = s.[schema_id] AND o.[object_id] = @id
 WHERE o.[type] IN ('U', 'V')
 ORDER BY o.[name]
/*
SELECT Sum (CASE 
              WHEN ( index_id < 2 ) THEN rows 
              ELSE 0 
            END) 
FROM   sys.partitions 
WHERE  object_id = @OBJECT_ID
*/