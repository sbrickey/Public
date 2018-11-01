function Get-InverseImage( [System.Drawing.Image]$imgSrc ) {

    # for the base copy of each page into its output (source and dest are both copied at 10%)
    $matrixBase = [float[][]] @( , @( -1 ,  0 ,  0 ,  0 , 0 )
                                 , @(  0 , -1 ,  0 ,  0 , 0 )
                                 , @(  0 ,  0 , -1 ,  0 , 0 )
                                 , @(  0 ,  0 ,  0 ,  1 , 0 )
                                 , @(  1 ,  1 ,  1 ,  0 , 1 )
                               )
    $colorMatrixBase = New-Object System.Drawing.Imaging.ColorMatrix( (, $matrixBase ) )
    $imgAttrBase = [System.Drawing.Imaging.ImageAttributes]::new()
    $imgAttrBase.SetColorMatrix($colorMatrixBase, [System.Drawing.Imaging.ColorMatrixFlag]::Default, [System.Drawing.Imaging.ColorAdjustType]::Bitmap)
    
    $imgOut = [System.Drawing.Bitmap]::new($imgSrc)
        # copy into imgD
        $gfx = [System.Drawing.Graphics]::FromImage($imgOut)
            $gfx.Clear([System.Drawing.Color]::Black)
            $gfx.DrawImage($imgSrc, ( [System.Drawing.Rectangle]::new(0,0,$imgSrc.Width,$imgSrc.Height) ), 0,0,$imgSrc.Width,$imgSrc.Height, [System.Drawing.GraphicsUnit]::Pixel, $imgAttrBase)

            $gfx.Save() | Out-Null
        $gfx.Dispose()

    return $imgOut
}
