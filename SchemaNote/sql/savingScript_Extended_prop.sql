DECLARE @id int = @OBJECT_ID,
        @col_id int = @COLUMN_ID,
        @name sysname = @PROP_NAME,
        @value sql_variant = @PROP_VALUE,
        @level0name sysname,
        @level1type sysname,
        @level1name sysname,
        @level2name sysname,
        @qty int = 0,
        @propQty int = 0;

SELECT
  @level0name = s.[name], --SCHEMA_NAME
  @level1name = o.[name], --OBJECT_NAME
  @level1type = o.[type]  --TYPE
FROM sys.objects AS o
INNER JOIN sys.schemas AS s
  ON o.[schema_id] = s.[schema_id]
  AND o.[object_id] = @id
WHERE o.[type] IN ('U', 'V');

IF (@level0name IS NOT NULL
  AND @level1name IS NOT NULL
  AND @level1type IS NOT NULL)
BEGIN
SET @level1type = CASE WHEN @level1type = 'U' THEN 'TABLE'
                       WHEN @level1type = 'V' THEN 'VIEW'
					   END
  SELECT
    @level2name = c.[name],
    @qty = COUNT(column_id) OVER (PARTITION BY [object_id])
  FROM sys.columns AS c
  WHERE [object_id] = @id
  AND column_id = @col_id;

  SELECT
    @propQty = COUNT(*)
  FROM sys.extended_properties
  WHERE [major_id] = @id
  AND [name] = @name
  AND ([minor_id] = @col_id
  OR @qty = 0)

  IF (@propQty > 0)
  BEGIN
    IF (@qty > 0)
    BEGIN
      EXEC sys.sp_updateextendedproperty
	  @name = @name,
      @value = @value,
      @level0type = N'SCHEMA',
      @level0name = @level0name,
      @level1type = @level1type,
      @level1name = @level1name,
      @level2type = N'COLUMN',
      @level2name = @level2name;
    END
    ELSE
    BEGIN
      EXEC sys.sp_updateextendedproperty
	  @name = @name,
      @value = @value,
      @level0type = N'SCHEMA',
      @level0name = @level0name,
      @level1type = @level1type,
      @level1name = @level1name;
    END
  END
  ELSE
  BEGIN
    IF (@qty > 0)
    BEGIN
      EXEC sys.sp_addextendedproperty
	  @name = @name,
      @value = @value,
      @level0type = N'SCHEMA',
      @level0name = @level0name,
      @level1type = @level1type,
      @level1name = @level1name,
      @level2type = N'COLUMN',
      @level2name = @level2name;
    END
    ELSE
    BEGIN
      EXEC sys.sp_addextendedproperty
	  @name = @name,
      @value = @value,
      @level0type = N'SCHEMA',
      @level0name = @level0name,
      @level1type = @level1type,
      @level1name = @level1name;
    END
  END
END
GO