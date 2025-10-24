// PR Reviewer Bot - Main Server Entry Point
// Optimized for Windows Server IIS deployment with iisnode

const express = require('express');
const crypto = require('crypto');
const { Octokit } = require('@octokit/rest');
const OpenAI = require('openai');
const path = require('path');
const fs = require('fs');

// Load environment variables
require('dotenv').config();

const app = express();

// Configuration
const config = {
    port: process.env.PORT || process.env.IISNODE_PORT || 3000,
    github: {
        token: process.env.GITHUB_TOKEN,
        webhookSecret: process.env.GITHUB_WEBHOOK_SECRET,
    },
    openai: {
        apiKey: process.env.OPENAI_API_KEY,
        model: process.env.OPENAI_MODEL || 'gpt-4',
    },
    targetBranch: process.env.TARGET_BRANCH || 'beta',
    maxFilesToReview: parseInt(process.env.MAX_FILES_TO_REVIEW) || 20,
    maxDiffSize: parseInt(process.env.MAX_DIFF_SIZE) || 10000,
    logLevel: process.env.LOG_LEVEL || 'info',
};

// Initialize clients
const octokit = new Octokit({ auth: config.github.token });
const openai = new OpenAI({ apiKey: config.openai.apiKey });

// Logging utility
const logger = {
    info: (message, data = {}) => {
        const timestamp = new Date().toISOString();
        const logEntry = { timestamp, level: 'INFO', message, ...data };
        console.log(JSON.stringify(logEntry));
        
        // Write to file for IIS
        if (process.env.NODE_ENV === 'production') {
            const logDir = path.join(__dirname, 'logs');
            if (!fs.existsSync(logDir)) fs.mkdirSync(logDir, { recursive: true });
            
            const logFile = path.join(logDir, `app-${new Date().toISOString().split('T')[0]}.log`);
            fs.appendFileSync(logFile, JSON.stringify(logEntry) + '\n');
        }
    },
    error: (message, error = {}) => {
        const timestamp = new Date().toISOString();
        const logEntry = { 
            timestamp, 
            level: 'ERROR', 
            message, 
            error: error.message || error,
            stack: error.stack 
        };
        console.error(JSON.stringify(logEntry));
        
        // Write to file for IIS
        if (process.env.NODE_ENV === 'production') {
            const logDir = path.join(__dirname, 'logs');
            if (!fs.existsSync(logDir)) fs.mkdirSync(logDir, { recursive: true });
            
            const logFile = path.join(logDir, `error-${new Date().toISOString().split('T')[0]}.log`);
            fs.appendFileSync(logFile, JSON.stringify(logEntry) + '\n');
        }
    }
};

// Middleware
app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true }));

// Health check endpoint
app.get('/health', (req, res) => {
    const health = {
        status: 'healthy',
        timestamp: new Date().toISOString(),
        version: process.env.npm_package_version || '1.0.0',
        environment: process.env.NODE_ENV || 'development',
        uptime: process.uptime(),
        memory: process.memoryUsage(),
        github: {
            configured: !!config.github.token,
            webhookSecret: !!config.github.webhookSecret,
        },
        openai: {
            configured: !!config.openai.apiKey,
            model: config.openai.model,
        },
        config: {
            targetBranch: config.targetBranch,
            maxFiles: config.maxFilesToReview,
            maxDiffSize: config.maxDiffSize,
        }
    };
    
    logger.info('Health check requested', { ip: req.ip, userAgent: req.get('User-Agent') });
    res.json(health);
});

// Webhook signature verification
function verifyWebhookSignature(req, res, next) {
    if (!config.github.webhookSecret) {
        logger.error('Webhook secret not configured');
        return res.status(500).json({ error: 'Webhook secret not configured' });
    }

    const signature = req.get('X-Hub-Signature-256');
    if (!signature) {
        logger.error('Missing webhook signature');
        return res.status(401).json({ error: 'Missing signature' });
    }

    const expectedSignature = 'sha256=' + crypto
        .createHmac('sha256', config.github.webhookSecret)
        .update(JSON.stringify(req.body))
        .digest('hex');

    if (!crypto.timingSafeEqual(Buffer.from(signature), Buffer.from(expectedSignature))) {
        logger.error('Invalid webhook signature');
        return res.status(401).json({ error: 'Invalid signature' });
    }

    next();
}

// File filtering utility
function shouldReviewFile(filename) {
    const skipExtensions = ['.lock', '.min.js', '.min.css', '.map', '.png', '.jpg', '.jpeg', '.gif', '.svg', '.ico', '.woff', '.woff2', '.ttf', '.eot'];
    const skipPaths = ['node_modules/', 'dist/', 'build/', 'coverage/', '.git/', 'logs/'];
    
    // Skip files with certain extensions
    if (skipExtensions.some(ext => filename.endsWith(ext))) {
        return false;
    }
    
    // Skip files in certain directories
    if (skipPaths.some(path => filename.includes(path))) {
        return false;
    }
    
    return true;
}

// OpenAI code review
async function reviewCodeWithAI(files, prContext) {
    const prompt = `You are an expert code reviewer. Please review the following code changes and provide constructive feedback.

PR Context:
- Title: ${prContext.title}
- Description: ${prContext.description}
- Target Branch: ${prContext.targetBranch}
- Files Changed: ${files.length}

Focus on:
1. Security vulnerabilities (SQL injection, XSS, hardcoded secrets, etc.)
2. Performance issues (inefficient algorithms, memory leaks, etc.)
3. Code quality (error handling, best practices, maintainability)
4. Potential bugs or edge cases
5. Suggestions for improvement

Files to review:
${files.map(file => `
File: ${file.filename}
Status: ${file.status}
Changes: +${file.additions} -${file.deletions}

Code:
\`\`\`${file.language || 'text'}
${file.patch || 'No changes to display'}
\`\`\`
`).join('\n')}

Please provide specific, actionable feedback. If the code looks good, mention what's done well. Be constructive and helpful.`;

    try {
        const response = await openai.chat.completions.create({
            model: config.openai.model,
            messages: [
                {
                    role: 'system',
                    content: 'You are a senior software engineer conducting a thorough code review. Provide specific, actionable feedback that helps improve code quality, security, and performance.'
                },
                {
                    role: 'user',
                    content: prompt
                }
            ],
            max_tokens: 2000,
            temperature: 0.3,
        });

        return response.choices[0].message.content;
    } catch (error) {
        logger.error('OpenAI API error', error);
        throw new Error(`OpenAI API error: ${error.message}`);
    }
}

// Main webhook handler
app.post('/webhook/github', verifyWebhookSignature, async (req, res) => {
    const { action, pull_request: pr, repository } = req.body;

    logger.info('Webhook received', { 
        action, 
        repository: repository?.name,
        pr: pr?.number,
        targetBranch: pr?.base?.ref 
    });

    // Only process opened and synchronize events
    if (!['opened', 'synchronize'].includes(action)) {
        logger.info('Ignoring webhook action', { action });
        return res.status(200).json({ message: 'Action ignored' });
    }

    // Only process PRs targeting the configured branch
    if (pr.base.ref !== config.targetBranch) {
        logger.info('PR not targeting configured branch', { 
            targetBranch: pr.base.ref, 
            configuredBranch: config.targetBranch 
        });
        return res.status(200).json({ message: 'Branch not configured for review' });
    }

    try {
        // Get PR files
        const { data: files } = await octokit.rest.pulls.listFiles({
            owner: repository.owner.login,
            repo: repository.name,
            pull_number: pr.number,
        });

        // Filter files for review
        const reviewableFiles = files
            .filter(file => shouldReviewFile(file.filename))
            .slice(0, config.maxFilesToReview);

        if (reviewableFiles.length === 0) {
            logger.info('No reviewable files found', { prNumber: pr.number });
            return res.status(200).json({ message: 'No files to review' });
        }

        // Check total diff size
        const totalChanges = reviewableFiles.reduce((sum, file) => sum + (file.changes || 0), 0);
        if (totalChanges > config.maxDiffSize) {
            logger.info('PR too large for review', { 
                prNumber: pr.number, 
                totalChanges, 
                maxSize: config.maxDiffSize 
            });
            
            await octokit.rest.issues.createComment({
                owner: repository.owner.login,
                repo: repository.name,
                issue_number: pr.number,
                body: `🤖 **PR Reviewer Bot**\n\nThis PR is too large for automated review (${totalChanges} changes, max ${config.maxDiffSize}). Please consider breaking it into smaller PRs for better review quality.`
            });
            
            return res.status(200).json({ message: 'PR too large' });
        }

        logger.info('Starting code review', { 
            prNumber: pr.number, 
            filesCount: reviewableFiles.length,
            totalChanges 
        });

        // Prepare context for AI review
        const prContext = {
            title: pr.title,
            description: pr.body || 'No description provided',
            targetBranch: pr.base.ref,
            author: pr.user.login,
        };

        // Get AI review
        const reviewComment = await reviewCodeWithAI(reviewableFiles, prContext);

        // Post review comment
        await octokit.rest.issues.createComment({
            owner: repository.owner.login,
            repo: repository.name,
            issue_number: pr.number,
            body: `🤖 **Automated Code Review**\n\n${reviewComment}\n\n---\n*This review was generated by PR Reviewer Bot. Please use your judgment and conduct additional manual review as needed.*`
        });

        logger.info('Review posted successfully', { prNumber: pr.number });
        res.status(200).json({ message: 'Review completed' });

    } catch (error) {
        logger.error('Error processing webhook', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

// Error handling middleware
app.use((error, req, res, next) => {
    logger.error('Unhandled error', error);
    res.status(500).json({ error: 'Internal server error' });
});

// 404 handler
app.use((req, res) => {
    res.status(404).json({ error: 'Not found' });
});

// Start server
const server = app.listen(config.port, () => {
    logger.info('PR Reviewer Bot started', {
        port: config.port,
        environment: process.env.NODE_ENV || 'development',
        targetBranch: config.targetBranch,
        pid: process.pid
    });
});

// Graceful shutdown
process.on('SIGTERM', () => {
    logger.info('SIGTERM received, shutting down gracefully');
    server.close(() => {
        logger.info('Server closed');
        process.exit(0);
    });
});

process.on('SIGINT', () => {
    logger.info('SIGINT received, shutting down gracefully');
    server.close(() => {
        logger.info('Server closed');
        process.exit(0);
    });
});

// Handle uncaught exceptions
process.on('uncaughtException', (error) => {
    logger.error('Uncaught exception', error);
    process.exit(1);
});

process.on('unhandledRejection', (reason, promise) => {
    logger.error('Unhandled rejection', { reason, promise });
    process.exit(1);
});

module.exports = app;

