# Genera pausa.ico (PNG embebido, 256x256) con el mismo ojo del icono de la bandeja
Add-Type -AssemblyName System.Drawing
$s = 256
$bmp = New-Object System.Drawing.Bitmap $s, $s
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.Clear([System.Drawing.Color]::Transparent)

$acento = [System.Drawing.Color]::FromArgb(56, 189, 248)
$k = $s / 64.0

$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$path.AddBezier(4*$k, 32*$k, 22*$k, 8*$k, 42*$k, 8*$k, 60*$k, 32*$k)
$path.AddBezier(60*$k, 32*$k, 42*$k, 56*$k, 22*$k, 56*$k, 4*$k, 32*$k)
$pen = New-Object System.Drawing.Pen $acento, (5*$k)
$pen.LineJoin = 'Round'
$g.DrawPath($pen, $path)

$br = New-Object System.Drawing.SolidBrush $acento
$g.FillEllipse($br, 24*$k, 22*$k, 20*$k, 20*$k)
$br2 = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(12, 17, 30))
$g.FillEllipse($br2, 30*$k, 27*$k, 8*$k, 8*$k)
$g.Dispose()

$ms = New-Object System.IO.MemoryStream
$bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
$png = $ms.ToArray()
$bmp.Dispose()

$out = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter $out
$w.Write([UInt16]0); $w.Write([UInt16]1); $w.Write([UInt16]1)   # ICONDIR
$w.Write([Byte]0); $w.Write([Byte]0)                            # 256x256
$w.Write([Byte]0); $w.Write([Byte]0)
$w.Write([UInt16]1); $w.Write([UInt16]32)
$w.Write([UInt32]$png.Length); $w.Write([UInt32]22)
$w.Write($png)
$w.Flush()
[System.IO.File]::WriteAllBytes("$PSScriptRoot\pausa.ico", $out.ToArray())
$w.Dispose()
"pausa.ico generado"
