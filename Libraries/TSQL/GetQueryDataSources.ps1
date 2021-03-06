[void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Management.SqlParser")

function GetQueryDataSources($Sql) {

    $outval = @()

    $ParseOptions = New-Object Microsoft.SqlServer.Management.SqlParser.Parser.ParseOptions
    $ParseOptions.BatchSeparator = 'GO'

    $Parser = new-object Microsoft.SqlServer.Management.SqlParser.Parser.Scanner($ParseOptions)
    $Parser.SetSource($Sql, 0)
    $Token = [Microsoft.SqlServer.Management.SqlParser.Parser.Tokens]::TOKEN_SET
    $Start = 0
    $End   = 0
    $State = 0 
    $IsEndOfBatch = $false
    $IsMatched = $false
    $IsExecAutoParamHelp = $false

    $allTokens = @()
    $lastTokenID = $null
    $TOKENS_ReferringToRelevantObject = $true
    while(
           ($Token = $Parser.GetNext([ref]$State ,[ref]$Start, [ref]$End, [ref]$IsMatched, [ref]$IsExecAutoParamHelp )) `
           -ne `
           [Microsoft.SqlServer.Management.SqlParser.Parser.Tokens]::EOF
         ) {

        try{ ($TokenPrs = [Microsoft.SqlServer.Management.SqlParser.Parser.Tokens]$Token) | Out-Null }catch{ $TokenPrs = $Token }
        $segment = $Sql.Substring( $Start, ($end - $Start) + 1 )


        switch ($TokenPrs)
        {
            ## THESE TOKENS CANNOT BEGIN A STATEMENT OR BE PROCEEDED BY A (RELEVANT) TOKEN_ID
            { $_ -eq [Microsoft.SqlServer.Management.SqlParser.Parser.Tokens]::TOKEN_INTO }
            {
                $TOKENS_ReferringToRelevantObject = $true
            }

            ## THESE TOKENS CAN BE PRECEEDED BY ANOTHER RELEVANT TOKEN
            { $_ -eq [Microsoft.SqlServer.Management.SqlParser.Parser.Tokens]::TOKEN_FROM -or    ## CASE: INSERT INTO x FROM a
              $_ -eq [Microsoft.SqlServer.Management.SqlParser.Parser.Tokens]::TOKEN_UPDATE -or  ## CASE: SELECT FROM x; UPDATE a
              $_ -eq [Microsoft.SqlServer.Management.SqlParser.Parser.Tokens]::TOKEN_EXECUTE -or ## CASE: SELECT FROM x; EXEC a
              $_ -eq [Microsoft.SqlServer.Management.SqlParser.Parser.Tokens]::TOKEN_JOIN }
            {
                ## if a JOIN occurs while identifying tables/views, push and reset lastTokenID
                if ($TOKENS_ReferringToRelevantObject -eq $true) {
                    if ( $DEBUG ) {
                        $o = [String]::Format("Adding token: [{0,10}]", $lastTokenID)
                        Write-Host $o
                    }

                    $allTokens += $lastTokenID
                    $lastTokenID = $null
                }
                $TOKENS_ReferringToRelevantObject = $true
            }

            { $_ -eq 44 }  ## comma (,)
            {
                ## if a comma occurs while identifying tables/views, it is an implicit JOIN - push and reset lastTokenID
                if ($TOKENS_ReferringToRelevantObject -eq $true) {
                    if ( $DEBUG ) {
                        $o = [String]::Format("Adding token: [{0,10}]", $lastTokenID)
                        Write-Host $o
                    }

                    $allTokens += $lastTokenID
                    $lastTokenID = $null
                }
            }



            ## THESE TOKENS ACTUALLY MATTER
            { $_ -eq [Microsoft.SqlServer.Management.SqlParser.Parser.Tokens]::TOKEN_ID }
            {
                ## IGNORE TEMP TABLES
                if ( $segment.StartsWith("#") -or $segment.StartsWith("@") ) {
                    $TOKENS_ReferringToRelevantObject = $false
                } # if segment starts with "#" or "@"

                if ($TOKENS_ReferringToRelevantObject -eq $true) {

                    ## IGNORE ALIAS NAMES
                    if ( [String]::IsNullOrEmpty( $lastTokenID ) -or `
                         $lastTokenID.EndsWith(".") ) {
                        if (-not $segment.StartsWith('[')) { $lastTokenID += "[" }
                        $lastTokenID += $segment
                        if (-not $segment.EndsWith(']')) { $lastTokenID += "]" }
                    } # if lastTokenID.EndsWith(".")
                } # if TOKEN_ID is referring to a relevant object
            } # TOKEN_ID

            { $_ -eq 46 }  ## decimal (.)
            {
                if ($TOKENS_ReferringToRelevantObject -eq $true) {
                    $lastTokenID += $segment
                }
            }

            ## OTHERWISE, RESET
            default
            {
                if ($lastTokenID -ne $null)
                {

                    if ( $DEBUG ) {
                        $o = [String]::Format("Adding token: [{0,10}]", $lastTokenID)
                        Write-Host $o
                    }

                    $allTokens += $lastTokenID
                    $lastTokenID = $null
                }
                $TOKENS_ReferringToRelevantObject = $false
            }
        }

        if ( $DEBUG ) {
            $o = [String]::Format("Token: [{0,25}]    Segment: [{1,15}]     isMatched: [{2,10}]    IsExecAutoParamHelp: [{3}]", $TokenPrs, $segment, $IsMatched, $IsExecAutoParamHelp)
            Write-Host $o
        }

    }

    if ($lastTokenID -ne $null) { $allTokens += $lastTokenID }

    # filter out any temporary tokens
    $allTokens | ? { $_ -ne $null } `
               | ? { -not ($_.StartsWith("[#") -or $_.StartsWith("[@") ) } `
               | % { $outval += $_ }

    return $outval
}


#########################
##                     ##
##  TESTING FRAMEWORK  ##
##                     ##
#########################


function TestScenario([string]$name, $inputData, $output)
{
  $object = New-Object –TypeName PSObject
  $object | Add-Member -MemberType NoteProperty –Name Name           –Value $name
  $object | Add-Member -MemberType NoteProperty –Name Input          –Value $inputData
  $object | Add-Member –MemberType NoteProperty –Name ExpectedOutput –Value $output
  return $object
}
set-alias ?: Invoke-Ternary -Option AllScope
filter Invoke-Ternary ([scriptblock]$decider, [scriptblock]$ifTrue, [scriptblock]$ifFalse)
{
   if (&$decider) { &$ifTrue } else { &$ifFalse }
}


##################
##              ##
##  UNIT TESTS  ##
##              ##
##################

function Tests() {
    $Scenarios = @()
    $Scenarios += TestScenario -name "Null input"    -input $null               -output $null
    $Scenarios += TestScenario -name "Empty String"  -input ""                  -output $null

    $Scenarios += TestScenario -name "SPROC - Base"                               -input          "cde"                  -output @(         "[cde]" )
    $Scenarios += TestScenario -name "SPROC - EXEC Base"                          -input "EXEC     cde"                  -output @(         "[cde]" )
    $Scenarios += TestScenario -name "SPROC - Qualfiied Name"                     -input         "[cde]"                 -output @(         "[cde]" )
    $Scenarios += TestScenario -name "SPROC - Qualfiied Name + param"             -input         "[cde]'f'"              -output @(         "[cde]" )
    $Scenarios += TestScenario -name "SPROC - Schema Qualified"                   -input        "b.cde"                  -output @(     "[b].[cde]" )
    $Scenarios += TestScenario -name "SPROC - DB + Schema Qualified"              -input      "a.b.cde"                  -output @( "[a].[b].[cde]" )
    $Scenarios += TestScenario -name "SPROC - Schema Qualified + Identifier"      -input      "[b].cde"                  -output @(     "[b].[cde]" )
    $Scenarios += TestScenario -name "SPROC - DB + Schema Qualified + Identifier" -input  "[a].[b].cde"                  -output @( "[a].[b].[cde]" )
    $Scenarios += TestScenario -name "SPROC - DB + Schema Qualified + Identifier" -input "[a].[b].[cde]"                 -output @( "[a].[b].[cde]" )
    $Scenarios += TestScenario -name "SPROC - Identifier + param"                 -input "[a].[b].[cde]'f'"              -output @( "[a].[b].[cde]" )

    $Scenarios += TestScenario -name "Select - Base"                              -input "SELECT * FROM     cde"         -output @(         "[cde]" )
    $Scenarios += TestScenario -name "Select - Base + Alias"                      -input "SELECT * FROM     cde f"       -output @(         "[cde]" )
    $Scenarios += TestScenario -name "Select - Schema Qualified"                  -input "SELECT * FROM   b.cde"         -output @(     "[b].[cde]" )
    $Scenarios += TestScenario -name "Select - Schema Qualified + Alias"          -input "SELECT * FROM   b.cde f"       -output @(     "[b].[cde]" )
    $Scenarios += TestScenario -name "Select - DB + Schema Qualified"             -input "SELECT * FROM a.b.cde"         -output @( "[a].[b].[cde]" )
    $Scenarios += TestScenario -name "Select - DB + Schema Qualified + Alias"     -input "SELECT * FROM a.b.cde f"       -output @( "[a].[b].[cde]" )
    $Scenarios += TestScenario -name "Select - Qualified + Identifier"            -input "SELECT * FROM [a].b.[cde]"     -output @( "[a].[b].[cde]" )
    $Scenarios += TestScenario -name "Select - Qualified + Identifier"            -input "SELECT * FROM [a].[b].[cde] a" -output @( "[a].[b].[cde]" )
    $Scenarios += TestScenario -name "Select - Two resultsets"                    -input "SELECT * FROM  cde   SELECT * FROM ijk" -output @( "[cde]", "[ijk]" )
    $Scenarios += TestScenario -name "Select - Two resultsets"                    -input "SELECT * FROM [cde]; SELECT * FROM ijk" -output @( "[cde]", "[ijk]" )
    $Scenarios += TestScenario -name "Select - DB..Table"                         -input "SELECT * FROM a..cde"                   -output @( "[a]..[cde]" )
    $Scenarios += TestScenario -name "Select - [DB]..[Table]Alias"                -input "SELECT * FROM [a]..[cde]f"              -output @( "[a]..[cde]" )

    $Scenarios += TestScenario -name "Select - table, table"                      -input "SELECT * FROM cde, ijk"                 -output @( "[cde]", "[ijk]" )
    $Scenarios += TestScenario -name "Select - table JOIN table"                  -input "SELECT * FROM cde JOIN ijk"             -output @( "[cde]", "[ijk]" )

    $Scenarios += TestScenario -name "Select - table JOIN #table"                 -input "SELECT * FROM cde JOIN #ijk"            -output @( "[cde]" )

    $Scenarios += TestScenario -name "Select - #TEMP table"                       -input "SELECT * FROM #x"                       -output $null
    $Scenarios += TestScenario -name "Select - @TEMP table"                       -input "SELECT * FROM @x"                       -output $null
    $Scenarios += TestScenario -name "Select - #TEMP table + alias"               -input "SELECT * FROM #x cde"                   -output $null
    $Scenarios += TestScenario -name "Select - @TEMP table + alias"               -input "SELECT * FROM @x cde"                   -output $null

    $Scenarios += TestScenario -name "Insert - into TEMP table"                   -input "INSERT INTO #x"                         -output $null
    $Scenarios += TestScenario -name "Insert - into real table"                   -input "INSERT INTO cde"                        -output @(            "[cde]" )
    $Scenarios += TestScenario -name "Insert - into TEMP from real"               -input "INSERT INTO #x FROM b.cde"              -output @(        "[b].[cde]" )
    $Scenarios += TestScenario -name "Insert - into real from real"               -input "INSERT INTO x FROM b.cde"               -output @( "[x]", "[b].[cde]" )

    $Scenarios += TestScenario -name "Update - real table"                        -input "UPDATE cde"                             -output @( "[cde]" )
    $Scenarios += TestScenario -name "Update - #TEMP table"                       -input "UPDATE #x"                              -output $null

    $Scenarios += TestScenario -name "Update, Select"                             -input "UPDATE  cde SELECT * FROM  ijk"         -output @( "[cde]", "[ijk]" )
    $Scenarios += TestScenario -name "Update, Select"                             -input "UPDATE #cde SELECT * FROM  ijk"         -output @( "[ijk]" )
    $Scenarios += TestScenario -name "Update, Select"                             -input "UPDATE  cde SELECT * FROM #ijk"         -output @( "[cde]" )

    $Scenarios += TestScenario -name "Select, Update"                             -input "SELECT * FROM  ijk UPDATE  cde"         -output @( "[cde]", "[ijk]" )
    $Scenarios += TestScenario -name "Select, Update"                             -input "SELECT * FROM  ijk UPDATE #cde"         -output @( "[ijk]" )
    $Scenarios += TestScenario -name "Select, Update"                             -input "SELECT * FROM #ijk UPDATE  cde"         -output @( "[cde]" )

    $Scenarios += TestScenario -name "Delete - real table"                        -input "DELETE FROM cde"                        -output @( "[cde]" )
    $Scenarios += TestScenario -name "Delete - #TEMP table"                       -input "DELETE FROM #x"                         -output $null
    $Scenarios += TestScenario -name "Delete - @TEMP table"                       -input "DELETE FROM @x"                         -output $null


    foreach ($scenario in $Scenarios)
    {
        # Act
        $actual = GetQueryDataSources $scenario.Input

        $passed = ( ?: { $actual -eq $null -and $scenario.ExpectedOutput -eq $null } { $true } `
                       { ( ?: { $actual -eq $null -xor $scenario.ExpectedOutput -eq $null } { $false } `
                              { ( ?: { diff $actual $scenario.ExpectedOutput } { $false } { $true } ) } `
                       ) } `
                  )

        $msg = [String]::Format("TEST {1} : [{0,-50}]{2}", `
                                $Scenario.Name, `
                                ( ?: { $passed } { "PASSED" } { "FAILED" } ), `
                                ( ?: { $passed } { "" } `
                                                 { [String]::Format(" : Expected [{0}] : Actual [{1}]", `
                                                                    ( ?: { $scenario.ExpectedOutput -eq $null } `
                                                                         { "`$null" } `
                                                                         { [String]::Join(",", $scenario.ExpectedOutput) } `
                                                                    ), `
                                                                    ( ?: { $actual -eq $null } `
                                                                         { "`$null" } `
                                                                         { [String]::Join(",", $actual) } `
                                                                    ) `
                                                                   ) `
                                                 } `
                                ) `
                               )
        Write-Host $msg
    } # foreach scenario
} # function Tests

Tests