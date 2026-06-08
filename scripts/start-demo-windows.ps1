$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
Set-Location $RootDir

$Port = if ($env:PORT) { $env:PORT } else { "8787" }
$SqlitePath = if ($env:SQLITE_PATH) { $env:SQLITE_PATH } else { Join-Path $RootDir "apps\api\data\demo.sqlite" }
$LogDir = Join-Path $RootDir "logs"
$RunStamp = Get-Date -Format "yyyyMMdd-HHmmss"
$RunLog = Join-Path $LogDir "demo-run-$RunStamp.log"
$ApiLog = Join-Path $LogDir "api-output-$RunStamp.log"
$ApiErrorLog = Join-Path $LogDir "api-error-$RunStamp.log"

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
Start-Transcript -Path $RunLog -Append | Out-Null

function Write-Step {
  param([string]$Message)
  Write-Host ""
  Write-Host "[$(Get-Date -Format "HH:mm:ss")] $Message"
}

function Get-LanAddresses {
  $configs = Get-NetIPConfiguration |
    Where-Object {
      $_.NetAdapter.Status -eq "Up" -and
      $_.IPv4Address -and
      $_.IPv4DefaultGateway
    }

  $addresses = @()
  foreach ($config in $configs) {
    foreach ($address in $config.IPv4Address) {
      if ($address.IPAddress -notlike "127.*" -and
          $address.IPAddress -notlike "169.254.*" -and
          $address.IPAddress -notlike "172.16.*" -and
          $address.IPAddress -notlike "172.17.*" -and
          $address.IPAddress -notlike "172.18.*" -and
          $address.IPAddress -notlike "172.19.*") {
        $addresses += [PSCustomObject]@{
          IP = $address.IPAddress
          Interface = $config.InterfaceAlias
        }
      }
    }
  }

  if ($addresses.Count -eq 0) {
    $fallback = Get-NetIPAddress -AddressFamily IPv4 |
      Where-Object { $_.IPAddress -notlike "127.*" -and $_.IPAddress -notlike "169.254.*" } |
      Select-Object -First 1
    if ($fallback) {
      $addresses += [PSCustomObject]@{
        IP = $fallback.IPAddress
        Interface = $fallback.InterfaceAlias
      }
    }
  }

  return $addresses
}

function Enable-DemoFirewallRule {
  param([string]$Port)

  $principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
  $isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
  $ruleName = "VR Avatar HeartWatch API $Port"

  if (-not $isAdmin) {
    Write-Host "Firewall note: run PowerShell as Administrator once if iPhone cannot reach the API."
    Write-Host "Admin command:"
    Write-Host "  New-NetFirewallRule -DisplayName '$ruleName' -Direction Inbound -Action Allow -Protocol TCP -LocalPort $Port"
    return
  }

  $existing = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
  if (-not $existing) {
    New-NetFirewallRule -DisplayName $ruleName -Direction Inbound -Action Allow -Protocol TCP -LocalPort $Port | Out-Null
    Write-Host "Firewall rule added: $ruleName"
  } else {
    Write-Host "Firewall rule already exists: $ruleName"
  }
}

try {
  $LanAddresses = @(Get-LanAddresses)
  $PrimaryIp = if ($LanAddresses.Count -gt 0) { $LanAddresses[0].IP } else { "YOUR_WINDOWS_IP" }

  Write-Host ""
  Write-Host "VR Avatar HeartWatch Demo"
  Write-Host "============================================================"
  Write-Host "Run log: $RunLog"
  Write-Host "API log: $ApiLog"
  Write-Host "SQLite path: $SqlitePath"
  Write-Host "API bind address: 0.0.0.0:$Port"
  Write-Host ""
  Write-Host "Use this URL first in the iPhone app Settings tab:"
  Write-Host "  http://$PrimaryIp`:$Port"
  Write-Host ""
  Write-Host "All detected LAN candidates:"
  if ($LanAddresses.Count -eq 0) {
    Write-Host "  No LAN IP was detected. Connect Windows and iPhone to the same Wi-Fi."
  } else {
    foreach ($address in $LanAddresses) {
      Write-Host "  http://$($address.IP):$Port  ($($address.Interface))"
    }
  }
  Write-Host ""
  Write-Host "Demo order:"
  Write-Host "1. Enter the printed API URL in iPhone Settings."
  Write-Host "2. Open HeartWatch on Apple Watch and tap Start."
  Write-Host "3. In VR, press Quest right-hand B to start the demo."
  Write-Host "4. Watch this logs folder while presenting: $LogDir"
  Write-Host "============================================================"
  Write-Host ""

  Enable-DemoFirewallRule -Port $Port

  if (-not (Test-Path (Join-Path $RootDir "node_modules"))) {
    Write-Step "Installing Node dependencies"
    npm install
  }

  New-Item -ItemType Directory -Force -Path (Split-Path -Parent $SqlitePath) | Out-Null

  $env:HOST = "0.0.0.0"
  $env:PORT = $Port
  $env:SQLITE_PATH = $SqlitePath

  Write-Step "Starting SQLite API"
  $npmCommand = (Get-Command npm.cmd -ErrorAction SilentlyContinue)
  if (-not $npmCommand) {
    $npmCommand = Get-Command npm -ErrorAction Stop
  }

  $Api = Start-Process `
    -FilePath $npmCommand.Source `
    -ArgumentList "run", "api:start" `
    -WorkingDirectory $RootDir `
    -RedirectStandardOutput $ApiLog `
    -RedirectStandardError $ApiErrorLog `
    -PassThru

  Start-Sleep -Seconds 3
  Write-Host "API process id: $($Api.Id)"

  Write-Step "Local API health check"
  try {
    Invoke-RestMethod "http://127.0.0.1:$Port/api/demo/status" | ConvertTo-Json -Depth 5
  } catch {
    Write-Host "Local API check failed. Read API logs:"
    Write-Host "  $ApiLog"
    Write-Host "  $ApiErrorLog"
  }

  Write-Step "LAN URL check from this Windows computer"
  foreach ($address in $LanAddresses) {
    $url = "http://$($address.IP):$Port/api/health"
    try {
      Invoke-RestMethod $url -TimeoutSec 3 | ConvertTo-Json -Depth 3
      Write-Host "Reachable candidate: http://$($address.IP):$Port"
    } catch {
      Write-Host "Could not reach $url from Windows. If this is the Wi-Fi IP, check Windows Firewall."
    }
  }

  Write-Host ""
  Write-Host "iPhone quick test:"
  Write-Host "1. Open Safari on iPhone."
  Write-Host "2. Visit http://$PrimaryIp`:$Port/api/health"
  Write-Host "3. It should show JSON with ok=true. If Safari cannot open it, Windows Firewall or Wi-Fi isolation is blocking the phone."

  $WindowsExe = Join-Path $RootDir "release\Windows\VRAvatarHeartWatch.exe"
  if (Test-Path $WindowsExe) {
    Write-Step "Opening built Windows demo app"
    Start-Process $WindowsExe
  } else {
    Write-Host "No built Windows app found at:"
    Write-Host $WindowsExe
  }

  Write-Host ""
  Write-Host "Keep this window open while presenting. Press Ctrl+C to stop the API."
  Write-Host "Logs are being written to:"
  Write-Host "  $RunLog"
  Write-Host "  $ApiLog"
  Wait-Process -Id $Api.Id
} finally {
  if ($Api -and -not $Api.HasExited) {
    Stop-Process -Id $Api.Id
  }
  Stop-Transcript | Out-Null
}
