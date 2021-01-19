DECLARE @id INT= @OBJECT_ID;
SELECT s.[name] AS [SCHEMA_NAME], 
       o.[name] AS [OBJECT_NAME],
	   o.[type] AS [TYPE]
FROM sys.objects AS o
     INNER JOIN sys.schemas AS s ON o.[schema_id] = s.[schema_id]
                                    AND o.[object_id] = @id
WHERE o.[type] IN('U', 'V');