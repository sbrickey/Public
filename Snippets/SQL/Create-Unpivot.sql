GO
CREATE OR ALTER PROCEDURE [dbo].[Create-Unpivot]
(
    @dbName           sysname      = NULL
  , @schemaInput      varchar(50)  = 'dbo'
  , @tableInput       varchar(50)  = NULL
  , @schemaOutput     varchar(50)  = 'dbo'
  , @tableOutput      varchar(50)  = NULL
  , @ColsToKeep      nvarchar(MAX) = NULL
  , @ColsToUnpivot   nvarchar(MAX) = NULL
  , @UnpivotColName   SYSNAME      = NULL
  , @ValueColName     SYSNAME      = NULL
  , @WhatIf               bit      = 0
  , @VERBOSE              bit      = 0
)
AS
BEGIN
    SET @ColsToKeep    = NULLIF(LTRIM(RTRIM(@ColsToKeep   )), '')
    SET @ColsToUnpivot = NULLIF(LTRIM(RTRIM(@ColsToUnpivot)), '')

    if (@dbName         IS NULL ) THROW 50000,'Missing parameter @dbName'         , 1;
    if (@tableInput     IS NULL ) THROW 50000,'Missing parameter @tableName'      , 1;
    if (@tableOutput    IS NULL ) THROW 50000,'Missing parameter @tableOutput'    , 1;
    if (@UnpivotColName IS NULL ) THROW 50000,'Missing parameter @UnpivotColName' , 1;
    if (@ValueColName   IS NULL ) THROW 50000,'Missing parameter @ValueColName'   , 1;
    if (@ColsToKeep IS NULL AND @ColsToUnpivot IS NULL) THROW 50000,'Missing parameter @ColsToKeep OR @ColsToUnpivot' , 1;

    DECLARE @LogTable TABLE ( [dt] datetime NOT NULL, SqlStmt nvarchar(MAX) NOT NULL )

    DECLARE @SqlStatement        nvarchar(MAX) = NULL
          , @SpidStr              varchar(  6) = CAST(@@SPID AS varchar(6))
 
 -- cleanse the ColsToKeep/ColsToUnpivot -- basically remove any surrounding []'s
    SET @ColsToKeep    = ( SELECT STRING_AGG(PARSENAME(c.[value],1), ',') FROM string_split(@ColsToKeep   , ',') c )
    SET @ColsToUnpivot = ( SELECT STRING_AGG(PARSENAME(c.[value],1), ',') FROM string_split(@ColsToUnpivot, ',') c )

 -- if ColsToKeep ^ ColsToUnpivot is undefined, fill in from the other
    DECLARE @tableInputColumns TABLE ( [Name] sysname )
    IF (@ColsToKeep IS NOT NULL AND @ColsToUnpivot IS     NULL) -- derive unpivot cols
    BEGIN
        SET @SqlStatement = 'SELECT [name] FROM [' + @dbName + '].sys.columns c WHERE c.[object_id] = object_id(''[' + @dbName + '].[' + @schemaInput + '].[' + @tableInput + ']'')'
        INSERT INTO @tableInputColumns EXEC sp_executeSQL @SqlStatement
        DELETE FROM @tableInputColumns WHERE [Name] IN ( SELECT a.[value] FROM string_split(@ColsToKeep, ',') a )
        SET @ColsToUnpivot = ( SELECT STRING_AGG(c.[Name], ',') FROM @tableInputColumns c )
    END
    IF (@ColsToKeep IS  NULL AND @ColsToUnpivot IS NOT NULL) -- derive cols-to-keep
    BEGIN
        SET @SqlStatement = 'SELECT [name] FROM [' + @dbName + '].sys.columns c WHERE c.[object_id] = object_id(''[' + @dbName + '].[' + @schemaInput + '].[' + @tableInput + ']'')'
        INSERT INTO @tableInputColumns EXEC sp_executeSQL @SqlStatement
        DELETE FROM @tableInputColumns WHERE [Name] IN ( SELECT a.[value] FROM string_split(@ColsToUnpivot, ',') a )
        SET @ColsToKeep = ( SELECT STRING_AGG(c.[Name], ',') FROM @tableInputColumns c )
    END
    PRINT 'ColsToKeep    : ' + ISNULL(@ColsToKeep   , '')
    PRINT 'ColsToUnpivot : ' + ISNULL(@ColsToUnpivot, '')

    BEGIN TRY

     -- if the table exists, drop so it can be recreated
        SET @SqlStatement = N'IF OBJECT_ID(''[' + @dbName + '].[' + @schemaOutput + '].[' + @tableOutput + ']'', ''U'') IS NOT NULL DROP TABLE [' + @dbName + '].[' + @schemaOutput + '].[' + @tableOutput + ']'
        IF (@VERBOSE = 1) BEGIN PRINT @SqlStatement ; SELECT CAST('<root><![CDATA[' + @SqlStatement + ']]></root>' AS XML); END
        INSERT INTO @LogTable VALUES ( GETDATE() , @SqlStatement ) ; IF (@WhatIf = 0) BEGIN EXECUTE sp_executeSQL @SqlStatement END

        SET @SqlStatement = N'
           SELECT ' + ( SELECT STRING_AGG('unPvt.[' + c.[value] + ']' , ',')
                          FROM string_split(@ColsToKeep, ',') c
                      ) + N', unPvt.[' + @UnpivotColName + N'], unPvt.[' + @ValueColName + N']
             INTO [' + @dbName + '].[' + @schemaOutput + '].[' + @tableOutput + ']
             FROM ( SELECT * FROM [' + @dbName + '].[' + @schemaInput  + '].[' + @tableInput  + '] ) src
          UNPIVOT ( [' + @ValueColName + '] FOR [' + @UnpivotColName + '] IN ( ' + ( SELECT STRING_AGG('[' + c.[value] + ']' , ',')
                                                                                       FROM string_split(@ColsToUnpivot, ',') c
                                                                                   ) + ' )
                  ) unPvt'
     -- run @SqlStatement
        IF (@VERBOSE = 1) BEGIN PRINT @SqlStatement ; SELECT CAST('<root><![CDATA[' + @SqlStatement + ']]></root>' AS XML); END
        INSERT INTO @LogTable VALUES ( GETDATE() , @SqlStatement ) ; IF (@WhatIf = 0) BEGIN EXECUTE sp_executeSQL @SqlStatement END

    END TRY
    BEGIN CATCH
        SELECT [dt], CAST('<root><![CDATA[' + [SqlStmt] + ']]></root>' AS XML) FROM @LogTable ORDER BY [dt]

        -- Return if there is no error information to retrieve.
        IF ERROR_NUMBER() IS NULL
            RETURN;
 
        DECLARE @ErrorNumber          INT       = ERROR_NUMBER()
              , @ErrorSeverity        INT       = ERROR_SEVERITY()
              , @ErrorState           INT       = ERROR_STATE()
              , @ErrorLine            INT       = ERROR_LINE()
              , @ErrorProcedure  NVARCHAR(200)  = ISNULL(ERROR_PROCEDURE(), '-')
        DECLARE @ErrorMessage    NVARCHAR(4000) = N'Error %d, Level %d, State %d, Procedure %s, Line %d, Message: '+ ERROR_MESSAGE();
 
        -- Raise an error: msg_str parameter of RAISERROR will contain the original error information.
        RAISERROR(@ErrorMessage, @ErrorSeverity, 1,
            @ErrorNumber,    /* parameter: original error number. */
            @ErrorSeverity,  /* parameter: original error severity. */
            @ErrorState,     /* parameter: original error state. */
            @ErrorProcedure, /* parameter: original error procedure name. */
            @ErrorLine       /* parameter: original error line number. */
            );

    END CATCH
END
GO
