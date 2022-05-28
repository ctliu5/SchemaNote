SELECT @id = -1
       ,@col_id = 0
       ,@name = N'{0}'
       ,@value = N'{1}'
       ,@level0name = N'{2}'
       ,@level1type = N'{3}'
       ,@level1name = N'{4}'
       ,@level2name = N'{5}'
       ,@propQty = 0;

    SELECT @id = o.[object_id]
      FROM sys.objects AS o
      INNER JOIN sys.schemas AS s ON o.[schema_id] = s.[schema_id]
                                     AND s.[name] = @level0name
                                     AND o.[type] = @level1type
                                     AND o.[name] = @level1name
     WHERE o.[type] IN ('U', 'V')

IF (@id <> -1)
  BEGIN
    SET @level1type = CASE WHEN @level1type = 'U' THEN 'TABLE' WHEN @level1type = 'V' THEN 'VIEW' END

    SELECT @col_id = c.[column_id]
      FROM sys.columns AS c
     WHERE [object_id] = @id
       AND c.[name] = @level2name;

    SELECT @propQty = COUNT(*)
      FROM sys.extended_properties
     WHERE [major_id] = @id
       AND [name] = @name
       AND [minor_id] = @col_id

    IF (@propQty > 0)
      BEGIN
        IF (@col_id > 0)
          BEGIN
            EXEC sys.sp_dropextendedproperty @name = @name,@level0type = N'SCHEMA',@level0name = @level0name,@level1type = @level1type,@level1name = @level1name,@level2type = N'COLUMN',@level2name = @level2name;
          END
        ELSE
          BEGIN
            EXEC sys.sp_dropextendedproperty @name = @name,@level0type = N'SCHEMA',@level0name = @level0name,@level1type = @level1type,@level1name = @level1name;
          END
      END
  END 
