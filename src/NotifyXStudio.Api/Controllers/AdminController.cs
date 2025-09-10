using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for administrative operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IWorkflowService _workflowService;
        private readonly IRunService _runService;
        private readonly ICredentialService _credentialService;

        public AdminController(
            ILogger<AdminController> logger,
            IWorkflowService workflowService,
            IRunService runService,
            ICredentialService credentialService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _runService = runService ?? throw new ArgumentNullException(nameof(runService));
            _credentialService = credentialService ?? throw new ArgumentNullException(nameof(credentialService));
        }

        /// <summary>
        /// Gets system statistics.
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetSystemStats()
        {
            try
            {
                var stats = new
                {
                    timestamp = DateTime.UtcNow,
                    workflows = new
                    {
                        total = await _workflowService.GetWorkflowCountAsync(),
                        active = await _workflowService.GetActiveWorkflowCountAsync()
                    },
                    runs = new
                    {
                        total = await _runService.GetRunCountAsync(),
                        active = await _runService.GetActiveRunCountAsync(),
                        completed = await _runService.GetCompletedRunCountAsync(),
                        failed = await _runService.GetFailedRunCountAsync()
                    },
                    credentials = new
                    {
                        total = await _credentialService.GetCredentialCountAsync()
                    }
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system stats: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve system statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Cleans up old data.
        /// </summary>
        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupOldData([FromQuery] int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                
                // Clean up old runs
                var deletedRuns = await _runService.DeleteOldRunsAsync(cutoffDate);
                
                // Clean up old logs
                var deletedLogs = await _runService.DeleteOldLogsAsync(cutoffDate);

                return Ok(new
                {
                    message = "Cleanup completed successfully",
                    deletedRuns,
                    deletedLogs,
                    cutoffDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old data: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to cleanup old data",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Exports system data.
        /// </summary>
        [HttpPost("export")]
        public async Task<IActionResult> ExportSystemData([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                // Export workflows
                var workflows = await _workflowService.GetWorkflowsAsync(start, end);
                
                // Export runs
                var runs = await _runService.GetRunsAsync(start, end);
                
                // Export credentials (without sensitive data)
                var credentials = await _credentialService.GetCredentialsAsync();

                var exportData = new
                {
                    exportDate = DateTime.UtcNow,
                    dateRange = new { start, end },
                    workflows,
                    runs,
                    credentials = credentials.Select(c => new
                    {
                        c.Id,
                        c.TenantId,
                        c.ConnectorType,
                        c.CreatedAt,
                        c.UpdatedAt
                        // Exclude sensitive data
                    })
                };

                return Ok(exportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export system data: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to export system data",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Imports system data.
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportSystemData([FromBody] ImportDataRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Import data is required");
                }

                var importedCount = 0;

                // Import workflows
                if (request.Workflows != null)
                {
                    foreach (var workflow in request.Workflows)
                    {
                        await _workflowService.CreateWorkflowAsync(workflow);
                        importedCount++;
                    }
                }

                // Import credentials (without sensitive data)
                if (request.Credentials != null)
                {
                    foreach (var credential in request.Credentials)
                    {
                        // Note: In a real implementation, you'd need to handle credential secrets
                        // This is a simplified version that only imports metadata
                        await _credentialService.CreateCredentialAsync(credential);
                        importedCount++;
                    }
                }

                return Ok(new
                {
                    message = "Import completed successfully",
                    importedCount,
                    importedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import system data: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to import system data",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Import data request model.
    /// </summary>
    public class ImportDataRequest
    {
        /// <summary>
        /// Workflows to import.
        /// </summary>
        public List<Core.Models.Workflow>? Workflows { get; set; }

        /// <summary>
        /// Credentials to import.
        /// </summary>
        public List<Core.Models.Credential>? Credentials { get; set; }
    }
}