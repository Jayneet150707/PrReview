# 🧪 Local Testing Guide for PR Reviewer Bot

This directory contains comprehensive testing files and scenarios to test your PR reviewer bot locally.

## 📁 Testing Structure

```
tests/local-testing/
├── README.md                    # This guide
├── test-scenarios/              # Different test scenarios
│   ├── basic-test.js           # Simple JavaScript test
│   ├── security-issues.js      # Security vulnerability tests
│   ├── performance-issues.js   # Performance problem tests
│   ├── best-practices.js       # Code quality tests
│   └── complex-logic.js        # Complex code logic tests
├── sample-files/               # Sample files for testing
│   ├── good-code.js           # Well-written code example
│   ├── bad-code.js            # Poorly written code example
│   └── mixed-quality.js       # Mixed quality code
└── scripts/                   # Testing automation scripts
    ├── create-test-pr.sh      # Script to create test PRs
    ├── setup-local-test.sh    # Local testing setup
    └── cleanup-test.sh        # Cleanup test branches
```

## 🚀 Quick Testing Steps

### 1. Setup Local Environment
```bash
# From repository root
npm install
cp .env.example .env
# Edit .env with your API keys
```

### 2. Start Bot Locally
```bash
npm run dev
```

### 3. Expose with ngrok
```bash
# In new terminal
ngrok http 3000
# Copy the https URL
```

### 4. Configure GitHub Webhook
- Repository Settings → Webhooks → Add webhook
- URL: `https://your-ngrok-url.ngrok.io/webhook/github`
- Events: "Pull requests"
- Secret: Same as in .env

### 5. Run Test Scenarios
```bash
# Make the scripts executable
chmod +x tests/local-testing/scripts/*.sh

# Run automated test
./tests/local-testing/scripts/create-test-pr.sh
```

## 🧪 Manual Testing Scenarios

### Scenario 1: Basic Code Review
```bash
git checkout -b test-basic-review
cp tests/local-testing/test-scenarios/basic-test.js ./
git add . && git commit -m "Add basic test file"
git push origin test-basic-review
# Create PR targeting beta branch
```

### Scenario 2: Security Issues
```bash
git checkout -b test-security-issues
cp tests/local-testing/test-scenarios/security-issues.js ./
git add . && git commit -m "Add security test file"
git push origin test-security-issues
# Create PR targeting beta branch
```

### Scenario 3: Performance Issues
```bash
git checkout -b test-performance-issues
cp tests/local-testing/test-scenarios/performance-issues.js ./
git add . && git commit -m "Add performance test file"
git push origin test-performance-issues
# Create PR targeting beta branch
```

## 📊 Expected Bot Behavior

The bot should:
- ✅ **Detect security vulnerabilities** (hardcoded secrets, SQL injection risks)
- ✅ **Identify performance issues** (inefficient loops, memory leaks)
- ✅ **Suggest best practices** (proper error handling, code organization)
- ✅ **Provide constructive feedback** with specific suggestions
- ✅ **Ignore irrelevant files** (lock files, build artifacts)

## 🔍 Monitoring Your Tests

### Check Bot Logs
```bash
# Terminal running npm run dev should show:
[INFO] Webhook received: pull_request.opened
[INFO] Processing PR #X targeting beta
[INFO] Analyzing N files...
[INFO] Sending to OpenAI for review...
[INFO] Posted review comment successfully
```

### Check ngrok Interface
- Open: http://127.0.0.1:4040
- Monitor webhook deliveries
- Check request/response details

### Check GitHub
- Webhook deliveries in repository settings
- PR comments from the bot
- Review feedback quality

## 🎯 Success Criteria

Your bot is working correctly when:
- ✅ Webhooks are received successfully
- ✅ Bot processes PRs targeting beta branch only
- ✅ Review comments appear on PRs
- ✅ Feedback is relevant and constructive
- ✅ Security and performance issues are identified
- ✅ No errors in bot logs

## 🔧 Troubleshooting

### Common Issues:
1. **Webhook not received**: Check ngrok URL and GitHub webhook config
2. **No bot comments**: Verify PR targets beta branch and check API keys
3. **Authentication errors**: Check GitHub token permissions and OpenAI credits
4. **Rate limiting**: Monitor API usage and adjust limits

### Debug Commands:
```bash
# Test health endpoint
curl http://localhost:3000/health

# Test GitHub token
curl -H "Authorization: token YOUR_TOKEN" https://api.github.com/user

# Test OpenAI key
curl -H "Authorization: Bearer YOUR_KEY" https://api.openai.com/v1/models
```

## 🎉 Ready for Production

Once all tests pass:
1. Deploy to your preferred platform
2. Update webhook URL to production domain
3. Set NODE_ENV=production
4. Monitor production logs and performance

Happy testing! 🚀

