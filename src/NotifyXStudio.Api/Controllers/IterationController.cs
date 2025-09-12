using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for iteration operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class IterationController : ControllerBase
    {
        private readonly ILogger<IterationController> _logger;
        private readonly IIterationService _iterationService;

        public IterationController(ILogger<IterationController> logger, IIterationService iterationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _iterationService = iterationService ?? throw new ArgumentNullException(nameof(iterationService));
        }

        /// <summary>
        /// Creates an iteration.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateIteration([FromBody] CreateIterationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Iteration request is required");
                }

                var iterationId = await _iterationService.CreateIterationAsync(
                    request.ProjectId,
                    request.Title,
                    request.Description,
                    request.StartDate,
                    request.EndDate,
                    request.IterationType,
                    request.Metadata);

                return Ok(new
                {
                    iterationId,
                    message = "Iteration created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create iteration: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create iteration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets iteration information.
        /// </summary>
        [HttpGet("{iterationId}")]
        public async Task<IActionResult> GetIteration(string iterationId)
        {
            try
            {
                var iteration = await _iterationService.GetIterationAsync(iterationId);

                if (iteration == null)
                {
                    return NotFound(new
                    {
                        error = "Iteration not found",
                        iterationId
                    });
                }

                return Ok(iteration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get iteration {IterationId}: {Message}", iterationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve iteration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists iterations.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListIterations(
            [FromQuery] string? projectId,
            [FromQuery] string? iterationType,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var iterations = await _iterationService.ListIterationsAsync(projectId, iterationType, status, page, pageSize);
                var totalCount = await _iterationService.GetIterationCountAsync(projectId, iterationType, status);

                return Ok(new
                {
                    iterations,
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
                _logger.LogError(ex, "Failed to list iterations: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list iterations",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates an iteration.
        /// </summary>
        [HttpPut("{iterationId}")]
        public async Task<IActionResult> UpdateIteration(
            string iterationId,
            [FromBody] UpdateIterationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _iterationService.UpdateIterationAsync(
                    iterationId,
                    request.Title,
                    request.Description,
                    request.StartDate,
                    request.EndDate,
                    request.IterationType,
                    request.Status,
                    request.Metadata);

                return Ok(new
                {
                    message = "Iteration updated successfully",
                    iterationId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update iteration {IterationId}: {Message}", iterationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update iteration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes an iteration.
        /// </summary>
        [HttpDelete("{iterationId}")]
        public async Task<IActionResult> DeleteIteration(string iterationId)
        {
            try
            {
                await _iterationService.DeleteIterationAsync(iterationId);

                return Ok(new
                {
                    message = "Iteration deleted successfully",
                    iterationId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete iteration {IterationId}: {Message}", iterationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete iteration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets iteration status.
        /// </summary>
        [HttpGet("{iterationId}/status")]
        public async Task<IActionResult> GetIterationStatus(string iterationId)
        {
            try
            {
                var status = await _iterationService.GetIterationStatusAsync(iterationId);

                return Ok(new
                {
                    iterationId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get iteration status for {IterationId}: {Message}", iterationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve iteration status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets iteration issues.
        /// </summary>
        [HttpGet("{iterationId}/issues")]
        public async Task<IActionResult> GetIterationIssues(string iterationId)
        {
            try
            {
                var issues = await _iterationService.GetIterationIssuesAsync(iterationId);

                return Ok(new
                {
                    iterationId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get iteration issues for {IterationId}: {Message}", iterationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve iteration issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets iteration statistics.
        /// </summary>
        [HttpGet("{iterationId}/stats")]
        public async Task<IActionResult> GetIterationStats(string iterationId)
        {
            try
            {
                var stats = await _iterationService.GetIterationStatsAsync(iterationId);

                return Ok(new
                {
                    iterationId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get iteration stats for {IterationId}: {Message}", iterationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve iteration statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available iteration types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetIterationTypes()
        {
            try
            {
                var iterationTypes = await _iterationService.GetIterationTypesAsync();

                return Ok(new
                {
                    iterationTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get iteration types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve iteration types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create iteration request model.
    /// </summary>
    public class CreateIterationRequest
    {
        /// <summary>
        /// Project ID.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Iteration title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Iteration description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Iteration start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Iteration end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Iteration type.
        /// </summary>
        public string IterationType { get; set; } = "sprint";

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update iteration request model.
    /// </summary>
    public class UpdateIterationRequest
    {
        /// <summary>
        /// Iteration title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Iteration description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Iteration start date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Iteration end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Iteration type.
        /// </summary>
        public string? IterationType { get; set; }

        /// <summary>
        /// Iteration status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}