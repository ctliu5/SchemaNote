SELECT major_id  AS [MAJOR_ID]
       ,minor_id AS [MINOR_ID]
       ,NAME     AS [NAME]
       ,value    AS [VALUE]
       ,class    AS [CLASS]
  FROM sys.extended_properties
 WHERE NOT (minor_id = 0
        AND class = 1
        AND NAME = N'microsoft_database_tools_support') 
