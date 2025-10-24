@echo off
REM Deployment script for PR Reviewer Bot on Windows Server IIS
REM Run this script from your project directory as Administrator

echo.
echo 🤖 PR Reviewer Bot - Windows IIS Deployment
echo ==========================================
echo.

REM Configuration
set SITE_NAME=PRReviewerBot
set APP_POOL_NAME=PRReviewerBotAppPool
set SITE_PATH=C:\inetpub\wwwroot\PRReviewerBot
set NODE_PATH=C:\Program Files\nodejs\node.exe
set NPM_PATH=C:\Program Files\nodejs\npm.cmd

REM Check if running as Administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ❌ Error: This script must be run as Administrator
    echo Right-click and select "Run as administrator"
    pause
    exit /b 1
)

echo ⏹️  Stopping application pool...
%windir%\system32\inetsrv\appcmd stop apppool /apppool.name:"%APP_POOL_NAME%" >nul 2>&1

echo 📦 Copying application files...
if not exist "%SITE_PATH%" mkdir "%SITE_PATH%"

REM Copy main application files
copy /Y "server.js" "%SITE_PATH%\" >nul 2>&1
copy /Y "package.json" "%SITE_PATH%\" >nul 2>&1
copy /Y "package-lock.json" "%SITE_PATH%\" >nul 2>&1

REM Copy configuration files
copy /Y "deployment\windows-iis\web.config" "%SITE_PATH%\" >nul 2>&1
copy /Y "deployment\windows-iis\iisnode.yml" "%SITE_PATH%\" >nul 2>&1

REM Copy source code
if exist "src" (
    if exist "%SITE_PATH%\src" rmdir /s /q "%SITE_PATH%\src"
    xcopy /E /I /Y /Q "src" "%SITE_PATH%\src" >nul 2>&1
)

echo ✅ Files copied successfully

echo 📦 Installing dependencies...
cd /d "%SITE_PATH%"
"%NPM_PATH%" install --production --silent
if %errorLevel% neq 0 (
    echo ❌ Error: Failed to install dependencies
    pause
    exit /b 1
)
echo ✅ Dependencies installed

echo 🔐 Setting permissions...
icacls "%SITE_PATH%" /grant "IIS_IUSRS:(OI)(CI)F" /T >nul 2>&1
echo ✅ Permissions set

echo ▶️  Starting application pool...
%windir%\system32\inetsrv\appcmd start apppool /apppool.name:"%APP_POOL_NAME%" >nul 2>&1
echo ✅ Application pool started

echo.
echo 🎉 Deployment Complete!
echo ======================
echo.
echo Health Check: http://localhost/health
echo Webhook URL: http://localhost/webhook/github
echo.
pause
