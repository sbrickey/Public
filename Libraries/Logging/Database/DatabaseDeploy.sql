PRINT 'Log Structure rollout'

-- SERVER VALIDATION : SQLx2
IF (PATINDEX('%SQL[D|T|0]2', @@SERVERNAME) = 0)
BEGIN
	raiserror('WRONG SERVER!!!', 20, -1) with log
END

-- LOG ENVIRONMENT TO OUTPUT
IF ( @@SERVERNAME = 'SQLDEV'  ) BEGIN PRINT 'Deploy [ROLLOUT ] to : DEV'  END
IF ( @@SERVERNAME = 'SQLTEST' ) BEGIN PRINT 'Deploy [ROLLOUT ] to : TEST' END
IF ( @@SERVERNAME = 'SQLPROD' ) BEGIN PRINT 'Deploy [ROLLOUT ] to : PROD' END

-- BEGIN ROLLOUT  SCRIPT
USE [DatabaseHere]
GO

CREATE SCHEMA [Log]
GO

CREATE TABLE [Log].[Log] (
  -- Core table columns
    [Id]                 [int]            IDENTITY(1, 1)    NOT NULL  PRIMARY KEY
  , [Parent_Lineage]     [hierarchyid]                          NULL  -- max value per node is 14 digits (100tril - 1)
  , [Parent_LineageStr]                                               AS [Parent_Lineage].ToString()
  -- Core log columns
  , [TimeStamp]          [datetimeoffset]                   NOT NULL
  , [TimeStamp_Local]                                                 AS CAST(SWITCHOFFSET([TimeStamp], DATEPART(TZOFFSET, SYSDATETIMEOFFSET())) AS DATETIME2)
  , [MachineName]        [nvarchar]( 32)                    NOT NULL
  , [UserName]           [nvarchar]( 64)                    NOT NULL  DEFAULT SYSTEM_USER
  , [ProcessName]        [nvarchar](500)                    NOT NULL
  , [ProcessId]          [int]                              NOT NULL
  , [ThreadId]           [int]                              NOT NULL
  -- Supplemental log columns
  , [Category]           [nvarchar](100)                    NOT NULL
  , [EventId]            [int]                              NOT NULL
  , [Level]              [nvarchar]( 50)                    NOT NULL
  , [Message]            [nvarchar](MAX)                    NOT NULL
  , [Exception]          [nvarchar](MAX)                        NULL
)
GO


GO
CREATE PROCEDURE [Log].[Log_Insert]
(
    @Parent_LogID [int]
  , @TimeStamp    [datetimeoffset]
  , @MachineName  [nvarchar](  32)
  , @UserName     [nvarchar](  64)
  , @ProcessName  [nvarchar]( 500)
  , @ProcessID    [int]
  , @ThreadID     [int]

  , @Category     [nvarchar]( 100)
  , @EventId      [int]
  , @Level        [nvarchar](  50)
  , @Message      [nvarchar]( MAX)
  , @Exception    [nvarchar]( MAX) = NULL
)
AS

SET @TimeStamp   = ISNULL(@TimeStamp   , SYSDATETIMEOFFSET() ) -- default value
SET @MachineName = ISNULL(@MachineName , HOST_NAME()         ) -- default value
SET @UserName    = ISNULL(@UserName    , SYSTEM_USER         ) -- default value
SET @ProcessName = ISNULL(@ProcessName , APP_NAME()          ) -- default value

-- log parameters to XML, to better track the original data across various error handling paths
-- (aka, if the parent ID is bad, and/or parameters are missing, we still want the original data logged
DECLARE @ParametersToXML nvarchar(MAX) =
 ( SELECT * FROM ( VALUES       ( @Parent_LogID  , @TimeStamp  , @MachineName  , @UserName  , @ProcessName  , @ProcessID  , @ThreadID  , @Category  , @EventId  , @Level  , @Message  , @Exception  ) )
                   [Parameters] ( [Parent_LogID] , [TimeStamp] , [MachineName] , [UserName] , [ProcessName] , [ProcessID] , [ThreadID] , [Category] , [EventId] , [Level] , [Message] , [Exception] )
   FOR XML AUTO, ELEMENTS XSINIL
 )
-- Query the log table for the parent's lineage, so that the current lineage can be appended, and also perform validation
DECLARE @Parent_ID int = CASE WHEN @Parent_LogID IS NULL THEN NULL
                              ELSE ( SELECT [Id] FROM [Log].[Log] WITH (NOLOCK) WHERE [Id] = @Parent_LogID )
                              END
DECLARE @Parent_Lineage hierarchyid = CASE WHEN @Parent_LogID IS NULL THEN NULL
                                           ELSE ( SELECT [Parent_Lineage] FROM [Log].[Log] WITH (NOLOCK) WHERE [Id] = @Parent_LogID )
                                           END
DECLARE @Lineage hierarchyid = CASE WHEN @Parent_ID      IS     NULL THEN NULL
                                    WHEN @Parent_Lineage IS     NULL THEN [hierarchyid]::Parse(                       '/' + CAST(@Parent_ID AS nvarchar(5)) + '/')
                                    WHEN @Parent_Lineage IS NOT NULL THEN [hierarchyid]::Parse(@Parent_Lineage.ToString() + CAST(@Parent_ID AS nvarchar(5)) + '/')
                                    END
-- determine execution validity
DECLARE @Parent_IsValid      bit = CASE WHEN @Parent_LogID IS NOT NULL AND @Parent_ID IS NULL THEN 0 ELSE 1 END
DECLARE @Parameters_AreValid bit = CASE WHEN @MachineName IS NULL OR @ProcessName IS NULL OR @ProcessID IS NULL
                                          OR @Category IS NULL OR @EventId IS NULL OR @Level IS NULL OR @Message IS NULL THEN 0 ELSE 1 END

DECLARE @NewLogIDs TABLE ( [Id] int )
IF (@Parent_IsValid = 0 OR @Parameters_AreValid = 0) -- if the call is bad, log as such
BEGIN
    DECLARE @ExceptionMessage nvarchar(MAX) = CASE WHEN @Parent_IsValid = 0 AND @Parameters_AreValid = 1 THEN '@Parent_LogID does not exist'
	                                               WHEN @Parent_IsValid = 1 AND @Parameters_AreValid = 0 THEN 'NULL values provided'
	                                               WHEN @Parent_IsValid = 1 AND @Parameters_AreValid = 1 THEN '@Parent_LogID does not exist, NULL values provided'
	                                               END
	                                        + /* CRLF */ CHAR(13)+CHAR(10)
	                                        + 'Parameters:' + /* CRLF */ CHAR(13)+CHAR(10)
	                                        + @ParametersToXML
    INSERT INTO [Log].[Log]
           ( [Parent_Lineage] , [TimeStamp]         , [MachineName] , [UserName] , [ProcessName] , [ProcessID] , [ThreadID] , [Category] , [EventId] , [Level] , [Message]                   , [Exception]       )
    OUTPUT INSERTED.[Id] INTO @NewLogIDs
    VALUES ( @Lineage         , SYSDATETIMEOFFSET() , @@SERVERNAME  , @UserName  , 'TSQL'        , @@SPID      ,        -1  , 'SPROC'    ,        0  , 'ERROR' , '[Log].[Log_Insert] failed' , @ExceptionMessage )
END
ELSE -- otherwise, log as the caller had intended
BEGIN
    INSERT INTO [Log].[Log]
           ( [Parent_Lineage] , [TimeStamp] , [MachineName] , [UserName] , [ProcessName] , [ProcessID] , [ThreadID] , [Category] , [EventId] , [Level] , [Message] , [Exception] )
    OUTPUT INSERTED.[Id] INTO @NewLogIDs
    VALUES ( @Lineage         , @TimeStamp  , @MachineName  , @UserName  , @ProcessName  , @ProcessID  , @ThreadID  , @Category  , @EventId  , @Level  , @Message  , @Exception  )
END
SELECT [Id] FROM @NewLogIDs
GO
CREATE PROCEDURE [Log].[Log_InsertSQL]
(
    @Parent_LogID [int]
  , @Category     [nvarchar](100)
  , @EventId      [int]
  , @Level        [nvarchar]( 50)
  , @Message      [nvarchar](MAX)
  , @Exception    [nvarchar](MAX) = NULL
)
AS

-- log parameters to XML, to better track the original data across various error handling paths
-- (aka, if the parent ID is bad, and/or parameters are missing, we still want the original data logged
DECLARE @ParametersToXML nvarchar(MAX) =
 ( SELECT * FROM ( VALUES       ( @Parent_LogID  , @Category  , @EventId  , @Level  , @Message  , @Exception  ) )
                   [Parameters] ( [Parent_LogID] , [Category] , [EventId] , [Level] , [Message] , [Exception] )
   FOR XML AUTO, ELEMENTS XSINIL
 )
-- Query the log table for the parent's lineage, so that the current lineage can be appended, and also perform validation
DECLARE @Parent_ID int = CASE WHEN @Parent_LogID IS NULL THEN NULL
                              ELSE ( SELECT [Id] FROM [Log].[Log] WITH (NOLOCK) WHERE [Id] = @Parent_LogID )
                              END
DECLARE @Parent_Lineage hierarchyid = CASE WHEN @Parent_LogID IS NULL THEN NULL
                                           ELSE ( SELECT [Parent_Lineage] FROM [Log].[Log] WITH (NOLOCK) WHERE [Id] = @Parent_LogID )
                                           END
DECLARE @Lineage hierarchyid = CASE WHEN @Parent_ID IS NULL THEN NULL
                                    WHEN @Parent_Lineage IS     NULL THEN [hierarchyid]::Parse(                       '/' + CAST(@Parent_ID AS nvarchar(5)) + '/')
                                    WHEN @Parent_Lineage IS NOT NULL THEN [hierarchyid]::Parse(@Parent_Lineage.ToString() + CAST(@Parent_ID AS nvarchar(5)) + '/')
                                    END
-- determine execution validity
DECLARE @Parent_IsValid      bit = CASE WHEN @Parent_LogID IS NOT NULL AND @Parent_ID IS NULL THEN 0 ELSE 1 END
DECLARE @Parameters_AreValid bit = CASE WHEN @Category IS NULL OR @EventId IS NULL OR @Level IS NULL OR @Message IS NULL THEN 0 ELSE 1 END

DECLARE @NewLogIDs TABLE ( [Id] int )
IF (@Parent_IsValid = 0 OR @Parameters_AreValid = 0) -- if the call is bad, log as such
BEGIN
    DECLARE @ExceptionMessage nvarchar(MAX) = CASE WHEN @Parent_IsValid = 0 AND @Parameters_AreValid = 1 THEN '@Parent_LogID does not exist'
	                                               WHEN @Parent_IsValid = 1 AND @Parameters_AreValid = 0 THEN 'NULL values provided'
	                                               WHEN @Parent_IsValid = 1 AND @Parameters_AreValid = 1 THEN '@Parent_LogID does not exist, NULL values provided'
	                                               END
	                                        + /* CRLF */ CHAR(13)+CHAR(10)
	                                        + 'Parameters:' + /* CRLF */ CHAR(13)+CHAR(10)
	                                        + @ParametersToXML
    INSERT INTO [Log].[Log]
           ( [Parent_Lineage] , [TimeStamp]         , [MachineName] , [UserName]  , [ProcessName] , [ProcessID] , [ThreadID] , [Category] , [EventId] , [Level] , [Message]                      , [Exception]       )
    OUTPUT INSERTED.[Id] INTO @NewLogIDs
    VALUES ( @Lineage         , SYSDATETIMEOFFSET() , @@SERVERNAME  , SYSTEM_USER , 'TSQL'        , @@SPID      ,        -1  , 'SPROC'    ,       -1  , 'ERROR' , '[Log].[Log_InsertSQL] failed' , @ExceptionMessage )
END
ELSE -- otherwise, log as the caller had intended
BEGIN
    INSERT INTO [Log].[Log]
           ( [Parent_Lineage] , [TimeStamp]         , [MachineName] , [UserName]  , [ProcessName] , [ProcessID] , [ThreadID] , [Category] , [EventId] , [Level] , [Message] , [Exception] )
    OUTPUT INSERTED.[Id] INTO @NewLogIDs
    VALUES ( @Lineage         , SYSDATETIMEOFFSET() , @@SERVERNAME  , SYSTEM_USER , 'TSQL'        , @@SPID      ,        -1  , @Category  ,       -1  , @Level  , @Message  , @Exception  )
END

SELECT [Id] FROM @NewLogIDs
GO
CREATE PROCEDURE [Log].[Log_SelectByID]
( @LogId int )
AS
--DECLARE @LogId int = 10
-- get the root lineage (ex: /../@LogId), and also cast a copy as INT
DECLARE @RootLineage  hierarchyid = ( SELECT ISNULL(
                                                     [Parent_Lineage].GetAncestor([Parent_Lineage].GetLevel()-1)  -- get root level of lineage
                                                   , hierarchyid::Parse('/' + CAST(@LogId AS nvarchar(10)) + '/') -- or, PARENT is NULL, convert @LogID to HierarchyID
                                                   )
                                        FROM [log].[Log] WHERE [Id] = @LogId )
DECLARE @RootLogId            int = ( SELECT CAST(REPLACE(@RootLineage.ToString(), '/', '') AS int) )

SELECT *
     , [Lineage] = CASE WHEN [Parent_Lineage] IS NULL THEN '/' ELSE [Parent_Lineage].ToString() END
  FROM [Log].[Log] WITH (NOLOCK)
 WHERE [Id] = @RootLogId
    OR [Parent_Lineage].IsDescendantOf(@RootLineage) = 1
 ORDER BY [TimeStamp]
GO

GO
CREATE VIEW [Log].[Log_vw] AS
    SELECT *
      FROM [Log].[Log] WITH (NOLOCK)
GO
CREATE VIEW [Log].[Log_Latest10_vw] AS
WITH CTE AS
(
SELECT TOP 10 [log].[Log].*
  FROM [log].[Log]
 WHERE [Parent_Lineage] IS NULL
 ORDER BY [TimeStamp] DESC

UNION ALL

SELECT [log].[Log].*
  FROM [log].[Log]
  JOIN CTE
    ON [Log].[Parent_Lineage].IsDescendantOf(HierarchyId::Parse('/' + CAST(CTE.Id AS nvarchar(20)) + '/')) = 1
)
SELECT * FROM CTE --ORDER BY [TimeStamp], [Id]
GO

/***  Some Sample Data ***
**

EXEC [Log].[Log_InsertSQL] @Parent_LogID = NULL , @Category = 'abc' , @EventID = 1 , @Level = 'Info' , @Message = 'abc' , @Exception = NULL
EXEC [Log].[Log_InsertSQL] @Parent_LogID =    1 , @Category = 'def' , @EventID = 1 , @Level = ''     , @Message = ''    , @Exception = NULL

EXEC [Log].[Log_InsertSQL] @Parent_LogID = NULL , @Category = 'abc' , @EventID = 1 , @Level = 'Info' , @Message = 'abc' , @Exception = NULL
EXEC [Log].[Log_InsertSQL] @Parent_LogID =    3 , @Category = 'def' , @EventID = 1 , @Level = ''     , @Message = NULL  , @Exception = NULL -- error on @Message == NULL

EXEC [Log].[Log_InsertSQL] @Parent_LogID = NULL , @Category = 'abc' , @EventID = 1 , @Level = 'Info' , @Message = 'abc' , @Exception = NULL
EXEC [Log].[Log_InsertSQL] @Parent_LogID = 9999 , @Category = 'def' , @EventID = 1 , @Level = ''     , @Message = ''    , @Exception = NULL -- error on ParentLogID

EXEC [Log].[Log_InsertSQL] @Parent_LogID = NULL , @Category = 'abc' , @EventID = 1 , @Level = 'Info' , @Message = 'abc' , @Exception = NULL
EXEC [Log].[Log_InsertSQL] @Parent_LogID =    7 , @Category = 'def' , @EventID = 1 , @Level = ''     , @Message = ''    , @Exception = NULL
EXEC [Log].[Log_InsertSQL] @Parent_LogID =    8 , @Category = 'ghi' , @EventID = 1 , @Level = ''     , @Message = ''    , @Exception = NULL

SELECT * FROM [Log].[Log]
EXEC [Log].[Log_SelectByID] @LogId = 7

**
*/


GRANT SELECT  ON [Log].[Log_vw]         TO [public]
GRANT EXECUTE ON [Log].[Log_Insert]     TO [public]
GRANT EXECUTE ON [Log].[Log_InsertSQL]  TO [public]
GRANT EXECUTE ON [Log].[Log_SelectByID] TO [public]
GO



-- LOG ENVIRONMENT TO OUTPUT
IF ( @@SERVERNAME = 'SQLDEV'  ) BEGIN PRINT 'FINISH Deploy [ROLLOUT ] to : DEV  : ' + CONVERT(nvarchar(50), GetDate(), 121) END
IF ( @@SERVERNAME = 'SQLTEST' ) BEGIN PRINT 'FINISH Deploy [ROLLOUT ] to : TEST : ' + CONVERT(nvarchar(50), GetDate(), 121) END
IF ( @@SERVERNAME = 'SQLPROD' ) BEGIN PRINT 'FINISH Deploy [ROLLOUT ] to : PROD : ' + CONVERT(nvarchar(50), GetDate(), 121) END