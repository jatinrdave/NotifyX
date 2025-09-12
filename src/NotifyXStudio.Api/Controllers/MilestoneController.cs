using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for milestone operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MilestoneController : ControllerBase
    {
        private readonly ILogger<MilestoneController> _logger;
        private readonly IMilestoneService _milestoneService;

        public MilestoneController(ILogger<MilestoneController> logger, IMilestoneService milestoneService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _milestoneService = milestoneService ?? throw new ArgumentNullException(nameof(milestoneService));
        }

        /// <summary>
        /// Creates a milestone.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMilestone([FromBody] CreateMilestoneRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Milestone request is required");
                }

                var milestoneId = await _milestoneService.CreateMilestoneAsync(
                    request.ProjectId,
                    request.Title,
                    request.Description,
                    request.DueDate,
                    request.MilestoneType,
                    request.Metadata);

                return Ok(new
                {
                    milestoneId,
                    message = "Milestone created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create milestone: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create milestone",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets milestone information.
        /// </summary>
        [HttpGet("{milestoneId}")]
        public async Task<IActionResult> GetMilestone(string milestoneId)
        {
            try
            {
                var milestone = await _milestoneService.GetMilestoneAsync(milestoneId);

                if (milestone == null)
                {
                    return NotFound(new
                    {
                        error = "Milestone not found",
                        milestoneId
                    });
                }

                return Ok(milestone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get milestone {MilestoneId}: {Message}", milestoneId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve milestone",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists milestones.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListMilestones(
            [FromQuery] string? projectId,
            [FromQuery] string? milestoneType,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var milestones = await _milestoneService.ListMilestonesAsync(projectId, milestoneType, status, page, pageSize);
                var totalCount = await _milestoneService.GetMilestoneCountAsync(projectId, milestoneType, status);

                return Ok(new
                {
                    milestones,
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
                _logger.LogError(ex, "Failed to list milestones: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list milestones",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a milestone.
        /// </summary>
        [HttpPut("{milestoneId}")]
        public async Task<IActionResult> UpdateMilestone(
            string milestoneId,
            [FromBody] UpdateMilestoneRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _milestoneService.UpdateMilestoneAsync(
                    milestoneId,
                    request.Title,
                    request.Description,
                    request.DueDate,
                    request.MilestoneType,
                    request.Status,
                    request.Metadata);

                return Ok(new
                {
                    message = "Milestone updated successfully",
                    milestoneId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update milestone {MilestoneId}: {Message}", milestoneId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update milestone",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a milestone.
        /// </summary>
        [HttpDelete("{milestoneId}")]
        public async Task<IActionResult> DeleteMilestone(string milestoneId)
        {
            try
            {
                await _milestoneService.DeleteMilestoneAsync(milestoneId);

                return Ok(new
                {
                    message = "Milestone deleted successfully",
                    milestoneId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete milestone {MilestoneId}: {Message}", milestoneId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete milestone",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets milestone status.
        /// </summary>
        [HttpGet("{milestoneId}/status")]
        public async Task<IActionResult> GetMilestoneStatus(string milestoneId)
        {
            try
            {
                var status = await _milestoneService.GetMilestoneStatusAsync(milestoneId);

                return Ok(new
                {
                    milestoneId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get milestone status for {MilestoneId}: {Message}", milestoneId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve milestone status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets milestone issues.
        /// </summary>
        [HttpGet("{milestoneId}/issues")]
        public async Task<IActionResult> GetMilestoneIssues(string milestoneId)
        {
            try
            {
                var issues = await _milestoneService.GetMilestoneIssuesAsync(milestoneId);

                return Ok(new
                {
                    milestoneId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get milestone issues for {MilestoneId}: {Message}", milestoneId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve milestone issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets milestone statistics.
        /// </summary>
        [HttpGet("{milestoneId}/stats")]
        public async Task<IActionResult> GetMilestoneStats(string milestoneId)
        {
            try
            {
                var stats = await _milestoneService.GetMilestoneStatsAsync(milestoneId);

                return Ok(new
                {
                    milestoneId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get milestone stats for {MilestoneId}: {Message}", milestoneId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve milestone statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available milestone types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetMilestoneTypes()
        {
            try
            {
                var milestoneTypes = await _milestoneService.GetMilestoneTypesAsync();

                return Ok(new
                {
                    milestoneTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get milestone types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve milestone types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create milestone request model.
    /// </summary>
    public class CreateMilestoneRequest
    {
        /// <summary>
        /// Project ID.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Milestone title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Milestone description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Milestone due date.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Milestone type.
        /// </summary>
        public string MilestoneType { get; set; } = "release";

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update milestone request model.
    /// </summary>
    public class UpdateMilestoneRequest
    {
        /// <summary>
        /// Milestone title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Milestone description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Milestone due date.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Milestone type.
        /// </summary>
        public string? MilestoneType { get; set; }

        /// <summary>
        /// Milestone status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}