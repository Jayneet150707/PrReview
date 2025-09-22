import { Router } from 'express';
import { logger } from '../utils/logger';
import { environment } from '../config/environment';
import { GitHubWebhookPayload } from '../types/github-types';
import { validateGitHubWebhook, validateContentType, validateGitHubEvent } from '../middleware/webhook-validator';
import { asyncHandler } from '../middleware/error-handler';
import { CodeAnalyzer } from '../services/code-analyzer';
import { OpenAIReviewer } from '../services/openai-reviewer';
import { GitHubClient } from '../services/github-client';

const router = Router();
const codeAnalyzer = new CodeAnalyzer();
const openaiReviewer = new OpenAIReviewer();
const githubClient = new GitHubClient();

// Apply middleware
router.use(validateContentType);
router.use(validateGitHubWebhook);
router.use(validateGitHubEvent);

// Handle pull request events
router.post('/', asyncHandler(async (req, res) => {
  const payload: GitHubWebhookPayload = req.body;
  const event = req.body.githubEvent;

  logger.info('GitHub webhook received', {
    event,
    action: payload.action,
    pullRequest: payload.pull_request?.number,
    repository: payload.repository?.full_name
  });

  // Only process pull request events
  if (event !== 'pull_request') {
    logger.debug('Ignoring non-pull-request event', { event });
    return res.status(200).json({ message: 'Event ignored' });
  }

  // Only process relevant actions
  const relevantActions = ['opened', 'synchronize', 'reopened'];
  if (!relevantActions.includes(payload.action)) {
    logger.debug('Ignoring pull request action', { action: payload.action });
    return res.status(200).json({ message: 'Action ignored' });
  }

  // Check if review is enabled
  if (!environment.reviewEnabled) {
    logger.info('Review is disabled, skipping');
    return res.status(200).json({ message: 'Review disabled' });
  }

  // Check if PR targets the configured branch
  const targetBranch = payload.pull_request.base.ref;
  if (targetBranch !== environment.targetBranch) {
    logger.debug('PR does not target configured branch', {
      targetBranch,
      configuredBranch: environment.targetBranch
    });
    return res.status(200).json({ message: 'Branch not configured for review' });
  }

  try {
    await processPullRequest(payload);
    res.status(200).json({ message: 'Pull request processed successfully' });
  } catch (error) {
    logger.error('Failed to process pull request', {
      error: error instanceof Error ? error.message : 'Unknown error',
      pullRequest: payload.pull_request.number,
      repository: payload.repository.full_name
    });
    
    // Don't fail the webhook, just log the error
    res.status(200).json({ message: 'Pull request processing failed' });
  }
}));

async function processPullRequest(payload: GitHubWebhookPayload): Promise<void> {
  const { pull_request: pr, repository } = payload;
  
  logger.info('Processing pull request', {
    number: pr.number,
    title: pr.title,
    repository: repository.full_name,
    author: pr.user.login
  });

  try {
    // Step 1: Analyze code changes
    logger.debug('Analyzing code changes');
    const reviewContext = await codeAnalyzer.analyzePullRequest(
      repository.owner.login,
      repository.name,
      pr.number
    );

    // Step 2: Skip if no meaningful changes
    if (reviewContext.changes.length === 0) {
      logger.info('No meaningful changes found, skipping review');
      return;
    }

    // Step 3: Skip if changes are too large
    if (reviewContext.totalAdditions + reviewContext.totalDeletions > environment.maxDiffSize) {
      logger.info('Changes too large for review', {
        totalChanges: reviewContext.totalAdditions + reviewContext.totalDeletions,
        maxSize: environment.maxDiffSize
      });
      
      await githubClient.addComment(
        repository.owner.login,
        repository.name,
        pr.number,
        '🤖 **PR Reviewer Bot**\n\n' +
        'This PR contains too many changes for automated review. ' +
        'Please consider breaking it into smaller PRs for better review quality.'
      );
      return;
    }

    // Step 4: Get AI review
    logger.debug('Getting AI review');
    const reviewResult = await openaiReviewer.reviewCode(reviewContext);

    // Step 5: Post review to GitHub
    logger.debug('Posting review to GitHub');
    await githubClient.submitReview(
      repository.owner.login,
      repository.name,
      pr.number,
      reviewResult
    );

    logger.info('Pull request review completed successfully', {
      number: pr.number,
      repository: repository.full_name,
      recommendation: reviewResult.event,
      issuesFound: reviewResult.comments.length
    });

  } catch (error) {
    logger.error('Error processing pull request', {
      error: error instanceof Error ? error.message : 'Unknown error',
      stack: error instanceof Error ? error.stack : undefined,
      pullRequest: pr.number,
      repository: repository.full_name
    });
    
    // Post error comment to PR
    try {
      await githubClient.addComment(
        repository.owner.login,
        repository.name,
        pr.number,
        '🤖 **PR Reviewer Bot**\n\n' +
        '❌ An error occurred while reviewing this PR. Please check the logs or contact the administrator.'
      );
    } catch (commentError) {
      logger.error('Failed to post error comment', {
        error: commentError instanceof Error ? commentError.message : 'Unknown error'
      });
    }
    
    throw error;
  }
}

export { router as githubWebhookHandler };

