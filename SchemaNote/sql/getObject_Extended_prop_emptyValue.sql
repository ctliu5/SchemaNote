WITH PROP
     AS (SELECT [major_id] AS [MAJOR_ID], 
                [minor_id] AS [MINOR_ID], 
                [name] AS [PROP_NAME], 
                [value] AS [PROP_VALUE], 
                [class] AS [CLASS]
         FROM sys.extended_properties
         WHERE NOT([minor_id] = 0 AND 
                   [class] = 1 AND 
                   ([name] = N'microsoft_database_tools_support' OR [name] LIKE 'MS_DiagramPane%'))
			   AND ISNULL([value], '') = ''),
     TBL
     AS (SELECT s.[name] AS [SCHEMA_NAME], 
                o.[object_id] AS [OBJECT_ID], 
                o.[name] AS [NAME], 
                type AS [TYPE], 
                0 AS [COLUMN_ID],
                NULL AS [COLUMN_NAME] 
         FROM sys.objects AS o
              INNER JOIN sys.schemas AS s ON o.[schema_id] = s.[schema_id] AND 
                                             o.[type] IN('U', 'V')),
     COL
     AS (SELECT o.[SCHEMA_NAME],
                o.[OBJECT_ID],
                o.[NAME],
                o.[TYPE],
                c.[column_id] AS [COLUMN_ID],
                c.[name] AS [COLUMN_NAME]
         FROM TBL AS o
              INNER JOIN sys.columns AS c ON c.[object_id] = o.[object_id]),
     Result
	 AS (SELECT *
           FROM TBL AS o
                INNER JOIN PROP AS p ON p.[MAJOR_ID] = o.[object_id] AND 
                                        p.[MINOR_ID] = 0
          UNION
         SELECT *
           FROM COL AS c
                INNER JOIN PROP AS p ON p.[MAJOR_ID] = c.[object_id] AND 
                                        p.[MINOR_ID] = c.[column_id])
     SELECT *
	   FROM Result