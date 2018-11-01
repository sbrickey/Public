function Get-AdjustImageContrast( [System.Drawing.Image]$imgSrc , [byte]$contrastLevel ) {

    $imgSrcLock  = $imgSrc.LockBits( ([System.Drawing.Rectangle]::new(0,0,$imgSrc.Width,$imgSrc.Height)) , [System.Drawing.Imaging.ImageLockMode]::ReadOnly , $imgSrc.PixelFormat )

    # using locked bitmap, copy values as byte[]
    $imgSize  = [System.Math]::Abs($imgSrcLock.Stride) * $imgSrc.Height  # stride is num bytes (3 for RGB) per row of pixels
    $imgBytes = [byte[]]::new($imgSize)                                  # image buffer
    [System.Runtime.InteropServices.Marshal]::Copy($imgSrcLock.Scan0, $imgBytes, 0, $imgSize) # copy into buffer

    $imgSrc.UnlockBits($imgSrcLock)

    $contrastMultiplier = [Math]::Pow($contrastLevel, 2)
    for ($i = 0; $i -lt $imgBytes.Count; $i += 4) {
        $r = (((($imgBytes[$i + 0] / 255.0) - 0.5) * $contrastMultiplier) + 0.5) * 255.0;
        $g = (((($imgBytes[$i + 1] / 255.0) - 0.5) * $contrastMultiplier) + 0.5) * 255.0;
        $b = (((($imgBytes[$i + 2] / 255.0) - 0.5) * $contrastMultiplier) + 0.5) * 255.0;
        
        if ($r -lt 0) { $r = 0 } elseif ($r -gt 255) { $r = 255 }
        if ($g -lt 0) { $g = 0 } elseif ($g -gt 255) { $g = 255 }
        if ($b -lt 0) { $b = 0 } elseif ($b -gt 255) { $b = 255 }
        $imgBytes[$i + 0] = $r
        $imgBytes[$i + 1] = $g
        $imgBytes[$i + 2] = $b
    }

    $imgOut = [System.Drawing.Bitmap]::new($imgSrc)
    $imgOutLock  = $imgOut.LockBits( ([System.Drawing.Rectangle]::new(0,0,$imgOut.Width,$imgOut.Height)) , [System.Drawing.Imaging.ImageLockMode]::WriteOnly , $imgOut.PixelFormat )
    [System.Runtime.InteropServices.Marshal]::Copy($imgBytes, 0, $imgOutLock.Scan0, $imgSize) # copy into buffer
    $imgOut.UnlockBits($imgOutLock)

    return $imgOut
}
