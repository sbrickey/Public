-- Search SQL server for any place that might contain references to an SSAS (MSOLAP) server.
--   this isn't 100% exhaustive - plenty of other methods exist (SSIS in catalog/file, Agent Job to PowerShell/ActiveX to OLAP, etc)
--   but it covers some of the core SQL places to check.

-- agent jobs
select j.[enabled], j.[name], j.[description]
     , js.[step_name], js.[subsystem], js.[server], js.[database_name] --, js.[command]
     , h.[run_date], h.[run_time], h.[run_status]
  from [msdb].[dbo].[sysjobsteps] js WITH (NOLOCK)
  JOIN [msdb].[dbo].[sysjobs]     j  WITH (NOLOCK) ON j.[job_id] = js.[job_id]
 OUTER APPLY ( SELECT TOP 1 * FROM [msdb].[dbo].[sysjobhistory] h WITH (NOLOCK)
                WHERE h.[job_id] = j.[job_id] ORDER BY [run_date] DESC, [run_time] DESC
             ) h
 WHERE [subsystem] IN ( 'ANALYSISCOMMAND','ANALYSISQUERY' )
 OR js.[command] LIKE '%OPENROWSET%MSOLAP%'
 
-- linked server(s)
select * from master.sys.servers ls WHERE ls.[provider] LIKE '%OLAP%'

-- sprocs using OpenRowSet (aka unregistered linked server)
-- use a @ table to consolidate output result sets, and for faster execution
DECLARE @ModuleUsage TABLE ( [Database] SYSNAME , [definition] ntext )
INSERT INTO @ModuleUsage
EXEC sp_MSforeachdb '
  SELECT [Database] = ''?'', [definition]
    FROM [?].[sys].[sql_modules]
   WHERE [definition] LIKE ''%OPENROWSET%MSOLAP%''
'
SELECT * FROM @ModuleUsage
