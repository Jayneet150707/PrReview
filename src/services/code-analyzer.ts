import { Octokit } from '@octokit/rest';
import { environment } from '../config/environment';
import { logger } from '../utils/logger';
import { GitHubFile, GitHubDiff } from '../types/github-types';
import { CodeChange, ReviewContext } from '../types/review-types';

export class CodeAnalyzer {
  private octokit: Octokit;

  constructor() {
    this.octokit = new Octokit({
      auth: environment.githubToken,
    });
  }

  async analyzePullRequest(
    owner: string,
    repo: string,
    pullNumber: number
  ): Promise<ReviewContext> {
    try {
      // Get PR details
      const { data: pullRequest } = await this.octokit.pulls.get({
        owner,
        repo,
        pull_number: pullNumber,
      });

      // Get PR files
      const { data: files } = await this.octokit.pulls.listFiles({
        owner,
        repo,
        pull_number: pullNumber,
      });

      // Filter and process files
      const meaningfulFiles = this.filterMeaningfulFiles(files);
      const limitedFiles = meaningfulFiles.slice(0, environment.maxFilesToReview);

      if (limitedFiles.length < meaningfulFiles.length) {
        logger.warn('Too many files in PR, limiting review', {
          totalFiles: meaningfulFiles.length,
          reviewedFiles: limitedFiles.length,
          maxFiles: environment.maxFilesToReview
        });
      }

      // Convert to CodeChange objects
      const changes: CodeChange[] = await Promise.all(
        limitedFiles.map(file => this.convertToCodeChange(file, owner, repo))
      );

      const reviewContext: ReviewContext = {
        pullRequestNumber: pullNumber,
        title: pullRequest.title,
        description: pullRequest.body || '',
        author: pullRequest.user?.login || 'unknown',
        baseBranch: pullRequest.base.ref,
        headBranch: pullRequest.head.ref,
        repository: `${owner}/${repo}`,
        changes,
        totalAdditions: pullRequest.additions || 0,
        totalDeletions: pullRequest.deletions || 0,
      };

      logger.debug('Code analysis completed', {
        pullRequest: pullNumber,
        filesAnalyzed: changes.length,
        totalAdditions: reviewContext.totalAdditions,
        totalDeletions: reviewContext.totalDeletions
      });

      return reviewContext;
    } catch (error) {
      logger.error('Failed to analyze pull request', {
        error: error instanceof Error ? error.message : 'Unknown error',
        owner,
        repo,
        pullNumber
      });
      throw error;
    }
  }

  private filterMeaningfulFiles(files: GitHubFile[]): GitHubFile[] {
    const skipPatterns = [
      /^package-lock\.json$/,
      /^yarn\.lock$/,
      /^pnpm-lock\.yaml$/,
      /^composer\.lock$/,
      /^Gemfile\.lock$/,
      /^poetry\.lock$/,
      /\.min\.(js|css)$/,
      /\.map$/,
      /^dist\//,
      /^build\//,
      /^node_modules\//,
      /^vendor\//,
      /\.log$/,
      /\.tmp$/,
      /\.cache$/,
      /^\.git/,
      /^\.vscode/,
      /^\.idea/,
      /\.DS_Store$/,
      /Thumbs\.db$/,
    ];

    return files.filter(file => {
      // Skip files that match skip patterns
      if (skipPatterns.some(pattern => pattern.test(file.filename))) {
        return false;
      }

      // Skip files without patches (binary files, etc.)
      if (!file.patch) {
        return false;
      }

      // Skip very large files
      if (file.changes > 1000) {
        logger.debug('Skipping large file', {
          filename: file.filename,
          changes: file.changes
        });
        return false;
      }

      return true;
    });
  }

  private async convertToCodeChange(
    file: GitHubFile,
    owner: string,
    repo: string
  ): Promise<CodeChange> {
    const language = this.detectLanguage(file.filename);
    
    let content: string | undefined;
    
    // For new files, try to get the content
    if (file.status === 'added' && file.contents_url) {
      try {
        const { data } = await this.octokit.repos.getContent({
          owner,
          repo,
          path: file.filename,
        });
        
        if ('content' in data && data.content) {
          content = Buffer.from(data.content, 'base64').toString('utf-8');
        }
      } catch (error) {
        logger.debug('Failed to get file content', {
          filename: file.filename,
          error: error instanceof Error ? error.message : 'Unknown error'
        });
      }
    }

    return {
      filename: file.filename,
      status: file.status,
      additions: file.additions,
      deletions: file.deletions,
      patch: file.patch || '',
      language,
      content,
    };
  }

  private detectLanguage(filename: string): string {
    const extension = filename.split('.').pop()?.toLowerCase();
    
    const languageMap: Record<string, string> = {
      'js': 'javascript',
      'jsx': 'javascript',
      'ts': 'typescript',
      'tsx': 'typescript',
      'py': 'python',
      'java': 'java',
      'c': 'c',
      'cpp': 'cpp',
      'cc': 'cpp',
      'cxx': 'cpp',
      'cs': 'csharp',
      'php': 'php',
      'rb': 'ruby',
      'go': 'go',
      'rs': 'rust',
      'swift': 'swift',
      'kt': 'kotlin',
      'scala': 'scala',
      'sh': 'bash',
      'bash': 'bash',
      'zsh': 'bash',
      'fish': 'bash',
      'ps1': 'powershell',
      'sql': 'sql',
      'html': 'html',
      'htm': 'html',
      'css': 'css',
      'scss': 'scss',
      'sass': 'sass',
      'less': 'less',
      'json': 'json',
      'xml': 'xml',
      'yaml': 'yaml',
      'yml': 'yaml',
      'toml': 'toml',
      'ini': 'ini',
      'cfg': 'ini',
      'conf': 'ini',
      'md': 'markdown',
      'markdown': 'markdown',
      'txt': 'text',
    };

    return languageMap[extension || ''] || 'text';
  }
}

