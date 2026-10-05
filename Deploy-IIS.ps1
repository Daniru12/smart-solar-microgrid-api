# ==============================================================================
# Smart Solar Microgrid API - Automated IIS Deployment Script
# ==============================================================================
# This script configures and deploys the ASP.NET Core Web API to Windows IIS:
# 1. Verifies / elevates to Administrator privileges
# 2. Ensures IIS Windows Features are enabled
# 3. Verifies ASP.NET Core Hosting Bundle (AspNetCoreModuleV2)
# 4. Publishes SmartSolarMicrogrid.API in Release mode to C:\inetpub\wwwroot\SolarMicrogridApi
# 5. Configures NTFS directory and logs permissions for IIS worker identities
# 6. Sets up the IIS Application Pool (No Managed Code) and Site (Port 5000)
# 7. Configures Windows Defender Firewall inbound rule
# 8. Detects local LAN/Wi-Fi IPv4 address and verifies HTTP endpoints
# ==============================================================================

# Step 0: Ensure script is running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "[!] Administrator privileges required. Requesting elevation..." -ForegroundColor Yellow
    Start-Process powershell.exe -Verb RunAs -ArgumentList ("-NoExit", "-ExecutionPolicy", "Bypass", "-File", "`"$PSCommandPath`"")
    Exit
}

$ErrorActionPreference = "Stop"

Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host "  Solar Microgrid API - IIS Automated Deployment Setup " -ForegroundColor Cyan
Write-Host "=======================================================" -ForegroundColor Cyan

# Configuration parameters
$siteName = "SolarMicrogridApi"
$poolName = "SolarMicrogridAppPool"
$port = 5000
$publishPath = "C:\inetpub\wwwroot\SolarMicrogridApi"
$logsPath = Join-Path $publishPath "logs"
$scriptDir = $PSScriptRoot

# Locate the .csproj file
$projectFile = Join-Path $scriptDir "SmartSolarMicrogrid.API.csproj"
if (-not (Test-Path $projectFile)) {
    # If run from workspace root
    $projectFile = Join-Path $scriptDir "smart-solar-microgrid-api\SmartSolarMicrogrid.API.csproj"
}
if (-not (Test-Path $projectFile)) {
    Write-Error "Could not locate SmartSolarMicrogrid.API.csproj. Please run this script from the project folder."
    Exit 1
}

# ------------------------------------------------------------------------------
# Step 1: Enable IIS Features if not already installed
# ------------------------------------------------------------------------------
Write-Host "`n[1/7] Checking Windows IIS Features..." -ForegroundColor Yellow
$iisFeatures = @(
    "IIS-WebServerRole",
    "IIS-WebServer",
    "IIS-CommonHttpFeatures",
    "IIS-HttpErrors",
    "IIS-HttpRedirect",
    "IIS-ApplicationDevelopment",
    "IIS-NetFxExtensibility45",
    "IIS-ISAPIExtensions",
    "IIS-ISAPIFilter",
    "IIS-ASPNET45",
    "IIS-WebSockets"
)

$featuresToInstall = @()
foreach ($feature in $iisFeatures) {
    $state = (Get-WindowsOptionalFeature -Online -FeatureName $feature -ErrorAction SilentlyContinue).State
    if ($state -ne "Enabled") {
        $featuresToInstall += $feature
    }
}

if ($featuresToInstall.Count -gt 0) {
    Write-Host "Installing missing IIS features: $($featuresToInstall -join ', ')..." -ForegroundColor Cyan
    Enable-WindowsOptionalFeature -Online -FeatureName $featuresToInstall -All -NoRestart | Out-Null
    Write-Host "IIS features enabled successfully." -ForegroundColor Green
} else {
    Write-Host "All required IIS features are already enabled." -ForegroundColor Green
}

# ------------------------------------------------------------------------------
# Step 2: Verify ASP.NET Core Module V2 (ANCM)
# ------------------------------------------------------------------------------
Write-Host "`n[2/7] Checking ASP.NET Core Module V2 (ANCM)..." -ForegroundColor Yellow
$ancmDllPath = "C:\Program Files\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll"
$ancmDllPathX86 = "C:\Program Files (x86)\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll"

if (-not (Test-Path $ancmDllPath) -and -not (Test-Path $ancmDllPathX86)) {
    Write-Host "[!] AspNetCoreModuleV2 was not detected in IIS directory." -ForegroundColor Red
    Write-Host "    You must install the .NET Hosting Bundle for your .NET version (e.g. .NET 10 / .NET 8)." -ForegroundColor Yellow
    Write-Host "    Download link: https://dotnet.microsoft.com/download/dotnet" -ForegroundColor Cyan
    Write-Host "    After installing the Hosting Bundle, restart IIS using 'iisreset' and re-run this script." -ForegroundColor Yellow
    $prompt = Read-Host "Would you like to open the download page now? (Y/N)"
    if ($prompt -match "^[yY]") {
        Start-Process "https://dotnet.microsoft.com/download/dotnet"
    }
} else {
    Write-Host "AspNetCoreModuleV2 detected successfully." -ForegroundColor Green
}

# ------------------------------------------------------------------------------
# Step 3: Publish ASP.NET Core Web API in Release Mode
# ------------------------------------------------------------------------------
Write-Host "`n[3/7] Publishing Web API in Release mode to '$publishPath'..." -ForegroundColor Yellow
if (-not (Test-Path $publishPath)) {
    New-Item -ItemType Directory -Path $publishPath -Force | Out-Null
}

dotnet publish "$projectFile" -c Release -o "$publishPath"
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed. Please inspect build errors above."
    Exit 1
}
Write-Host "Project published successfully to $publishPath." -ForegroundColor Green

# ------------------------------------------------------------------------------
# Step 4: Configure File & Folder Permissions
# ------------------------------------------------------------------------------
Write-Host "`n[4/7] Configuring NTFS File Permissions for IIS..." -ForegroundColor Yellow
if (-not (Test-Path $logsPath)) {
    New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
}

# Grant Read & Execute on root publish directory
icacls "$publishPath" /grant "IIS_IUSRS:(OI)(CI)RX" /T | Out-Null
icacls "$publishPath" /grant "IUSR:(OI)(CI)RX" /T | Out-Null

# Grant Modify permissions on logs folder for stdout logging
icacls "$logsPath" /grant "IIS_IUSRS:(OI)(CI)M" /T | Out-Null
icacls "$logsPath" /grant "IUSR:(OI)(CI)M" /T | Out-Null
Write-Host "Permissions configured: IIS_IUSRS and IUSR have Read & Execute on app folder, and Modify on logs folder." -ForegroundColor Green

# ------------------------------------------------------------------------------
# Step 5: Configure IIS Application Pool & Web Site
# ------------------------------------------------------------------------------
Write-Host "`n[5/7] Configuring IIS AppPool & Site..." -ForegroundColor Yellow
Import-Module WebAdministration

# 1. Create or configure AppPool
if (!(Test-Path "IIS:\AppPools\$poolName")) {
    Write-Host "Creating AppPool '$poolName'..." -ForegroundColor Cyan
    New-Item "IIS:\AppPools\$poolName" | Out-Null
}
# ASP.NET Core Module handles runtime, so CLR version must be empty string
Set-ItemProperty "IIS:\AppPools\$poolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\$poolName" -Name "managedPipelineMode" -Value 0 # Integrated
Set-ItemProperty "IIS:\AppPools\$poolName" -Name "processModel.identityType" -Value 2 # ApplicationPoolIdentity

# 2. Stop Default Web Site if it is bound to port 5000 (rare, usually 80)
$conflictingBinding = Get-WebBinding -Port $port -ErrorAction SilentlyContinue | Where-Object { $_.ItemXPath -notlike "*$siteName*" }
if ($conflictingBinding) {
    Write-Warning "Another site is using port $port. Stopping conflicting site bindings..."
}

# 3. Create or Recreate Website
if (Test-Path "IIS:\Sites\$siteName") {
    Write-Host "Recreating IIS Site '$siteName'..." -ForegroundColor Cyan
    Stop-WebSite -Name $siteName -ErrorAction SilentlyContinue
    Remove-WebSite -Name $siteName
}

New-WebSite -Name $siteName `
    -Port $port `
    -IPAddress "*" `
    -PhysicalPath $publishPath `
    -ApplicationPool $poolName | Out-Null

Start-WebSite -Name $siteName
Write-Host "IIS Site '$siteName' started on port $port!" -ForegroundColor Green

# ------------------------------------------------------------------------------
# Step 6: Configure Windows Defender Firewall (Inbound Port 5000)
# ------------------------------------------------------------------------------
Write-Host "`n[6/7] Configuring Windows Defender Firewall..." -ForegroundColor Yellow
$firewallRuleName = "ASP.NET Core IIS Port $port"
$existingRule = Get-NetFirewallRule -DisplayName $firewallRuleName -ErrorAction SilentlyContinue
if (-not $existingRule) {
    New-NetFirewallRule `
        -DisplayName $firewallRuleName `
        -Direction Inbound `
        -LocalPort $port `
        -Protocol TCP `
        -Action Allow `
        -Profile Any | Out-Null
    Write-Host "Inbound firewall rule created for TCP port $port." -ForegroundColor Green
} else {
    Write-Host "Firewall rule '$firewallRuleName' already exists." -ForegroundColor Green
}

# ------------------------------------------------------------------------------
# Step 7: Detect LAN IP and Verify Deployment
# ------------------------------------------------------------------------------
Write-Host "`n[7/7] Verifying Deployment..." -ForegroundColor Yellow

# Detect local LAN IPv4 address
$ipCandidates = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
    Where-Object { 
        $_.IPAddress -notlike "127.*" -and 
        $_.IPAddress -notlike "169.254.*" -and 
        $_.InterfaceAlias -notlike "*Loopback*" -and 
        $_.InterfaceAlias -notlike "*vEthernet*"
    }

# Prefer active Wi-Fi or Ethernet
$primaryIp = ($ipCandidates | Where-Object { $_.InterfaceAlias -like "*Wi-Fi*" -or $_.InterfaceAlias -like "*Ethernet*" } | Select-Object -First 1).IPAddress
if (-not $primaryIp) {
    $primaryIp = ($ipCandidates | Select-Object -First 1).IPAddress
}
if (-not $primaryIp) {
    $primaryIp = "127.0.0.1"
}

Write-Host "`nTesting local HTTP endpoint (http://localhost:$port/api/stations)..." -ForegroundColor Cyan
Start-Sleep -Seconds 2

try {
    $response = Invoke-RestMethod -Uri "http://localhost:$port/api/stations" -Method Get -TimeoutSec 10 -ErrorAction Stop
    Write-Host "Endpoint response SUCCESS: API is running and responded!" -ForegroundColor Green
} catch {
    Write-Host "[*] First request response: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "    If you encounter HTTP 500.30/502.5, review the logs in: $logsPath" -ForegroundColor DarkGray
}

Write-Host "`n=======================================================" -ForegroundColor Green
Write-Host "  IIS HOSTING IS READY! ACCESS URLS:                  " -ForegroundColor Green
Write-Host "=======================================================" -ForegroundColor Green
Write-Host " Local PC:" -ForegroundColor White
Write-Host "   -> http://localhost:$port/api/stations" -ForegroundColor Cyan
Write-Host "   -> http://localhost:$port/openapi/v1.json" -ForegroundColor Cyan
Write-Host "`n Mobile Phone / Devices on Same Wi-Fi:" -ForegroundColor White
Write-Host "   -> http://$($primaryIp):$port/api/stations" -ForegroundColor Green
Write-Host "   -> http://$($primaryIp):$port/openapi/v1.json" -ForegroundColor Green
Write-Host "=======================================================`n" -ForegroundColor Green
