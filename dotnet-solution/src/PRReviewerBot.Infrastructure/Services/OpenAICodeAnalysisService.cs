using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using PRReviewerBot.Core.Interfaces;
using PRReviewerBot.Core.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace PRReviewerBot.Infrastructure.Services;

/// <summary>
/// OpenAI-powered code analysis service for generating review comments
/// </summary>
public class OpenAICodeAnalysisService : ICodeAnalysisService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<OpenAICodeAnalysisService> _logger;
    private readonly IConfiguration _configuration;

    // File extensions that should be reviewed
    private static readonly HashSet<string> ReviewableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".vb", ".fs", // .NET
        ".js", ".ts", ".jsx", ".tsx", ".vue", // JavaScript/TypeScript
        ".py", ".pyw", // Python
        ".java", ".kt", ".scala", // JVM languages
        ".cpp", ".c", ".h", ".hpp", ".cc", ".cxx", // C/C++
        ".php", ".rb", ".go", ".rs", ".swift", // Other languages
        ".sql", ".xml", ".json", ".yaml", ".yml", // Data/Config
        ".html", ".css", ".scss", ".less", // Web
        ".sh", ".ps1", ".bat", ".cmd" // Scripts
    };

    // File patterns to skip
    private static readonly HashSet<string> SkipPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "node_modules", "bin", "obj", "dist", "build", "target", 
        ".git", ".vs", ".vscode", "packages", "vendor",
        "*.min.js", "*.min.css", "*.bundle.js", "*.bundle.css",
        "package-lock.json", "yarn.lock", "composer.lock"
    };

    public OpenAICodeAnalysisService(
        ChatClient chatClient,
        ILogger<OpenAICodeAnalysisService> logger,
        IConfiguration configuration)
    {
        _chatClient = chatClient;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Analyzes code changes and generates review comments
    /// </summary>
    public async Task<List<ReviewComment>> AnalyzeCodeChangesAsync(List<GitHubFile> files, PullRequestContext prContext)
    {
        var comments = new List<ReviewComment>();

        try
        {
            // Group files by language for more efficient analysis
            var filesByLanguage = files
                .Where(f => !string.IsNullOrEmpty(f.Patch))
                .GroupBy(f => GetFileLanguage(f.Filename))
                .ToList();

            foreach (var languageGroup in filesByLanguage)
            {
                var languageComments = await AnalyzeFilesForLanguage(languageGroup.ToList(), prContext, languageGroup.Key);
                comments.AddRange(languageComments);
            }

            _logger.LogInformation("Generated {CommentCount} review comments for {FileCount} files", 
                comments.Count, files.Count);

            return comments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing code changes");
            return new List<ReviewComment>();
        }
    }

    /// <summary>
    /// Determines if a file should be reviewed
    /// </summary>
    public bool ShouldReviewFile(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return false;

        // Check if file is in skip patterns
        if (SkipPatterns.Any(pattern => filename.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            return false;

        // Check file extension
        var extension = Path.GetExtension(filename);
        return ReviewableExtensions.Contains(extension);
    }

    /// <summary>
    /// Gets the programming language for a file
    /// </summary>
    public string GetFileLanguage(string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        
        return extension switch
        {
            ".cs" => "csharp",
            ".vb" => "vb.net",
            ".fs" => "fsharp",
            ".js" or ".jsx" => "javascript",
            ".ts" or ".tsx" => "typescript",
            ".py" or ".pyw" => "python",
            ".java" => "java",
            ".kt" => "kotlin",
            ".scala" => "scala",
            ".cpp" or ".cc" or ".cxx" => "cpp",
            ".c" => "c",
            ".h" or ".hpp" => "c_header",
            ".php" => "php",
            ".rb" => "ruby",
            ".go" => "go",
            ".rs" => "rust",
            ".swift" => "swift",
            ".sql" => "sql",
            ".xml" => "xml",
            ".json" => "json",
            ".yaml" or ".yml" => "yaml",
            ".html" => "html",
            ".css" => "css",
            ".scss" => "scss",
            ".sh" => "bash",
            ".ps1" => "powershell",
            ".bat" or ".cmd" => "batch",
            _ => "text"
        };
    }

    private async Task<List<ReviewComment>> AnalyzeFilesForLanguage(List<GitHubFile> files, PullRequestContext prContext, string language)
    {
        var comments = new List<ReviewComment>();

        try
        {
            var prompt = BuildAnalysisPrompt(files, prContext, language);
            
            var chatMessages = new List<ChatMessage>
            {
                new SystemChatMessage(GetSystemPrompt(language)),
                new UserChatMessage(prompt)
            };

            var response = await _chatClient.CompleteChatAsync(chatMessages, new ChatCompletionOptions
            {
                MaxOutputTokenCount = 2000,
                Temperature = 0.3f
            });

            var analysisResult = response.Value.Content[0].Text;
            comments.AddRange(ParseAnalysisResult(analysisResult, files));

            _logger.LogDebug("Analyzed {FileCount} {Language} files, generated {CommentCount} comments", 
                files.Count, language, comments.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing {Language} files", language);
        }

        return comments;
    }

    private string GetSystemPrompt(string language)
    {
        return $@"You are an expert {language} code reviewer. Your task is to review code changes and provide constructive feedback.

IMPORTANT GUIDELINES:
1. ONLY provide feedback on actual issues - do not comment on good code
2. Focus on: Security vulnerabilities, Performance issues, Bugs, Code quality problems
3. Be specific and actionable in your suggestions
4. Use a professional, helpful tone
5. Categorize issues as: SECURITY, PERFORMANCE, BUG, QUALITY
6. Rate severity as: CRITICAL, ERROR, WARNING, INFO

FORMAT your response as:
FILE: filename
LINE: line_number (if applicable)
CATEGORY: SECURITY|PERFORMANCE|BUG|QUALITY
SEVERITY: CRITICAL|ERROR|WARNING|INFO
COMMENT: Your specific feedback

If no issues are found, respond with: NO_ISSUES_FOUND";
    }

    private string BuildAnalysisPrompt(List<GitHubFile> files, PullRequestContext prContext, string language)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine($"Please review the following {language} code changes:");
        prompt.AppendLine($"PR Title: {prContext.Title}");
        prompt.AppendLine($"Target Branch: {prContext.TargetBranch}");
        prompt.AppendLine($"Author: {prContext.Author}");
        prompt.AppendLine();

        foreach (var file in files.Take(5)) // Limit to 5 files per request
        {
            prompt.AppendLine($"=== FILE: {file.Filename} ===");
            prompt.AppendLine($"Status: {file.Status}");
            prompt.AppendLine($"Changes: +{file.Additions} -{file.Deletions}");
            prompt.AppendLine();
            
            if (!string.IsNullOrEmpty(file.Patch))
            {
                // Limit patch size to avoid token limits
                var patch = file.Patch.Length > 2000 ? file.Patch.Substring(0, 2000) + "..." : file.Patch;
                prompt.AppendLine("DIFF:");
                prompt.AppendLine(patch);
            }
            
            prompt.AppendLine();
        }

        return prompt.ToString();
    }

    private List<ReviewComment> ParseAnalysisResult(string analysisResult, List<GitHubFile> files)
    {
        var comments = new List<ReviewComment>();

        if (analysisResult.Contains("NO_ISSUES_FOUND"))
        {
            return comments;
        }

        try
        {
            var sections = analysisResult.Split("FILE:", StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var section in sections.Skip(1)) // Skip first empty section
            {
                var comment = ParseCommentSection(section, files);
                if (comment != null)
                {
                    comments.Add(comment);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing analysis result: {Result}", analysisResult);
        }

        return comments;
    }

    private ReviewComment? ParseCommentSection(string section, List<GitHubFile> files)
    {
        try
        {
            var lines = section.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 4) return null;

            var filename = lines[0].Trim();
            var lineMatch = Regex.Match(lines[1], @"LINE:\s*(\d+)");
            var categoryMatch = Regex.Match(lines[2], @"CATEGORY:\s*(\w+)");
            var severityMatch = Regex.Match(lines[3], @"SEVERITY:\s*(\w+)");
            var commentIndex = Array.FindIndex(lines, l => l.StartsWith("COMMENT:"));
            
            if (commentIndex == -1) return null;

            var commentText = string.Join("\n", lines.Skip(commentIndex))
                .Replace("COMMENT:", "").Trim();

            // Find matching file
            var file = files.FirstOrDefault(f => f.Filename.EndsWith(filename) || filename.Contains(Path.GetFileName(f.Filename)));
            if (file == null) return null;

            var comment = new ReviewComment
            {
                FilePath = file.Filename,
                Body = FormatCommentBody(commentText, categoryMatch.Groups[1].Value, severityMatch.Groups[1].Value),
                Category = ParseCategory(categoryMatch.Groups[1].Value),
                Severity = ParseSeverity(severityMatch.Groups[1].Value)
            };

            if (lineMatch.Success && int.TryParse(lineMatch.Groups[1].Value, out var lineNumber))
            {
                comment.LineNumber = lineNumber;
            }

            return comment;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing comment section: {Section}", section);
            return null;
        }
    }

    private string FormatCommentBody(string comment, string category, string severity)
    {
        var icon = category.ToUpperInvariant() switch
        {
            "SECURITY" => "🔒",
            "PERFORMANCE" => "⚡",
            "BUG" => "🐛",
            "QUALITY" => "📝",
            _ => "💡"
        };

        var severityIcon = severity.ToUpperInvariant() switch
        {
            "CRITICAL" => "🔴",
            "ERROR" => "🟠",
            "WARNING" => "🟡",
            _ => "ℹ️"
        };

        return $"{icon} **{category}** {severityIcon}\n\n{comment}";
    }

    private ReviewCategory ParseCategory(string category)
    {
        return category.ToUpperInvariant() switch
        {
            "SECURITY" => ReviewCategory.Security,
            "PERFORMANCE" => ReviewCategory.Performance,
            "BUG" => ReviewCategory.CodeQuality,
            "QUALITY" => ReviewCategory.CodeQuality,
            _ => ReviewCategory.General
        };
    }

    private CommentSeverity ParseSeverity(string severity)
    {
        return severity.ToUpperInvariant() switch
        {
            "CRITICAL" => CommentSeverity.Critical,
            "ERROR" => CommentSeverity.Error,
            "WARNING" => CommentSeverity.Warning,
            _ => CommentSeverity.Info
        };
    }
}
