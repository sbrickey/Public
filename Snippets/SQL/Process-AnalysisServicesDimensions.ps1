$ssasServerName   = "ssas"
$ssasDatabaseName = "CubeDatabase"

[void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.AnalysisServices")
$svr = new-Object([Microsoft.AnalysisServices.Server])
$svr.Connect($ssasServerName)
$svr.CaptureXml = $true    # all (api issued) commands are now SCRIPTED, and NOT executed
$db = $svr.Databases.FindbyName($ssasDatabaseName)
$db.Dimensions | % { $_.Process([Microsoft.AnalysisServices.ProcessType]::ProcessFull, [Microsoft.AnalysisServices.WriteBackTableCreation]::UseExisting); $_.Dispose() }
$db.Dispose()

$Xmla = $svr.ConcatenateCaptureLog($true, $true, $true) # ProcessTransactional, ProcessParallel, ProcessAffected
Write-Output ( "Generated XMLA:" + [Environment]::NewLine + $Xmla )

$svr.CaptureXml = $false    # all (api issued) commands are now EXECUTED
Write-Output ( [DateTime]::Now.ToString("yyyy/MM/dd  hh:mm:ss tt") + " - START"    )
$results = $svr.Execute($Xmla)
Write-Output ( [DateTime]::Now.ToString("yyyy/MM/dd  hh:mm:ss tt") + " - COMPLETE" )

Write-Output "Messages..."
$results | % { $_.Messages }

$svr.Disconnect()
$svr.Dispose()
