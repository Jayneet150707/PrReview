export interface GitHubPullRequest {
  id: number;
  number: number;
  title: string;
  body: string | null;
  state: 'open' | 'closed' | 'draft';
  base: {
    ref: string;
    sha: string;
    repo: {
      name: string;
      full_name: string;
      owner: {
        login: string;
      };
    };
  };
  head: {
    ref: string;
    sha: string;
    repo: {
      name: string;
      full_name: string;
      owner: {
        login: string;
      };
    };
  };
  user: {
    login: string;
    id: number;
  };
  created_at: string;
  updated_at: string;
  mergeable: boolean | null;
  mergeable_state: string;
  merged: boolean;
  additions: number;
  deletions: number;
  changed_files: number;
}

export interface GitHubWebhookPayload {
  action: string;
  number: number;
  pull_request: GitHubPullRequest;
  repository: {
    id: number;
    name: string;
    full_name: string;
    owner: {
      login: string;
      id: number;
    };
  };
  sender: {
    login: string;
    id: number;
  };
}

export interface GitHubFile {
  filename: string;
  status: 'added' | 'removed' | 'modified' | 'renamed';
  additions: number;
  deletions: number;
  changes: number;
  patch?: string;
  contents_url: string;
  raw_url: string;
}

export interface GitHubDiff {
  files: GitHubFile[];
  totalAdditions: number;
  totalDeletions: number;
  totalChanges: number;
}

export interface ReviewComment {
  path: string;
  line: number;
  body: string;
  side?: 'LEFT' | 'RIGHT';
  start_line?: number;
  start_side?: 'LEFT' | 'RIGHT';
}

export interface ReviewResult {
  event: 'APPROVE' | 'REQUEST_CHANGES' | 'COMMENT';
  body: string;
  comments: ReviewComment[];
}

