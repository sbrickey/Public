GO
CREATE OR ALTER PROCEDURE [dbo].[Check-ColumnsExist]
(
    @dbName          sysname
  , @schemaName      sysname = 'dbo'
  , @tableName       sysname
  , @colNamesCSV     varchar(MAX)
  , @MissingCols    nvarchar(MAX) OUTPUT
)
AS
BEGIN
	DECLARE @outval nvarchar(MAX) = NULL

    DECLARE @SqlStatement nvarchar(MAX) = 'SELECT [name] FROM [' + @dbName + '].sys.columns c WHERE c.[object_id] = object_id(''[' + @dbName + '].[' + @schemaName + '].[' + @tableName + ']'')'
    DECLARE @actualColumns TABLE ( [Name] sysname )
    INSERT INTO @actualColumns EXEC sp_executeSQL @SqlStatement

    DECLARE @colNames TABLE ( [Name] sysname )
	INSERT INTO @colNames SELECT DISTINCT LTRIM(RTRIM(value)) FROM STRING_SPLIT(@colNamesCSV, ',')

	SET @outval = ( SELECT 'Source is missing necessary columns : ' + STRING_AGG(r.[Name], ',')
                      FROM @colNames r LEFT JOIN @actualColumns a ON a.[Name] = r.[Name]
                     WHERE a.[Name] IS NULL )
	SET @MissingCols = @outval
END
GO
