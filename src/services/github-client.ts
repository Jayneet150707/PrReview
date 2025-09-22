import { Octokit } from '@octokit/rest';
import { environment } from '../config/environment';
import { logger } from '../utils/logger';
import { ReviewResult, ReviewComment } from '../types/github-types';

export class GitHubClient {
  private octokit: Octokit;
  private rateLimitQueue: Promise<any> = Promise.resolve();

  constructor() {
    this.octokit = new Octokit({
      auth: environment.githubToken,
    });
  }

  async submitReview(
    owner: string,
    repo: string,
    pullNumber: number,
    review: ReviewResult
  ): Promise<void> {
    try {
      logger.info('Submitting review to GitHub', {
        owner,
        repo,
        pullNumber,
        event: review.event,
        commentsCount: review.comments.length
      });

      // Rate limiting
      await this.rateLimitQueue;
      this.rateLimitQueue = this.delay(3600000 / environment.githubRateLimitRph);

      // Submit the review
      const reviewResponse = await this.octokit.pulls.createReview({
        owner,
        repo,
        pull_number: pullNumber,
        body: review.body,
        event: review.event,
        comments: review.comments.map(comment => ({
          path: comment.path,
          line: comment.line,
          body: comment.body,
          side: comment.side || 'RIGHT',
          ...(comment.start_line && {
            start_line: comment.start_line,
            start_side: comment.start_side || 'RIGHT'
          })
        }))
      });

      logger.info('Review submitted successfully', {
        owner,
        repo,
        pullNumber,
        reviewId: reviewResponse.data.id,
        event: review.event
      });

    } catch (error) {
      logger.error('Failed to submit review', {
        error: error instanceof Error ? error.message : 'Unknown error',
        owner,
        repo,
        pullNumber,
        event: review.event
      });
      throw error;
    }
  }

  async addComment(
    owner: string,
    repo: string,
    pullNumber: number,
    body: string
  ): Promise<void> {
    try {
      logger.debug('Adding comment to PR', {
        owner,
        repo,
        pullNumber
      });

      // Rate limiting
      await this.rateLimitQueue;
      this.rateLimitQueue = this.delay(3600000 / environment.githubRateLimitRph);

      await this.octokit.issues.createComment({
        owner,
        repo,
        issue_number: pullNumber,
        body
      });

      logger.debug('Comment added successfully', {
        owner,
        repo,
        pullNumber
      });

    } catch (error) {
      logger.error('Failed to add comment', {
        error: error instanceof Error ? error.message : 'Unknown error',
        owner,
        repo,
        pullNumber
      });
      throw error;
    }
  }

  async addReviewComment(
    owner: string,
    repo: string,
    pullNumber: number,
    comment: ReviewComment
  ): Promise<void> {
    try {
      logger.debug('Adding review comment to PR', {
        owner,
        repo,
        pullNumber,
        path: comment.path,
        line: comment.line
      });

      // Rate limiting
      await this.rateLimitQueue;
      this.rateLimitQueue = this.delay(3600000 / environment.githubRateLimitRph);

      await this.octokit.pulls.createReviewComment({
        owner,
        repo,
        pull_number: pullNumber,
        body: comment.body,
        path: comment.path,
        line: comment.line,
        side: comment.side || 'RIGHT',
        ...(comment.start_line && {
          start_line: comment.start_line,
          start_side: comment.start_side || 'RIGHT'
        })
      });

      logger.debug('Review comment added successfully', {
        owner,
        repo,
        pullNumber,
        path: comment.path,
        line: comment.line
      });

    } catch (error) {
      logger.error('Failed to add review comment', {
        error: error instanceof Error ? error.message : 'Unknown error',
        owner,
        repo,
        pullNumber,
        path: comment.path,
        line: comment.line
      });
      throw error;
    }
  }

  async updatePullRequestStatus(
    owner: string,
    repo: string,
    sha: string,
    state: 'pending' | 'success' | 'failure' | 'error',
    description: string,
    context: string = 'pr-reviewer-bot'
  ): Promise<void> {
    try {
      logger.debug('Updating PR status', {
        owner,
        repo,
        sha,
        state,
        context
      });

      // Rate limiting
      await this.rateLimitQueue;
      this.rateLimitQueue = this.delay(3600000 / environment.githubRateLimitRph);

      await this.octokit.repos.createCommitStatus({
        owner,
        repo,
        sha,
        state,
        description,
        context
      });

      logger.debug('PR status updated successfully', {
        owner,
        repo,
        sha,
        state,
        context
      });

    } catch (error) {
      logger.error('Failed to update PR status', {
        error: error instanceof Error ? error.message : 'Unknown error',
        owner,
        repo,
        sha,
        state,
        context
      });
      throw error;
    }
  }

  async getPullRequest(
    owner: string,
    repo: string,
    pullNumber: number
  ): Promise<any> {
    try {
      // Rate limiting
      await this.rateLimitQueue;
      this.rateLimitQueue = this.delay(3600000 / environment.githubRateLimitRph);

      const { data } = await this.octokit.pulls.get({
        owner,
        repo,
        pull_number: pullNumber
      });

      return data;
    } catch (error) {
      logger.error('Failed to get pull request', {
        error: error instanceof Error ? error.message : 'Unknown error',
        owner,
        repo,
        pullNumber
      });
      throw error;
    }
  }

  async getPullRequestFiles(
    owner: string,
    repo: string,
    pullNumber: number
  ): Promise<any[]> {
    try {
      // Rate limiting
      await this.rateLimitQueue;
      this.rateLimitQueue = this.delay(3600000 / environment.githubRateLimitRph);

      const { data } = await this.octokit.pulls.listFiles({
        owner,
        repo,
        pull_number: pullNumber
      });

      return data;
    } catch (error) {
      logger.error('Failed to get pull request files', {
        error: error instanceof Error ? error.message : 'Unknown error',
        owner,
        repo,
        pullNumber
      });
      throw error;
    }
  }

  private delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}

