using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for issue operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class IssueController : ControllerBase
    {
        private readonly ILogger<IssueController> _logger;
        private readonly IIssueService _issueService;

        public IssueController(ILogger<IssueController> logger, IIssueService issueService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _issueService = issueService ?? throw new ArgumentNullException(nameof(issueService));
        }

        /// <summary>
        /// Creates an issue.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateIssue([FromBody] CreateIssueRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Issue request is required");
                }

                var issueId = await _issueService.CreateIssueAsync(
                    request.ProjectId,
                    request.Title,
                    request.Description,
                    request.IssueType,
                    request.Priority,
                    request.Assignee,
                    request.Metadata);

                return Ok(new
                {
                    issueId,
                    message = "Issue created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create issue: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create issue",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets issue information.
        /// </summary>
        [HttpGet("{issueId}")]
        public async Task<IActionResult> GetIssue(string issueId)
        {
            try
            {
                var issue = await _issueService.GetIssueAsync(issueId);

                if (issue == null)
                {
                    return NotFound(new
                    {
                        error = "Issue not found",
                        issueId
                    });
                }

                return Ok(issue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get issue {IssueId}: {Message}", issueId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve issue",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists issues.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListIssues(
            [FromQuery] string? projectId,
            [FromQuery] string? issueType,
            [FromQuery] string? priority,
            [FromQuery] string? status,
            [FromQuery] string? assignee,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var issues = await _issueService.ListIssuesAsync(projectId, issueType, priority, status, assignee, page, pageSize);
                var totalCount = await _issueService.GetIssueCountAsync(projectId, issueType, priority, status, assignee);

                return Ok(new
                {
                    issues,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list issues: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates an issue.
        /// </summary>
        [HttpPut("{issueId}")]
        public async Task<IActionResult> UpdateIssue(
            string issueId,
            [FromBody] UpdateIssueRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _issueService.UpdateIssueAsync(
                    issueId,
                    request.Title,
                    request.Description,
                    request.IssueType,
                    request.Priority,
                    request.Status,
                    request.Assignee,
                    request.Metadata);

                return Ok(new
                {
                    message = "Issue updated successfully",
                    issueId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update issue {IssueId}: {Message}", issueId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update issue",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes an issue.
        /// </summary>
        [HttpDelete("{issueId}")]
        public async Task<IActionResult> DeleteIssue(string issueId)
        {
            try
            {
                await _issueService.DeleteIssueAsync(issueId);

                return Ok(new
                {
                    message = "Issue deleted successfully",
                    issueId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete issue {IssueId}: {Message}", issueId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete issue",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets issue status.
        /// </summary>
        [HttpGet("{issueId}/status")]
        public async Task<IActionResult> GetIssueStatus(string issueId)
        {
            try
            {
                var status = await _issueService.GetIssueStatusAsync(issueId);

                return Ok(new
                {
                    issueId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get issue status for {IssueId}: {Message}", issueId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve issue status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets issue comments.
        /// </summary>
        [HttpGet("{issueId}/comments")]
        public async Task<IActionResult> GetIssueComments(string issueId)
        {
            try
            {
                var comments = await _issueService.GetIssueCommentsAsync(issueId);

                return Ok(new
                {
                    issueId,
                    comments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get issue comments for {IssueId}: {Message}", issueId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve issue comments",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets issue attachments.
        /// </summary>
        [HttpGet("{issueId}/attachments")]
        public async Task<IActionResult> GetIssueAttachments(string issueId)
        {
            try
            {
                var attachments = await _issueService.GetIssueAttachmentsAsync(issueId);

                return Ok(new
                {
                    issueId,
                    attachments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get issue attachments for {IssueId}: {Message}", issueId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve issue attachments",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets issue statistics.
        /// </summary>
        [HttpGet("{issueId}/stats")]
        public async Task<IActionResult> GetIssueStats(string issueId)
        {
            try
            {
                var stats = await _issueService.GetIssueStatsAsync(issueId);

                return Ok(new
                {
                    issueId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get issue stats for {IssueId}: {Message}", issueId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve issue statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available issue types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetIssueTypes()
        {
            try
            {
                var issueTypes = await _issueService.GetIssueTypesAsync();

                return Ok(new
                {
                    issueTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get issue types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve issue types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create issue request model.
    /// </summary>
    public class CreateIssueRequest
    {
        /// <summary>
        /// Project ID.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Issue title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Issue description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Issue type.
        /// </summary>
        public string IssueType { get; set; } = "bug";

        /// <summary>
        /// Issue priority.
        /// </summary>
        public string Priority { get; set; } = "medium";

        /// <summary>
        /// Issue assignee.
        /// </summary>
        public string? Assignee { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update issue request model.
    /// </summary>
    public class UpdateIssueRequest
    {
        /// <summary>
        /// Issue title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Issue description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Issue type.
        /// </summary>
        public string? IssueType { get; set; }

        /// <summary>
        /// Issue priority.
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Issue status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Issue assignee.
        /// </summary>
        public string? Assignee { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}