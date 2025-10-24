# PowerShell script to install and configure iisnode on Windows Server
# Run this script as Administrator

param(
    [string]$NodeVersion = "18.17.0",
    [string]$SiteName = "PRReviewerBot",
    [string]$SitePath = "C:\inetpub\wwwroot\PRReviewerBot",
    [int]$Port = 80
)

Write-Host "🤖 Installing PR Reviewer Bot on Windows Server IIS" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "This script must be run as Administrator. Exiting..."
    exit 1
}

# Function to download and install software
function Install-Software {
    param($Name, $Url, $Arguments = "/quiet")
    
    Write-Host "Installing $Name..." -ForegroundColor Yellow
    $tempFile = [System.IO.Path]::GetTempFileName() + ".msi"
    
    try {
        Invoke-WebRequest -Uri $Url -OutFile $tempFile -UseBasicParsing
        Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$tempFile`" $Arguments" -Wait -NoNewWindow
        Remove-Item $tempFile -Force
        Write-Host "✅ $Name installed successfully" -ForegroundColor Green
    }
    catch {
        Write-Error "❌ Failed to install $Name: $_"
        exit 1
    }
}

# Step 1: Install Node.js
Write-Host "`n📦 Step 1: Installing Node.js $NodeVersion" -ForegroundColor Cyan
$nodeUrl = "https://nodejs.org/dist/v$NodeVersion/node-v$NodeVersion-x64.msi"
Install-Software -Name "Node.js" -Url $nodeUrl

# Step 2: Install URL Rewrite Module
Write-Host "`n🔄 Step 2: Installing IIS URL Rewrite Module" -ForegroundColor Cyan
$rewriteUrl = "https://download.microsoft.com/download/1/2/8/128E2E22-C1B9-44A4-BE2A-5859ED1D4592/rewrite_amd64_en-US.msi"
Install-Software -Name "URL Rewrite Module" -Url $rewriteUrl

# Step 3: Install iisnode
Write-Host "`n🌐 Step 3: Installing iisnode" -ForegroundColor Cyan
$iisnodeUrl = "https://github.com/Azure/iisnode/releases/download/v0.2.26/iisnode-full-v0.2.26-x64.msi"
Install-Software -Name "iisnode" -Url $iisnodeUrl

# Step 4: Enable IIS Features
Write-Host "`n⚙️ Step 4: Enabling required IIS features" -ForegroundColor Cyan
$features = @(
    "IIS-WebServerRole",
    "IIS-WebServer",
    "IIS-CommonHttpFeatures",
    "IIS-HttpErrors",
    "IIS-HttpLogging",
    "IIS-RequestFiltering",
    "IIS-StaticContent",
    "IIS-DefaultDocument",
    "IIS-DirectoryBrowsing",
    "IIS-ASPNET45",
    "IIS-NetFxExtensibility45",
    "IIS-ISAPIExtensions",
    "IIS-ISAPIFilter"
)

foreach ($feature in $features) {
    Write-Host "Enabling $feature..." -ForegroundColor Yellow
    Enable-WindowsOptionalFeature -Online -FeatureName $feature -All -NoRestart
}

# Step 5: Create application directory
Write-Host "`n📁 Step 5: Creating application directory" -ForegroundColor Cyan
if (!(Test-Path $SitePath)) {
    New-Item -ItemType Directory -Path $SitePath -Force
    Write-Host "✅ Created directory: $SitePath" -ForegroundColor Green
}

# Step 6: Set permissions
Write-Host "`n🔐 Step 6: Setting directory permissions" -ForegroundColor Cyan
$acl = Get-Acl $SitePath
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($accessRule)
Set-Acl -Path $SitePath -AclObject $acl
Write-Host "✅ Permissions set successfully" -ForegroundColor Green

# Step 7: Create IIS site
Write-Host "`n🌐 Step 7: Creating IIS site" -ForegroundColor Cyan
Import-Module WebAdministration

if ($Port -eq 80 -and (Get-Website -Name "Default Web Site" -ErrorAction SilentlyContinue)) {
    Remove-Website -Name "Default Web Site"
    Write-Host "Removed Default Web Site" -ForegroundColor Yellow
}

if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
    Remove-Website -Name $SiteName
    Write-Host "Removed existing site: $SiteName" -ForegroundColor Yellow
}

New-Website -Name $SiteName -Port $Port -PhysicalPath $SitePath
Write-Host "✅ Created IIS site: $SiteName" -ForegroundColor Green

Write-Host "`n🎉 Installation Complete!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Copy your PR Reviewer Bot files to: $SitePath" -ForegroundColor White
Write-Host "2. Create .env file with your API keys" -ForegroundColor White
Write-Host "3. Run deployment script" -ForegroundColor White
Write-Host "4. Configure GitHub webhooks" -ForegroundColor White
