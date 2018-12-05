GO
CREATE OR ALTER PROCEDURE [dbo].[Create-UnionAllView]
    @dbName              SYSNAME      = NULL
  , @SchemaName          SYSNAME      = 'dbo'
  , @ViewName            SYSNAME
  , @TableNames          varchar(MAX)
  , @DefaultTableSchema  SYSNAME      = 'dbo' -- if/when the @TableNames don't specify a schema
AS
BEGIN
 -- tables used for processing
    DECLARE @Tables           TABLE ( [SortOrder] int NOT NULL, [inputText]   nvarchar(MAX) NOT NULL
                                    , [schema_id] int     NULL, [schema_name]  SYSNAME          NULL
                                    , [object_id] int     NULL, [object_name]  SYSNAME          NULL )
    DECLARE @Columns          TABLE ( [SortOrder] int NOT NULL, [column_name]  SYSNAME      NOT NULL )
    DECLARE @TableQueries     TABLE ( [SortOrder] int         , [Query]       nvarchar(MAX) NOT NULL )
    DECLARE @CRLF              CHAR(  2) = CHAR(13) + CHAR(10)
    DECLARE @SqlStmt       nvarchar(MAX)
 -- obtain object_id's for @TableNames - use OBJECT_ID to handle parsing to avoid recreating the wheel
    SET @SqlStmt = N'
        USE [' + @dbName + ']
        SELECT [SortOrder]   = ROW_NUMBER() OVER ( ORDER BY ( SELECT 1 ) )
             , [inputText]   = src.[value]
             , [schema_id]   = SCHEMA_ID(OBJECT_SCHEMA_NAME(OBJECT_ID(LTRIM(RTRIM(src.[value])))))
             , [schema_name] =           OBJECT_SCHEMA_NAME(OBJECT_ID(LTRIM(RTRIM(src.[value]))))
             , [object_id]   =                              OBJECT_ID(LTRIM(RTRIM(src.[value])))
             , [object_name] =                  OBJECT_NAME(OBJECT_ID(LTRIM(RTRIM(src.[value]))))
          FROM string_split(@TableNames, '','') src
    '
    INSERT INTO @Tables ( [SortOrder] , [inputText] , [schema_id] , [schema_name] , [object_id] , [object_name] )
    EXECUTE sp_executeSQL @SqlStmt, N'@TableNames nvarchar(MAX)', @TableNames = @TableNames
    
 -- obtain list of distinct columns, in the order that the tables declare them
    -- to support cross-database execution, easiest approach is to simply get the sys.columns into local @table
    -- instead of trying to copy @Tables into sp_executeSQL
    DECLARE @DBCols TABLE ( [object_id] int , [column_id] int , [name] SYSNAME )
    SET @SqlStmt = 'SELECT [object_id],[column_id],[name] FROM [' + @dbName + '].sys.columns c WHERE c.[object_id] IN ( ' + (SELECT STRING_AGG(t.[object_id], ',') FROM @Tables t) + ' )'
    INSERT INTO @DBCols EXECUTE sp_executeSQL @SqlStmt

 -- now it's just a matter of joining local table vars
    ;WITH cteColOrder AS (SELECT ColOrder = ROW_NUMBER() OVER ( ORDER BY t.[SortOrder], c.[column_id] ), t.*, ColName = c.[name], c.[column_id]
                            FROM @Tables t JOIN @DBCols c ON c.[object_id] = t.[object_id] )
    INSERT INTO @Columns ( [SortOrder] , [column_name] )
    SELECT [SortOrder] = ROW_NUMBER() OVER ( ORDER BY MIN([ColOrder]) ), [ColName]
      FROM cteColOrder
     GROUP BY [ColName]
     ORDER BY MIN([ColOrder]) ASC
        
    -- calculate per-table SELECT statement with NULL for missing columns
    INSERT INTO @TableQueries
    SELECT t.[SortOrder]
         , [Query] = N'SELECT '
                   + STRING_AGG( '[' + c.[column_name] + ']' + CASE WHEN sc.[column_id] IS NULL THEN ' = NULL' ELSE '' END
                               , ','
                               )
                   + N' FROM [' + t.[schema_name] + '].[' + t.[object_name] + '] WITH (NOLOCK)'
      FROM @Tables t
     CROSS JOIN @Columns c
      LEFT JOIN @DBCols sc ON sc.[object_id] = t.[object_id] AND sc.[name] = c.[column_name]
     GROUP BY t.[SortOrder], t.[schema_name], t.[object_name]

 -- now just wrap it into a CREATE VIEW statement
    DECLARE @UnionAllSeparator nvarchar(20) = @CRLF + ' UNION ALL '
    SET @SqlStmt = 'CREATE OR ALTER VIEW [' + @schemaName + '].[' + @viewName + ']'
         + @CRLF + 'AS'
         + @CRLF + 'SELECT *'
         + @CRLF + '  FROM ('
         + @CRLF + '           ' + ( SELECT STRING_AGG(q.[Query] , @UnionAllSeparator) FROM @TableQueries q )
         + @CRLF + '       ) a'
    -- SQL stupidity : to create a view in another database, sp_executesql must be called IN the other DB, which means
    --                 that this needs to run a double-nested dynamic SQL statement.
    -- see: https://www.bobpusateri.com/archive/2011/10/creating-views-in-another-database/
    -- cannot CREATE VIEW [db].[schema].[name]
    --   ERROR : 'CREATE/ALTER VIEW' does not allow specifying the database name as a prefix to the object name.
    -- cannot chain USE [database] with CREATE VIEW
    --   ERROR : 'CREATE VIEW' must be the first statement in a query batch.
    -- cannot use GO (batch separators) within @SqlStmt, since sp_executeSQL only runs a single batch and GO as a construct
    -- is only recognized within SSMS as a batch separator
    --   ERROR : Incorrect syntax near 'GO'.
    DECLARE @SqlStmt2 NVARCHAR(MAX) = 'EXEC [' + @dbName + '].dbo.sp_executesql @stmt'
    EXECUTE sp_executeSQL @SqlStmt2, N'@stmt NVARCHAR(MAX)', @stmt=@SqlStmt
END
GO
