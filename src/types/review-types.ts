export interface CodeChange {
  filename: string;
  status: 'added' | 'removed' | 'modified' | 'renamed';
  additions: number;
  deletions: number;
  patch: string;
  language: string;
  content?: string;
}

export interface ReviewContext {
  pullRequestNumber: number;
  title: string;
  description: string;
  author: string;
  baseBranch: string;
  headBranch: string;
  repository: string;
  changes: CodeChange[];
  totalAdditions: number;
  totalDeletions: number;
}

export interface ReviewIssue {
  type: 'error' | 'warning' | 'suggestion' | 'info';
  severity: 'high' | 'medium' | 'low';
  category: 'security' | 'performance' | 'maintainability' | 'style' | 'logic' | 'best-practice';
  title: string;
  description: string;
  filename: string;
  line?: number;
  suggestion?: string;
  code?: string;
}

export interface OpenAIReviewResponse {
  overall_assessment: {
    score: number; // 1-10
    summary: string;
    recommendation: 'approve' | 'request_changes' | 'comment';
  };
  issues: ReviewIssue[];
  positive_aspects: string[];
  general_feedback: string;
}

export interface ReviewConfiguration {
  enabledCategories: string[];
  severityThreshold: 'low' | 'medium' | 'high';
  maxIssuesPerFile: number;
  skipFiles: string[];
  customPrompts: Record<string, string>;
}

