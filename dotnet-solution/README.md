# PR Reviewer Bot - .NET Core Solution

## 🚫 COMMENT-ONLY MODE - NEVER MERGES PRs

A production-ready automated Pull Request reviewer bot built with .NET Core 8.0 that uses OpenAI to analyze code changes and post intelligent review comments on GitHub PRs.

**🔒 CRITICAL SAFETY FEATURE: This bot ONLY posts review comments and will NEVER merge, approve, or modify your PRs. Manual human review and approval are always required.**

## 🚀 Features

- **Automated Code Review**: Uses OpenAI GPT-4 to analyze code changes and provide intelligent feedback
- **GitHub Integration**: Seamlessly integrates with GitHub webhooks for real-time PR processing
- **Multi-Language Support**: Reviews code in C#, JavaScript, TypeScript, Python, Java, and many more languages
- **Security & Performance Focus**: Identifies security vulnerabilities, performance issues, and code quality problems
- **Comment-Only Mode**: Posts review comments without auto-merging (safe for production use)
- **Enterprise Architecture**: Clean architecture with proper separation of concerns
- **Health Monitoring**: Built-in health checks for GitHub and OpenAI APIs
- **Comprehensive Logging**: Structured logging with Serilog
- **Configurable Limits**: Prevents review of overly large PRs to maintain quality

## 🏗️ Architecture

```
PRReviewerBot.sln
├── src/
│   ├── PRReviewerBot.Api/           # ASP.NET Core Web API
│   ├── PRReviewerBot.Core/          # Domain models and interfaces
│   └── PRReviewerBot.Infrastructure/ # Implementation services
└── tests/
    └── PRReviewerBot.Tests/         # Unit and integration tests
```

### Key Components

- **WebhookController**: Handles GitHub webhook events
- **PullRequestReviewService**: Orchestrates the review process
- **OpenAICodeAnalysisService**: AI-powered code analysis
- **Health Checks**: Monitor GitHub and OpenAI API connectivity
- **Middleware**: Exception handling and request logging

## 🛠️ Setup & Configuration

### Prerequisites

- .NET 8.0 SDK
- GitHub Personal Access Token with repo permissions
- OpenAI API Key

### Configuration

Update `appsettings.json` with your credentials:

```json
{
  "GitHub": {
    "Token": "your-github-token",
    "WebhookSecret": "your-webhook-secret",
    "TargetBranch": "beta"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Model": "gpt-4"
  },
  "Review": {
    "MaxFilesToReview": 20,
    "MaxChangesToReview": 1000
  }
}
```

### Running the Application

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the API
cd src/PRReviewerBot.Api
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger UI at the root.

### GitHub Webhook Setup

1. Go to your repository settings → Webhooks
2. Add a new webhook with:
   - **Payload URL**: `https://your-domain.com/api/v1/webhook/github`
   - **Content type**: `application/json`
   - **Secret**: Your webhook secret from configuration
   - **Events**: Select "Pull requests"

## 🔧 Key Features

### Intelligent Code Analysis

The bot analyzes code changes and provides feedback on:

- **Security Issues**: SQL injection, XSS vulnerabilities, insecure configurations
- **Performance Problems**: Inefficient algorithms, memory leaks, blocking operations
- **Code Quality**: Naming conventions, code complexity, best practices
- **Bug Detection**: Null reference issues, logic errors, edge cases

### Review Categories

Comments are categorized and prioritized:

- 🔒 **Security** (Critical/Error/Warning/Info)
- ⚡ **Performance** (Critical/Error/Warning/Info)
- 🐛 **Bug** (Critical/Error/Warning/Info)
- 📝 **Code Quality** (Critical/Error/Warning/Info)

### Smart Filtering

- Only reviews source code files (skips binaries, dependencies, generated files)
- Configurable limits to prevent reviewing overly large PRs
- Language-specific analysis for better accuracy

## 📊 Monitoring & Health Checks

Access health information at `/health`:

```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "github",
      "status": "Healthy",
      "duration": "00:00:00.1234567"
    },
    {
      "name": "openai",
      "status": "Healthy",
      "duration": "00:00:00.2345678"
    }
  ]
}
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🚀 Deployment

### IIS Deployment

1. Publish the application:
```bash
dotnet publish -c Release -o ./publish
```

2. Copy files to IIS directory
3. Configure application pool for .NET Core
4. Set up environment variables for production

### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY publish/ .
EXPOSE 80
ENTRYPOINT ["dotnet", "PRReviewerBot.Api.dll"]
```

## 🔒 Security Considerations

- Webhook signature verification prevents unauthorized requests
- API keys are stored securely in configuration
- Request logging excludes sensitive data
- Rate limiting prevents API abuse

## 📝 Usage Example

When a PR is opened or updated targeting the `beta` branch, the bot will:

1. Receive the GitHub webhook
2. Fetch the changed files
3. Analyze code using OpenAI
4. Post structured review comments
5. Provide a summary with statistics

Example review comment:

```
🔒 **SECURITY** 🔴

**UserController.cs** (around line 45)

This endpoint is vulnerable to SQL injection. The user input is directly concatenated into the SQL query without parameterization.

**Recommendation**: Use parameterized queries or an ORM like Entity Framework to prevent SQL injection attacks.
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For issues and questions:
- Check the logs in the `logs/` directory
- Review health check endpoints
- Verify GitHub and OpenAI API connectivity
- Ensure webhook configuration is correct

---

## 🚫 CRITICAL SAFETY REMINDER

**🔒 COMMENT-ONLY MODE**: This bot operates in **COMMENT-ONLY MODE** and includes multiple safety mechanisms:

- ✅ **ONLY posts review comments** - Never merges PRs
- ✅ **NEVER approves PRs** - Human approval always required  
- ✅ **NEVER dismisses reviews** - Human reviews preserved
- ✅ **Built-in safety checks** - Validates safe operation before every action
- ✅ **Clear safety disclaimers** - All comments include safety information

**📖 For complete safety information, see [COMMENT-ONLY-MODE.md](./COMMENT-ONLY-MODE.md)**

**Perfect for teams that want AI-powered code review assistance without any automation risk!** 🛡️
