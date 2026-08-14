# Compila Pausa.exe, lo instala en %LOCALAPPDATA%\Pausa y lo reinicia
$d = $PSScriptRoot
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$dest = "$env:LOCALAPPDATA\Pausa"

$fuentes = @("App.cs","Config.cs","Native.cs","Salud.cs","Icono.cs","OverlayForm.cs",
             "AvisoForm.cs","SettingsForm.cs","ElegirAppForm.cs","AssemblyInfo.cs") |
           ForEach-Object { Join-Path $d $_ }

& $csc /target:winexe /out:"$d\Pausa.exe" /optimize+ /nologo /codepage:65001 `
    /win32icon:"$d\pausa.ico" /win32manifest:"$d\pausa.manifest" `
    /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll `
    $fuentes

if (-not $?) { Write-Host "fallo la compilacion"; exit 1 }

Get-Process Pausa -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1
New-Item -ItemType Directory -Force $dest | Out-Null
Copy-Item "$d\Pausa.exe" "$dest\Pausa.exe" -Force
Start-Process "$dest\Pausa.exe"
Write-Host "Pausa actualizada y corriendo desde $dest"
