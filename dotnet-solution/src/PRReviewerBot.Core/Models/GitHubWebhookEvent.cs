using System.Text.Json.Serialization;

namespace PRReviewerBot.Core.Models;

/// <summary>
/// Represents a GitHub webhook event payload
/// </summary>
public class GitHubWebhookEvent
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("pull_request")]
    public PullRequest? PullRequest { get; set; }

    [JsonPropertyName("repository")]
    public Repository? Repository { get; set; }

    [JsonPropertyName("sender")]
    public User? Sender { get; set; }
}

/// <summary>
/// Represents a GitHub pull request
/// </summary>
public class PullRequest
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("base")]
    public PullRequestBranch? Base { get; set; }

    [JsonPropertyName("head")]
    public PullRequestBranch? Head { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    [JsonPropertyName("changed_files")]
    public int ChangedFiles { get; set; }

    [JsonPropertyName("diff_url")]
    public string? DiffUrl { get; set; }

    [JsonPropertyName("patch_url")]
    public string? PatchUrl { get; set; }
}

/// <summary>
/// Represents a GitHub pull request branch
/// </summary>
public class PullRequestBranch
{
    [JsonPropertyName("ref")]
    public string Ref { get; set; } = string.Empty;

    [JsonPropertyName("sha")]
    public string Sha { get; set; } = string.Empty;

    [JsonPropertyName("repo")]
    public Repository? Repo { get; set; }
}

/// <summary>
/// Represents a GitHub repository
/// </summary>
public class Repository
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("owner")]
    public User? Owner { get; set; }

    [JsonPropertyName("private")]
    public bool Private { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonPropertyName("clone_url")]
    public string? CloneUrl { get; set; }

    [JsonPropertyName("default_branch")]
    public string DefaultBranch { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string? Language { get; set; }
}

/// <summary>
/// Represents a GitHub user
/// </summary>
public class User
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Represents a GitHub file in a pull request
/// </summary>
public class GitHubFile
{
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    [JsonPropertyName("changes")]
    public int Changes { get; set; }

    [JsonPropertyName("patch")]
    public string? Patch { get; set; }

    [JsonPropertyName("blob_url")]
    public string? BlobUrl { get; set; }

    [JsonPropertyName("raw_url")]
    public string? RawUrl { get; set; }

    [JsonPropertyName("contents_url")]
    public string? ContentsUrl { get; set; }
}
