$Environments = @( "DEV" , "TEST" , "PROD" )
$RSServers    = @{ "DEV"  = "http://SqlRsDev"
                 ; "TEST" = "http://SqlRsTest"
                 ; "PROD" = "http://SqlRsProd"
                 }
$EnvironmentA = "TEST"
$EnvironmentB = "PROD"
$OutputFolder = "C:\RS\UpgradeValidationResults"
$LogFile      = "$OutputFolder\LogFile.txt"
$Reports  = @(
               ( New-Object PSObject -Property @{ ReportPath   = "/Path/To/RDL"
                                                ; ReportParams = @{} } )
             , ( New-Object PSObject -Property @{ ReportPath   = "/Path/To/RDL2"
                                                ; ReportParams = @{ PrimaryKeyID = "12345" ; AnotherID = "67890" } } )
             )

# Assembly References
[System.Reflection.Assembly]::LoadFile("C:\RS\Nuget\Pdfium\v2.13.0.0\PdfiumViewer.dll") | Out-Null

Function cUsing {
    # REFERENCE: http://weblogs.asp.net/adweigert/archive/2008/08/27/powershell-adding-the-using-statement.aspx
    param (
        [System.IDisposable] $inputObject = $(throw "The parameter -inputObject is required."),
        [ScriptBlock] $scriptBlock = $(throw "The parameter -scriptBlock is required.")
    )
    Try { &$scriptBlock }
    Finally { if ($inputObject -ne $null) { if ($inputObject.psbase -eq $null) {$inputObject.Dispose() } else { $inputObject.psbase.Dispose() } } }
}

function Get-SsrsReportUrl {
  param (
    $SsrsServer
  , $Report
  , [switch] $AsPdf
  )

    $UriParams = @{ "rs:Command" = "Render" }
    if ($AsPdf) { $UriParams.Add("rs:Format", "PDF") }

    $Report.ReportParams.Keys | % { $UriParams.Add($_, $Report.ReportParams[$_]) }

    $EncodedRDL     = [System.Web.HttpUtility]::UrlEncode($Report.ReportPath)

    $EncodedParams  = $UriParams.Keys | % { "{0}={1}" -f $_, [System.Web.HttpUtility]::UrlEncode($UriParams[$_]) }
    $EncodedParams  = [String]::Join("&", $EncodedParams)

    $ReportUrl      = "$SsrsServer/ReportServer/Pages/ReportViewer.aspx?$EncodedRDL&$EncodedParams"
    return $ReportUrl
}

function Get-HttpWebResponse {
    param ($url, $outPath)

    $req = New-Object System.Net.WebClient
    $req.UseDefaultCredentials = $true

    $x = [byte[]]$null
    try   { $x = $req.DownloadData($url) }
    catch { [System.IO.File]::AppendAllText($LogFile, "ERROR : Get-HttpWebResponse ( $url )$([Environment]::NewLine)") }


    if ([String]::IsNullOrWhiteSpace($outPath))
    { return $x }
    else
    { [System.IO.File]::WriteAllBytes($outPath, $x) }
}
function ConvertTo-PDF([byte[]]$bytes) {
    $ms    = [System.IO.MemoryStream]::new($bytes)
    $pdf   = [PdfiumViewer.PdfDocument]::Load($ms)
    $ms.Dispose()
    return $pdf
}

function Get-ImageDiff {
    param ([System.Drawing.Image]$ImgA, [System.Drawing.Image]$ImgB)
    
    # for the base copy of each page into its output (source and dest are both copied at 10%)
    $matrixBase = [float[][]] @( , @( 1 , 0 , 0 , 0   , 0 )
                                 , @( 0 , 1 , 0 , 0   , 0 )
                                 , @( 0 , 0 , 1 , 0   , 0 )
                                 , @( 0 , 0 , 0 , 0.1 , 0 )
                                 , @( 0 , 0 , 0 , 0   , 0 )
                               )
    $colorMatrixBase = New-Object System.Drawing.Imaging.ColorMatrix( (, $matrixBase ) )
    $imgAttrBase = [System.Drawing.Imaging.ImageAttributes]::new()
    $imgAttrBase.SetColorMatrix($colorMatrixBase, [System.Drawing.Imaging.ColorMatrixFlag]::Default, [System.Drawing.Imaging.ColorAdjustType]::Bitmap)

    # for the differences, copy into the output at 50%
    $matrixDiff = [float[][]] @( , @( 1 , 0 , 0 , 0   , 0 )
                                 , @( 0 , 1 , 0 , 0   , 0 )
                                 , @( 0 , 0 , 1 , 0   , 0 )
                                 , @( 0 , 0 , 0 , 0.5 , 0 )
                                 , @( 0 , 0 , 0 , 0   , 0 )
                               )
    $colorMatrixDiff = New-Object System.Drawing.Imaging.ColorMatrix( (, $matrixDiff ) )
    $imgAttrDiff = [System.Drawing.Imaging.ImageAttributes]::new()
    $imgAttrDiff.SetColorMatrix($colorMatrixDiff, [System.Drawing.Imaging.ColorMatrixFlag]::Default, [System.Drawing.Imaging.ColorAdjustType]::Bitmap)
    
    $outImageBytes = [byte[]]$null
    $outImageDiffs = [String]::Empty
    $imgD = [System.Drawing.Bitmap]::new($imgA)
        # copy into imgD
        $gfx = [System.Drawing.Graphics]::FromImage($imgD)
            $gfx.Clear([System.Drawing.Color]::White)
            $gfx.DrawImage($imgA, ( [System.Drawing.Rectangle]::new(0,0,$imgA.Width,$imgA.Height) ), 0,0,$imgA.Width,$imgA.Height, [System.Drawing.GraphicsUnit]::Pixel, $imgAttrBase)
            $gfx.DrawImage($imgB, ( [System.Drawing.Rectangle]::new(0,0,$imgA.Width,$imgA.Height) ), 0,0,$imgA.Width,$imgA.Height, [System.Drawing.GraphicsUnit]::Pixel, $imgAttrBase)

            $pgDiffs = @()
            for($y = 0; $y -lt $imgA.Height; $y++) {
            for($x = 0; $x -lt $imgA.Width;  $x++) {

                $pixA = $imgA.GetPixel($x, $y)
                $pixB = $imgB.GetPixel($x, $y)

                if ($pixA.Name -ne $pixB.Name)
                {
                    $gfx.DrawImage($imgA, ([System.Drawing.Rectangle]::new($x,$y,1,1)), $x,$y,1,1, [System.Drawing.GraphicsUnit]::Pixel, $imgAttrDiff)
                    $gfx.DrawImage($imgB, ([System.Drawing.Rectangle]::new($x,$y,1,1)), $x,$y,1,1, [System.Drawing.GraphicsUnit]::Pixel, $imgAttrDiff)
                    $pgDiffs += "$($x) x $($y)"
                }

                if ($x % 5 -eq 0)
                { Write-Progress -Activity Comparing -Status "Comparing Pixels" -PercentComplete ( ( $imgA.Width * $y + $x ) / ( $imgA.Height * $imgA.Width ) * 100 ) }

            } } # for $x / $y

            if ($pgDiffs.Length -gt 0) {
                $ms = [System.IO.MemoryStream]::new()
                    $gfx.Save() | Out-Null
                    $imgD.Save($ms, [System.Drawing.Imaging.ImageFormat]::Bmp)
                    $outImageBytes = $ms.ToArray()
                $ms.Dispose()
                $outImageDiffs = [String]::Join([Environment]::NewLine, $pgDiffs)
            }
        $gfx.Dispose()
    $imgD.Dispose()

    $outval = New-Object PSObject -Property @{ DiffFileBytes = $outImageBytes ; DiffCoords = $outImageDiffs }
    return $outval
}
function Get-ReportDiff {
    param ($RSServerA, $RSServerB, $report, $SaveFolder, $SaveFilePrefix)
    
    " - get from $RSServerA"
    $pdfDocBytesA = Get-HttpWebResponse -url ( Get-SsrsReportUrl -SsrsServer $RSServerA -Report $report -AsPdf )
    " - get from $RSServerB"
    $pdfDocBytesB = Get-HttpWebResponse -url ( Get-SsrsReportUrl -SsrsServer $RSServerB -Report $report -AsPdf )
    $PdfDocA = ConvertTo-PDF -bytes $pdfDocBytesA
    $PdfDocB = ConvertTo-PDF -bytes $pdfDocBytesB
        
        if ($PdfDocA.PageCount -ne $PdfDocB.PageCount)
        { throw New-Object System.IndexOutOfRangeException("PageCount mismatch : [$($PdfDocA.PageCount)] vs [$($PdfDocB.PageCount)]") }

        for($pageNum = 1; $pageNum -le $PdfDocA.PageCount; $pageNum++) {
            " - Page [$pageNum]"

            $pageIdx = $pageNum - 1
            $pgSizeA = $PdfDocA.PageSizes[$pageIdx]
            $pgSizeB = $PdfDocB.PageSizes[$pageIdx]

            if ($pgSizeA.Width -ne $pgSizeB.Width -or $pgSizeA.Height -ne $pgSizeB.Height)
            { throw New-Object System.IndexOutOfRangeException("PageSize mismatch : page num [$($pageNum)] : [$($pgSizeA.Width)]x[$($pgSizeA.Height)] vs [$($pgSizeB.Width)]x[$($pgSizeB.Height)]") }

            $PdfDocA.GetInformation() | Out-Null
            $PdfDocB.GetInformation() | Out-Null
            $imgA = $PdfDocA.Render($pageIdx, $pgSizeA.Width, $pgSizeA.Height, 300, 300, [PdfiumViewer.PdfRenderFlags]::None)
            $imgB = $PdfDocB.Render($pageIdx, $pgSizeB.Width, $pgSizeB.Height, 300, 300, [PdfiumViewer.PdfRenderFlags]::None)
                $imgDiffs = Get-ImageDiff -ImgA $imgA -ImgB $imgB

                if ($imgDiffs -ne $null -and $imgDiffs.DiffFileBytes -ne $null) {
                    [System.IO.File]::WriteAllBytes("$($SaveFolder)\$($SaveFilePrefix)__$($pageNum.ToString("00")).bmp", $imgDiffs.DiffFileBytes)
                    [System.IO.File]::WriteAllText( "$($SaveFolder)\$($SaveFilePrefix)__$($pageNum.ToString("00")).txt", $imgDiffs.DiffCoords   )
                }
            $imgB.Dispose()
            $imgA.Dispose()
        } # for PageNum
    $pdfDocB.Dispose()
    $pdfDocA.Dispose()
}


# $Error.Clear()
Clear-Host


$RSServerA = $RSServers[$EnvironmentA]
$RSServerB = $RSServers[$EnvironmentB]

for($reportIdx = 0; $reportIdx -lt $Reports.Count; $reportIdx++) {
    $report = $reports[$reportIdx]
    "Report : $($report.ReportPath)"
    $outFileName = "$($reportIdx)_$([System.IO.Path]::GetFileName($report.ReportPath))"
    try { Get-ReportDiff -RSServerA $RSServerA -RSServerB $RSServerB -report $report -SaveFolder $OutputFolder -SaveFilePrefix $outFileName }
    catch {
        Write-Output " - ERR : $( $_.Exception.Message )"
        [System.IO.File]::AppendAllText($LogFile, "ERROR : $($_.Exception.Message)$([Environment]::NewLine)")
    }
}
