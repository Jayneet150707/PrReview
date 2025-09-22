# 🚀 Deployment Guide

This guide provides step-by-step instructions for deploying the PR Reviewer Bot to various platforms.

## Quick Deployment Options

### 1. Docker (Recommended)

The easiest way to deploy the bot is using Docker:

```bash
# Clone and setup
git clone <repository-url>
cd pr-reviewer-bot
cp .env.example .env
# Edit .env with your configuration

# Build and run
docker-compose up -d

# Check logs
docker-compose logs -f pr-reviewer-bot
```

### 2. Node.js Direct

For development or simple deployments:

```bash
# Install dependencies
npm install

# Build the project
npm run build

# Start the server
npm start
```

## Cloud Deployment

### Azure Container Instances

```bash
# Create resource group
az group create --name pr-reviewer-rg --location eastus

# Deploy container
az container create \
  --resource-group pr-reviewer-rg \
  --name pr-reviewer-bot \
  --image your-registry/pr-reviewer-bot:latest \
  --ports 3000 \
  --environment-variables NODE_ENV=production PORT=3000 \
  --secure-environment-variables \
    GITHUB_TOKEN=your_token \
    OPENAI_API_KEY=your_key \
    GITHUB_WEBHOOK_SECRET=your_secret
```

### AWS ECS

```bash
# Create task definition (see deploy/aws-ecs-task.json)
aws ecs register-task-definition --cli-input-json file://deploy/aws-ecs-task.json

# Create service
aws ecs create-service \
  --cluster your-cluster \
  --service-name pr-reviewer-bot \
  --task-definition pr-reviewer-bot:1 \
  --desired-count 1
```

### Google Cloud Run

```bash
# Build and push image
docker build -t gcr.io/your-project/pr-reviewer-bot .
docker push gcr.io/your-project/pr-reviewer-bot

# Deploy to Cloud Run
gcloud run deploy pr-reviewer-bot \
  --image gcr.io/your-project/pr-reviewer-bot \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --set-env-vars NODE_ENV=production \
  --set-env-vars PORT=8080
```

## Environment Configuration

Make sure to set these environment variables in your deployment:

```env
# Required
GITHUB_TOKEN=your_github_token
OPENAI_API_KEY=your_openai_key
GITHUB_WEBHOOK_SECRET=your_webhook_secret

# Optional
TARGET_BRANCH=beta
OPENAI_MODEL=gpt-4
MAX_FILES_TO_REVIEW=20
```

## Post-Deployment

1. **Configure GitHub Webhook**:
   - URL: `https://your-domain.com/webhook/github`
   - Events: Pull requests
   - Secret: Your webhook secret

2. **Test the Setup**:
   - Create a test PR
   - Check bot logs
   - Verify review comments appear

3. **Monitor**:
   - Health endpoint: `GET /health`
   - Application logs
   - API rate limits

## Troubleshooting

- **Webhook not received**: Check firewall and URL accessibility
- **Authentication errors**: Verify GitHub token permissions
- **OpenAI errors**: Check API key and credits
- **Rate limiting**: Monitor API usage and adjust limits

