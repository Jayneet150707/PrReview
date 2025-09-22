# 🤖 PR Reviewer Bot

An intelligent automated code review bot that uses OpenAI to provide comprehensive feedback on GitHub pull requests. Perfect for teams looking to maintain code quality and catch issues early in the development process.

## ✨ Features

- 🔍 **Intelligent Code Analysis** - Uses OpenAI GPT-4 to analyze code changes
- 🛡️ **Security Review** - Identifies potential security vulnerabilities
- ⚡ **Performance Analysis** - Spots performance bottlenecks and inefficiencies
- 📋 **Best Practices** - Ensures adherence to coding standards
- 🎯 **Targeted Reviews** - Configurable to review specific branches (e.g., beta)
- 💬 **Contextual Comments** - Provides line-specific feedback and suggestions
- 🚀 **Easy Deployment** - Docker-ready with cloud deployment options
- 📊 **Comprehensive Logging** - Detailed logging and error handling

## 🏗️ Architecture

```
GitHub PR Event → Webhook → Code Analysis → OpenAI Review → GitHub Comment
```

The bot follows a simple but effective workflow:
1. Receives GitHub webhook when PR is opened/updated
2. Analyzes code changes and extracts meaningful diffs
3. Sends code to OpenAI for intelligent review
4. Posts structured feedback as GitHub review comments

## 🚀 Quick Start

### Prerequisites

- Node.js 18+ 
- GitHub Personal Access Token or GitHub App
- OpenAI API Key
- Webhook endpoint (ngrok for local development)

### 1. Clone and Install

```bash
git clone <repository-url>
cd pr-reviewer-bot
npm install
```

### 2. Configuration

Copy the environment template and configure your settings:

```bash
cp .env.example .env
```

Edit `.env` with your configuration:

```env
# GitHub Configuration
GITHUB_TOKEN=your_github_personal_access_token
GITHUB_WEBHOOK_SECRET=your_webhook_secret
TARGET_BRANCH=beta

# OpenAI Configuration
OPENAI_API_KEY=your_openai_api_key
OPENAI_MODEL=gpt-4

# Server Configuration
PORT=3000
NODE_ENV=development
```

### 3. Development

```bash
# Start in development mode
npm run dev

# Build for production
npm run build

# Start production server
npm start
```

### 4. Set Up GitHub Webhook

1. Go to your repository settings → Webhooks
2. Add webhook with URL: `https://your-domain.com/webhook/github`
3. Select "Pull requests" events
4. Set the secret to match your `GITHUB_WEBHOOK_SECRET`

## 🐳 Docker Deployment

### Using Docker Compose

```bash
# Build and start
docker-compose up -d

# View logs
docker-compose logs -f pr-reviewer-bot

# Stop
docker-compose down
```

### Using Docker

```bash
# Build image
docker build -t pr-reviewer-bot .

# Run container
docker run -d \
  --name pr-reviewer-bot \
  -p 3000:3000 \
  --env-file .env \
  pr-reviewer-bot
```

## ☁️ Cloud Deployment

### Azure Container Instances

```bash
# Deploy to Azure
az container create \
  --resource-group myResourceGroup \
  --name pr-reviewer-bot \
  --image your-registry/pr-reviewer-bot:latest \
  --ports 3000 \
  --environment-variables \
    NODE_ENV=production \
    PORT=3000 \
  --secure-environment-variables \
    GITHUB_TOKEN=your_token \
    OPENAI_API_KEY=your_key
```

### AWS ECS / Google Cloud Run

See deployment examples in the `deploy/` directory.

## ⚙️ Configuration Options

| Variable | Description | Default |
|----------|-------------|---------|
| `TARGET_BRANCH` | Branch to review PRs for | `beta` |
| `REVIEW_ENABLED` | Enable/disable reviews | `true` |
| `AUTO_APPROVE_ENABLED` | Auto-approve good PRs | `false` |
| `MAX_FILES_TO_REVIEW` | Max files per PR | `20` |
| `MAX_DIFF_SIZE` | Max total changes | `10000` |
| `OPENAI_MODEL` | OpenAI model to use | `gpt-4` |
| `OPENAI_MAX_TOKENS` | Max tokens per request | `2000` |

## 🧪 Testing

```bash
# Run all tests
npm test

# Run tests in watch mode
npm run test:watch

# Run linter
npm run lint

# Fix linting issues
npm run lint:fix
```

## 📊 Monitoring

The bot includes comprehensive logging and health checks:

- **Health Endpoint**: `GET /health`
- **Structured Logging**: JSON format with Winston
- **Error Tracking**: Detailed error logs with context
- **Rate Limiting**: Built-in rate limiting for APIs

## 🔧 Customization

### Custom Review Prompts

Edit `src/prompts/review-prompts.ts` to customize the AI review behavior:

```typescript
export const CUSTOM_PROMPT = `
Your custom review instructions here...
`;
```

### File Filtering

Modify `src/services/code-analyzer.ts` to change which files are reviewed:

```typescript
const skipPatterns = [
  /^package-lock\.json$/,
  /\.min\.(js|css)$/,
  // Add your patterns here
];
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📝 License

MIT License - see [LICENSE](LICENSE) file for details.

## 🆘 Support

- 📖 [Documentation](docs/)
- 🐛 [Issue Tracker](https://github.com/your-repo/issues)
- 💬 [Discussions](https://github.com/your-repo/discussions)

## 🙏 Acknowledgments

- OpenAI for providing the GPT API
- GitHub for the excellent API and webhook system
- The open-source community for inspiration and tools

---

**Made with ❤️ for better code reviews**

