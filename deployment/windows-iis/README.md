# 🪟 Windows Server IIS Deployment Guide

Complete guide to deploy the PR Reviewer Bot on Windows Server with IIS.

## 📋 Prerequisites

- **Windows Server** (2016, 2019, or 2022)
- **IIS** installed and running
- **Administrator access** to the server
- **Internet connection** for downloading dependencies

## 🚀 Quick Installation

### Automated Installation (Recommended)

Run the automated PowerShell script as **Administrator**:

```powershell
# Download and run the installation script
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
.\deployment\windows-iis\install-iisnode.ps1
```

This script will:
- ✅ Install Node.js 18.17.0
- ✅ Install IIS URL Rewrite Module
- ✅ Install iisnode
- ✅ Configure IIS features
- ✅ Create application directory
- ✅ Set proper permissions
- ✅ Create IIS site and app pool

## 🔧 Deployment Steps

### 1. Copy Application Files
```batch
# Copy your PR Reviewer Bot files to the IIS directory
xcopy /E /Y /I "C:\path\to\your\bot\*" "C:\inetpub\wwwroot\PRReviewerBot\"
```

### 2. Configure Environment Variables
Create `.env` file in the application root:
```env
# GitHub Configuration
GITHUB_TOKEN=your_github_personal_access_token
GITHUB_WEBHOOK_SECRET=your_webhook_secret
TARGET_BRANCH=beta

# OpenAI Configuration
OPENAI_API_KEY=your_openai_api_key
OPENAI_MODEL=gpt-4

# Server Configuration
PORT=process.env.PORT
NODE_ENV=production
LOG_LEVEL=info
```

### 3. Deploy Using Batch Script
```batch
# Run the deployment script as Administrator
.\deployment\windows-iis\deploy.bat
```

## 🌐 GitHub Webhook Configuration

Configure your GitHub repository webhooks:

1. Go to **Repository Settings** → **Webhooks**
2. Click **Add webhook**
3. Set **Payload URL**: `http://your-server.com/webhook/github`
4. Set **Content type**: `application/json`
5. Set **Secret**: Your webhook secret from `.env`
6. Select **Pull requests** events
7. Click **Add webhook**

## 🔍 Testing the Deployment

### Health Check
```powershell
# Test the health endpoint
Invoke-WebRequest -Uri "http://localhost/health" -UseBasicParsing
```

### Check Logs
```powershell
# View application logs
Get-Content "C:\inetpub\wwwroot\PRReviewerBot\logs\*.log" -Tail 50
```

## 🎉 Success!

Your PR Reviewer Bot is now running on Windows Server with IIS! 

**Access URLs:**
- **Health Check**: `http://your-server/health`
- **Webhook Endpoint**: `http://your-server/webhook/github`
- **Logs**: `C:\inetpub\wwwroot\PRReviewerBot\logs\`

The bot will now automatically review PRs targeting your `beta` branch! 🚀
