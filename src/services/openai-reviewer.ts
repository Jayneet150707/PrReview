import OpenAI from 'openai';
import { environment } from '../config/environment';
import { logger } from '../utils/logger';
import { ReviewContext, OpenAIReviewResponse } from '../types/review-types';
import { ReviewResult, ReviewComment } from '../types/github-types';
import { 
  SYSTEM_PROMPT, 
  USER_PROMPT_TEMPLATE, 
  formatCodeChanges, 
  FOLLOW_UP_PROMPT 
} from '../prompts/review-prompts';

export class OpenAIReviewer {
  private openai: OpenAI;
  private rateLimitQueue: Promise<any> = Promise.resolve();

  constructor() {
    this.openai = new OpenAI({
      apiKey: environment.openaiApiKey,
    });
  }

  async reviewCode(context: ReviewContext): Promise<ReviewResult> {
    try {
      logger.info('Starting OpenAI code review', {
        pullRequest: context.pullRequestNumber,
        filesCount: context.changes.length,
        totalChanges: context.totalAdditions + context.totalDeletions
      });

      // Rate limiting
      await this.rateLimitQueue;
      this.rateLimitQueue = this.delay(60000 / environment.openaiRateLimitRpm);

      const reviewResponse = await this.getAIReview(context);
      const githubReview = this.convertToGitHubReview(reviewResponse, context);

      logger.info('OpenAI code review completed', {
        pullRequest: context.pullRequestNumber,
        recommendation: githubReview.event,
        issuesFound: reviewResponse.issues.length,
        score: reviewResponse.overall_assessment.score
      });

      return githubReview;
    } catch (error) {
      logger.error('OpenAI review failed', {
        error: error instanceof Error ? error.message : 'Unknown error',
        pullRequest: context.pullRequestNumber
      });
      
      // Return a fallback review
      return this.createFallbackReview(error);
    }
  }

  private async getAIReview(context: ReviewContext): Promise<OpenAIReviewResponse> {
    const codeChanges = formatCodeChanges(context.changes);
    
    const userPrompt = USER_PROMPT_TEMPLATE
      .replace('{title}', context.title)
      .replace('{author}', context.author)
      .replace('{description}', context.description || 'No description provided')
      .replace('{repository}', context.repository)
      .replace('{baseBranch}', context.baseBranch)
      .replace('{headBranch}', context.headBranch)
      .replace('{totalAdditions}', context.totalAdditions.toString())
      .replace('{totalDeletions}', context.totalDeletions.toString())
      .replace('{codeChanges}', codeChanges);

    const messages: OpenAI.Chat.Completions.ChatCompletionMessageParam[] = [
      { role: 'system', content: SYSTEM_PROMPT },
      { role: 'user', content: userPrompt },
      { role: 'user', content: FOLLOW_UP_PROMPT }
    ];

    logger.debug('Sending request to OpenAI', {
      model: environment.openaiModel,
      maxTokens: environment.openaiMaxTokens,
      messagesCount: messages.length
    });

    const completion = await this.openai.chat.completions.create({
      model: environment.openaiModel,
      messages,
      max_tokens: environment.openaiMaxTokens,
      temperature: 0.1,
      response_format: { type: 'json_object' }
    });

    const responseContent = completion.choices[0]?.message?.content;
    if (!responseContent) {
      throw new Error('No response content from OpenAI');
    }

    try {
      const reviewResponse: OpenAIReviewResponse = JSON.parse(responseContent);
      this.validateReviewResponse(reviewResponse);
      return reviewResponse;
    } catch (parseError) {
      logger.error('Failed to parse OpenAI response', {
        error: parseError instanceof Error ? parseError.message : 'Unknown error',
        response: responseContent
      });
      throw new Error('Invalid JSON response from OpenAI');
    }
  }

  private validateReviewResponse(response: any): void {
    if (!response.overall_assessment || !response.issues || !Array.isArray(response.issues)) {
      throw new Error('Invalid review response structure');
    }

    if (!response.overall_assessment.score || !response.overall_assessment.recommendation) {
      throw new Error('Invalid overall assessment structure');
    }
  }

  private convertToGitHubReview(
    aiResponse: OpenAIReviewResponse, 
    context: ReviewContext
  ): ReviewResult {
    const { overall_assessment, issues, positive_aspects, general_feedback } = aiResponse;

    // Determine GitHub review event
    let event: 'APPROVE' | 'REQUEST_CHANGES' | 'COMMENT';
    if (overall_assessment.recommendation === 'approve' && overall_assessment.score >= 8) {
      event = environment.autoApproveEnabled ? 'APPROVE' : 'COMMENT';
    } else if (overall_assessment.recommendation === 'request_changes' || overall_assessment.score < 6) {
      event = 'REQUEST_CHANGES';
    } else {
      event = 'COMMENT';
    }

    // Create review body
    const reviewBody = this.formatReviewBody(
      overall_assessment,
      positive_aspects,
      general_feedback,
      issues.length
    );

    // Convert issues to GitHub review comments
    const comments: ReviewComment[] = issues
      .filter(issue => issue.filename && issue.line)
      .map(issue => ({
        path: issue.filename,
        line: issue.line!,
        body: this.formatIssueComment(issue)
      }));

    return {
      event,
      body: reviewBody,
      comments
    };
  }

  private formatReviewBody(
    assessment: any,
    positiveAspects: string[],
    generalFeedback: string,
    issueCount: number
  ): string {
    const scoreEmoji = assessment.score >= 8 ? '🟢' : assessment.score >= 6 ? '🟡' : '🔴';
    const eventEmoji = assessment.recommendation === 'approve' ? '✅' : 
                      assessment.recommendation === 'request_changes' ? '❌' : '💬';

    let body = `## 🤖 AI Code Review\n\n`;
    body += `${eventEmoji} **Overall Assessment**: ${scoreEmoji} ${assessment.score}/10\n\n`;
    body += `**Summary**: ${assessment.summary}\n\n`;

    if (positiveAspects && positiveAspects.length > 0) {
      body += `### ✨ Positive Aspects\n`;
      positiveAspects.forEach(aspect => {
        body += `- ${aspect}\n`;
      });
      body += `\n`;
    }

    if (issueCount > 0) {
      body += `### 🔍 Issues Found\n`;
      body += `Found ${issueCount} issue(s) that need attention. See individual comments below.\n\n`;
    }

    if (generalFeedback) {
      body += `### 💡 General Feedback\n`;
      body += `${generalFeedback}\n\n`;
    }

    body += `---\n`;
    body += `*This review was generated by AI. Please use your judgment and consider the context.*`;

    return body;
  }

  private formatIssueComment(issue: any): string {
    const severityEmoji = {
      'high': '🚨',
      'medium': '⚠️',
      'low': '💡'
    };

    const categoryEmoji = {
      'security': '🔒',
      'performance': '⚡',
      'maintainability': '🔧',
      'style': '🎨',
      'logic': '🧠',
      'best-practice': '📋'
    };

    let comment = `${severityEmoji[issue.severity] || '💡'} **${issue.title}**\n\n`;
    comment += `${categoryEmoji[issue.category] || '📝'} *${issue.category}* | *${issue.severity} severity*\n\n`;
    comment += `${issue.description}\n`;

    if (issue.suggestion) {
      comment += `\n**Suggestion:**\n${issue.suggestion}`;
    }

    if (issue.code) {
      comment += `\n\n**Example:**\n\`\`\`\n${issue.code}\n\`\`\``;
    }

    return comment;
  }

  private createFallbackReview(error: any): ReviewResult {
    return {
      event: 'COMMENT',
      body: `## 🤖 AI Code Review\n\n` +
            `❌ **Review Failed**: Unable to complete automated review due to an error.\n\n` +
            `**Error**: ${error instanceof Error ? error.message : 'Unknown error'}\n\n` +
            `Please review this PR manually or try again later.\n\n` +
            `---\n` +
            `*This is an automated message from the PR Reviewer Bot.*`,
      comments: []
    };
  }

  private delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}

