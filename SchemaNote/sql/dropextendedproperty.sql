DECLARE @id INT= @OBJECT_ID
      , @col_id INT= @COLUMN_ID
      , @name SYSNAME= @PROP_NAME
      , @level0name SYSNAME = @SCHEMA_NAME
	  , @level1type SYSNAME = @TYPE
      , @level1name SYSNAME = @OBJECT_NAME
      , @level2name SYSNAME
	  , @qty INT = 0;

SELECT @level2name = c.[name], @qty = COUNT(column_id) OVER(PARTITION BY object_id) 
FROM sys.columns AS c
WHERE object_id = @id
      AND column_id = @col_id;

IF(@qty > 0)
BEGIN
EXEC sys.sp_dropextendedproperty 
     @name = @name, 
     @level0type = N'SCHEMA', 
     @level0name = @level0name, 
     @level1type = @level1type, 
     @level1name = @level1name, 
     @level2type = N'COLUMN', 
     @level2name = @level2name;
	 END
	 ELSE
	 BEGIN
EXEC 	  sys.sp_dropextendedproperty 
     @name = @name, 
     @level0type = N'SCHEMA', 
     @level0name = @level0name, 
     @level1type = @level1type, 
     @level1name = @level1name;
	 END