function Get-AdjustImageBrightness( [System.Drawing.Image]$imgSrc , [Int16]$adjustmentLevel ) {

    $imgLock  = $imgSrc.LockBits( ([System.Drawing.Rectangle]::new(0,0,$imgSrc.Width,$imgSrc.Height)) , [System.Drawing.Imaging.ImageLockMode]::ReadOnly , $imgSrc.PixelFormat )

    # using locked bitmap, copy values as byte[]
    $imgSize  = [System.Math]::Abs($imgLock.Stride) * $imgSrc.Height   # stride is num bytes (3 for RGB) per row of pixels
    $imgBytes = [byte[]]::new($imgSize)                                # image buffer
    [System.Runtime.InteropServices.Marshal]::Copy($imgLock.Scan0, $imgBytes, 0, $imgSize) # copy into buffer

    $imgSrc.UnlockBits($imgLock)

    if ( $adjustmentLevel -lt -255 ) { $adjustmentLevel = -255 }
    if ( $adjustmentLevel -gt  255 ) { $adjustmentLevel =  255 }

    for ($i = 0; $i -lt $imgBytes.Count; $i += 4) {
       #$r = $imgBytes[$i + 0] + ( ( 255.0 - $imgBytes[$i + 0] ) * ( $adjustmentLevel / 255.0 ) ) # 100 + ( 150 * ( 128 / 255 ) )
       #$g = $imgBytes[$i + 1] + ( ( 255.0 - $imgBytes[$i + 1] ) * ( $adjustmentLevel / 255.0 ) )
       #$b = $imgBytes[$i + 2] + ( ( 255.0 - $imgBytes[$i + 2] ) * ( $adjustmentLevel / 255.0 ) )
        $r = $imgBytes[$i + 0] + $adjustmentLevel
        $g = $imgBytes[$i + 1] + $adjustmentLevel
        $b = $imgBytes[$i + 2] + $adjustmentLevel
        
        if ($r -lt 0) { $r = 0 } elseif ($r -gt 255) { $r = 255 }
        if ($g -lt 0) { $g = 0 } elseif ($g -gt 255) { $g = 255 }
        if ($b -lt 0) { $b = 0 } elseif ($b -gt 255) { $b = 255 }
        $imgBytes[$i + 0] = $r
        $imgBytes[$i + 1] = $g
        $imgBytes[$i + 2] = $b
    }

    $imgOut = [System.Drawing.Bitmap]::new($imgSrc)
    $imgLock  = $imgOut.LockBits( ([System.Drawing.Rectangle]::new(0,0,$imgOut.Width,$imgOut.Height)) , [System.Drawing.Imaging.ImageLockMode]::WriteOnly , $imgOut.PixelFormat )
    [System.Runtime.InteropServices.Marshal]::Copy($imgBytes, 0, $imgLock.Scan0, $imgSize) # copy into buffer
    $imgOut.UnlockBits($imgLock)

    return $imgOut
}
