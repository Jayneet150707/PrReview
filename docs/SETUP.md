# 🚀 Setup Guide

This guide will walk you through setting up the PR Reviewer Bot for your GitHub repository.

## Prerequisites

Before you begin, make sure you have:

- ✅ Node.js 18+ installed
- ✅ A GitHub repository where you want to enable automated reviews
- ✅ GitHub Personal Access Token or GitHub App credentials
- ✅ OpenAI API key
- ✅ A server or cloud platform to host the bot

## Step 1: GitHub Configuration

### Option A: Personal Access Token (Recommended for small teams)

1. Go to GitHub Settings → Developer settings → Personal access tokens
2. Generate a new token with these permissions:
   - `repo` (Full control of private repositories)
   - `pull_requests:write` (Write access to pull requests)
   - `contents:read` (Read access to repository contents)

### Option B: GitHub App (Recommended for organizations)

1. Go to GitHub Settings → Developer settings → GitHub Apps
2. Create a new GitHub App with these permissions:
   - Repository permissions:
     - Contents: Read
     - Pull requests: Write
     - Metadata: Read
   - Subscribe to events:
     - Pull request

## Step 2: OpenAI Configuration

1. Sign up for OpenAI API access at https://platform.openai.com/
2. Generate an API key from the API keys section
3. Ensure you have sufficient credits/quota for your expected usage

## Step 3: Environment Setup

1. Clone the repository:
   ```bash
   git clone <your-repo-url>
   cd pr-reviewer-bot
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Copy the environment template:
   ```bash
   cp .env.example .env
   ```

4. Configure your environment variables:
   ```env
   # GitHub Configuration
   GITHUB_TOKEN=ghp_your_personal_access_token_here
   GITHUB_WEBHOOK_SECRET=your_secure_random_string_here
   
   # OpenAI Configuration
   OPENAI_API_KEY=sk-your_openai_api_key_here
   OPENAI_MODEL=gpt-4
   
   # Review Configuration
   TARGET_BRANCH=beta
   REVIEW_ENABLED=true
   AUTO_APPROVE_ENABLED=false
   ```

## Step 4: Webhook Configuration

### For Local Development (using ngrok)

1. Install ngrok: https://ngrok.com/download
2. Start your bot locally:
   ```bash
   npm run dev
   ```
3. In another terminal, expose your local server:
   ```bash
   ngrok http 3000
   ```
4. Copy the HTTPS URL (e.g., `https://abc123.ngrok.io`)

### For Production Deployment

Deploy your bot to your chosen platform first, then use your production URL.

### Configure GitHub Webhook

1. Go to your repository → Settings → Webhooks
2. Click "Add webhook"
3. Configure:
   - **Payload URL**: `https://your-domain.com/webhook/github`
   - **Content type**: `application/json`
   - **Secret**: Use the same value as `GITHUB_WEBHOOK_SECRET`
   - **Events**: Select "Let me select individual events" and check:
     - ✅ Pull requests
   - **Active**: ✅ Checked

## Step 5: Test the Setup

1. Create a test pull request targeting your configured branch (e.g., `beta`)
2. Check the bot logs for webhook reception
3. Verify that the bot posts a review comment on the PR

### Troubleshooting

**Webhook not received:**
- Check that your server is accessible from the internet
- Verify the webhook URL is correct
- Check GitHub webhook delivery logs

**Authentication errors:**
- Verify your GitHub token has the correct permissions
- Ensure the token hasn't expired
- Check that the repository is accessible with the token

**OpenAI errors:**
- Verify your API key is correct
- Check your OpenAI account has sufficient credits
- Ensure you have access to the specified model (e.g., GPT-4)

## Step 6: Customization

### Review Configuration

Adjust these settings in your `.env` file:

```env
# Only review PRs targeting this branch
TARGET_BRANCH=beta

# Maximum number of files to review per PR
MAX_FILES_TO_REVIEW=20

# Maximum total changes (additions + deletions) to review
MAX_DIFF_SIZE=10000

# Enable auto-approval for high-quality PRs
AUTO_APPROVE_ENABLED=false
```

### Custom Review Prompts

Edit `src/prompts/review-prompts.ts` to customize the AI behavior:

```typescript
export const CUSTOM_SYSTEM_PROMPT = `
You are a senior software engineer reviewing code for a fintech company.
Focus especially on:
- Security vulnerabilities
- Performance implications
- Compliance with financial regulations
- Error handling and edge cases
`;
```

## Step 7: Monitoring and Maintenance

### Health Checks

The bot provides a health endpoint at `/health`:

```bash
curl https://your-domain.com/health
```

### Logs

Monitor the application logs for:
- Webhook processing
- OpenAI API calls
- GitHub API interactions
- Errors and warnings

### Rate Limiting

The bot includes built-in rate limiting for:
- OpenAI API: 60 requests per minute (configurable)
- GitHub API: 5000 requests per hour (configurable)

## Next Steps

- 📖 Read the [Configuration Guide](CONFIGURATION.md) for advanced settings
- 🚀 Check out [Deployment Options](../deploy/) for production hosting
- 🧪 Run the test suite with `npm test`
- 📊 Set up monitoring and alerting for production use

## Support

If you encounter issues:

1. Check the [Troubleshooting Guide](TROUBLESHOOTING.md)
2. Review the application logs
3. Test webhook delivery in GitHub settings
4. Verify API credentials and permissions

For additional help, please open an issue in the repository.

