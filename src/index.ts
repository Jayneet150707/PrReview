import express from 'express';
import dotenv from 'dotenv';
import { logger } from './utils/logger';
import { environment } from './config/environment';
import { githubWebhookHandler } from './webhooks/github-handler';
import { errorHandler } from './middleware/error-handler';

// Load environment variables
dotenv.config();

const app = express();

// Middleware
app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true }));

// Health check endpoint
app.get('/health', (req, res) => {
  res.status(200).json({
    status: 'healthy',
    timestamp: new Date().toISOString(),
    version: process.env.npm_package_version || '1.0.0'
  });
});

// GitHub webhook endpoint
app.use('/webhook/github', githubWebhookHandler);

// Error handling middleware
app.use(errorHandler);

// Start server
const PORT = environment.port;
app.listen(PORT, () => {
  logger.info(`🤖 PR Reviewer Bot started on port ${PORT}`, {
    port: PORT,
    nodeEnv: environment.nodeEnv,
    targetBranch: environment.targetBranch
  });
});

// Graceful shutdown
process.on('SIGTERM', () => {
  logger.info('SIGTERM received, shutting down gracefully');
  process.exit(0);
});

process.on('SIGINT', () => {
  logger.info('SIGINT received, shutting down gracefully');
  process.exit(0);
});

export default app;

