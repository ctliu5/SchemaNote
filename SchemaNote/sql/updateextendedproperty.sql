DECLARE @id INT= @OBJECT_ID
      , @col_id INT= @COLUMN_ID
      , @name SYSNAME= @PROP_NAME
      , @value SQL_VARIANT= @PROP_VALUE
      , @schema SYSNAME = @SCHEMA_NAME
      , @object SYSNAME = @OBJECT_NAME
      , @column SYSNAME
	  , @qty INT = 0;

SELECT @column = c.[name], @qty = COUNT(column_id) OVER(PARTITION BY object_id) 
FROM sys.columns AS c
WHERE object_id = @id
      AND column_id = @col_id;

IF(@qty > 0)
BEGIN
EXEC sys.sp_updateextendedproperty 
     @name = @name, 
     @value = @value, 
     @level0type = N'SCHEMA', 
     @level0name = @schema, 
     @level1type = N'TABLE', 
     @level1name = @object, 
     @level2type = N'COLUMN', 
     @level2name = @column;
	 END
	 ELSE
	 BEGIN
EXEC 	  sys.sp_updateextendedproperty 
     @name = @name, 
     @value = @value, 
     @level0type = N'SCHEMA', 
     @level0name = @schema, 
     @level1type = N'TABLE', 
     @level1name = @object;
	 END