$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
Set-Location $RootDir

$Port = if ($env:PORT) { $env:PORT } else { "8787" }
$SqlitePath = if ($env:SQLITE_PATH) { $env:SQLITE_PATH } else { Join-Path $RootDir "apps\api\data\demo.sqlite" }
$Ip = (Get-NetIPAddress -AddressFamily IPv4 |
  Where-Object { $_.IPAddress -notlike "127.*" -and $_.PrefixOrigin -ne "WellKnown" } |
  Select-Object -First 1 -ExpandProperty IPAddress)
if (-not $Ip) { $Ip = "YOUR_WINDOWS_IP" }

Write-Host ""
Write-Host "VR Avatar HeartWatch Demo"
Write-Host "============================================================"
Write-Host "1. SQLite API: http://$Ip`:$Port"
Write-Host "2. iPhone app API URL: http://$Ip`:$Port"
Write-Host "3. Apple Watch: open HeartWatch and tap Start"
Write-Host "4. Quest 3: press right-hand B to start the VR demo"
Write-Host "============================================================"
Write-Host ""

if (-not (Test-Path (Join-Path $RootDir "node_modules"))) {
  Write-Host "Installing Node dependencies..."
  npm install
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $SqlitePath) | Out-Null

$env:HOST = "0.0.0.0"
$env:PORT = $Port
$env:SQLITE_PATH = $SqlitePath

Write-Host "Starting SQLite API..."
$Api = Start-Process -FilePath "npm" -ArgumentList "run", "api:start" -WorkingDirectory $RootDir -PassThru

Start-Sleep -Seconds 2
Write-Host ""
Write-Host "API health check:"
try {
  Invoke-RestMethod "http://127.0.0.1:$Port/api/demo/status" | ConvertTo-Json -Depth 5
} catch {
  Write-Host "API check failed. Keep this window open and check npm output."
}

$WindowsExe = Join-Path $RootDir "release\Windows\VRAvatarHeartWatch.exe"
$UnityProject = Join-Path $RootDir "unity\VRAvatarHeartWatch"

if (Test-Path $WindowsExe) {
  Write-Host "Opening built Windows demo app..."
  Start-Process $WindowsExe
} elseif ($env:UNITY_EXE -and (Test-Path $env:UNITY_EXE)) {
  Write-Host "No built Windows app found. Opening Unity project..."
  Start-Process $env:UNITY_EXE -ArgumentList "-projectPath", $UnityProject
} else {
  Write-Host "No built Windows app found."
  Write-Host "Set UNITY_EXE to Unity.exe or open this project manually:"
  Write-Host $UnityProject
}

Write-Host ""
Write-Host "Keep this window open while presenting. Press Ctrl+C to stop the API."
try {
  Wait-Process -Id $Api.Id
} finally {
  if (-not $Api.HasExited) {
    Stop-Process -Id $Api.Id
  }
}
