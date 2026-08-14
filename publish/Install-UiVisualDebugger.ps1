# UiVisualDebugger One-Click Local Installer
Write-Host "=== UiVisualDebugger Installer ===" -ForegroundColor Cyan

$installDir = Join-Path $env:LOCALAPPDATA "UiVisualDebugger"
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

Write-Host "[1/3] Copying files to $installDir..." -ForegroundColor Yellow
Copy-Item -Path "$PSScriptRoot\*" -Destination $installDir -Recurse -Force -Exclude "Install-UiVisualDebugger.ps1"

# Add to User PATH
$userPath = [Environment]::GetEnvironmentVariable("PATH", "User")
if ($userPath -notlike "*$installDir*") {
    Write-Host "[2/3] Adding $installDir to User PATH..." -ForegroundColor Yellow
    [Environment]::SetEnvironmentVariable("PATH", "$userPath;$installDir", "User")
}

# Create Start Menu Shortcut
$startMenuDir = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\UiVisualDebugger"
if (-not (Test-Path $startMenuDir)) {
    New-Item -ItemType Directory -Path $startMenuDir -Force | Out-Null
}
$shortcutPath = Join-Path $startMenuDir "UiVisualDebugger.lnk"
$wshShell = New-Object -ComObject WScript.Shell
$shortcut = $wshShell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = Join-Path $installDir "UiVisualDebugger.exe"
$shortcut.WorkingDirectory = $installDir
$shortcut.Description = "Real-Time UI Visual Debugger & Inspector"
$shortcut.Save()

Write-Host "[3/3] Installation completed successfully!" -ForegroundColor Green
Write-Host "You can now run 'UiVisualDebugger.exe' from any command prompt or start menu." -ForegroundColor Cyan
