GO
CREATE OR ALTER PROCEDURE [dbo].[Get-RandomSample]
(
    @dbName       varchar(50) = NULL
  , @schemaInput  varchar(50) = 'dbo'
  , @tableInput   varchar(50) = NULL
  , @schemaOutput varchar(50) = 'dbo'
  , @tableOutput  varchar(50) = NULL
  , @Percentage   int         = 10
  , @PrimaryKey   varchar(50) = 'ID'
)
AS
BEGIN
    if (@dbName      IS NULL ) THROW 50000,'Missing parameter @dbName'   , 1;
    if (@tableInput  IS NULL ) THROW 50000,'Missing parameter @tableName', 1;
    if (@tableOutput IS NULL ) THROW 50000,'Missing parameter @tableOutput', 1;

    DECLARE @SqlStatement nvarchar(MAX) = NULL

    SET @SqlStatement = '
 -- if the table exists, drop so it can be recreated
    IF OBJECT_ID(''[' + @dbName + '].[' + @schemaOutput + '].[' + @tableOutput + ']'', ''U'') IS NOT NULL '
    + N'DROP TABLE [' + @dbName + '].[' + @schemaOutput + '].[' + @tableOutput + ']

 -- now the CTE
    ;WITH cteSrc AS ( -- alias for the source table, which simplifies the dynamic SQL
        SELECT * FROM [' + @dbName + '].[' + @schemaInput + '].[' + @tableInput  + ']
    ), ctePKs AS (  -- unique IDs
        SELECT DISTINCT [' + @PrimaryKey + ']
          FROM cteSrc
    ), SubsetPKs AS ( -- sampling
        SELECT ' + @PrimaryKey + '
          FROM ctePKs
         WHERE (ABS( cast( (binary_checksum(*) * rand()) as int)) % 100) < ' + CAST(@Percentage AS varchar(10)) +'
    ), finalCTE AS ( -- source filtered to sample
        SELECT cteSrc.*
          FROM cteSrc
         WHERE cteSrc.[' + @PrimaryKey + '] IN ( SELECT [' + @PrimaryKey + '] FROM SubsetPKs )
    )
    SELECT *
      INTO [' + @dbName + '].[' + @schemaOutput + '].[' + @tableOutput + ']
      FROM finalCTE
    '
    PRINT @SqlStatement
    EXEC sp_executeSQL @SqlStatement
END
GO
