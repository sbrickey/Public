$InputFolder    = "C:\PdfFiles\In"
$OutputFolder   = "C:\PdfFiles\Out"

$ThirdPartyDLLs = @( "C:\DLLs\pdfSharp\1.50.4845-RC2a\PdfSharp.dll" )
$ThirdPartyDLLs | % { [void][System.Reflection.Assembly]::LoadFile($_) }

$Files          = [System.IO.Directory]::GetFiles($InputFolder, "*.pdf", [System.IO.SearchOption]::TopDirectoryOnly)
foreach($file in $files) { # $file = $files | select -first 1
    Write-Host "[$( [DateTime]::Now )] File [$( $file )] : Start"

    $outFile    = [System.IO.Path]::Combine($OutputFolder, [System.IO.Path]::GetFileNameWithoutExtension($file))

    # open source PDF for generating output
    $pdfInp     = [PdfSharp.Pdf.IO.PdfReader]::Open($file , [PdfSharp.Pdf.IO.PdfDocumentOpenMode]::Import)
    
    # loop over each page, dump into file
    for($pgIdx  = 0; $pgIdx -lt $pdfInp.PageCount; $pgIdx++) {
        $pdfOut = [PdfSharp.Pdf.PdfDocument]::new()
        $pdfOut.AddPage($pdfInp.Pages[$pgIdx]) | Out-Null
        $pdfOut.Save("$($outFile)_$($pgIdx + 1).pdf")
        $pdfOut.Dispose()
        $pdfOut = $null
    }

    # close source PDF
    $pdfInp.Close()
    $pdfInp.Dispose()
    $pdfInp = $null

    Write-Host "[$( [DateTime]::Now )] File [$( $file )] : Done"
}
