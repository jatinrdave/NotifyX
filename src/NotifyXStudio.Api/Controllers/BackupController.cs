using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for backup and restore operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly ILogger<BackupController> _logger;
        private readonly IBackupService _backupService;

        public BackupController(ILogger<BackupController> logger, IBackupService backupService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        }

        /// <summary>
        /// Creates a backup of the system.
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateBackup([FromBody] CreateBackupRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Backup request is required");
                }

                var backupId = await _backupService.CreateBackupAsync(
                    request.TenantId,
                    request.BackupType,
                    request.IncludeData,
                    request.IncludeCredentials,
                    request.IncludeLogs);

                return Ok(new
                {
                    backupId,
                    message = "Backup created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create backup: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create backup",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets backup information.
        /// </summary>
        [HttpGet("{backupId}")]
        public async Task<IActionResult> GetBackupInfo(string backupId)
        {
            try
            {
                var backupInfo = await _backupService.GetBackupInfoAsync(backupId);

                if (backupInfo == null)
                {
                    return NotFound(new
                    {
                        error = "Backup not found",
                        backupId
                    });
                }

                return Ok(backupInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get backup info for {BackupId}: {Message}", backupId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve backup information",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists available backups.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListBackups(
            [FromQuery] Guid? tenantId,
            [FromQuery] string? backupType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var backups = await _backupService.ListBackupsAsync(tenantId, backupType, page, pageSize);
                var totalCount = await _backupService.GetBackupCountAsync(tenantId, backupType);

                return Ok(new
                {
                    backups,
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
                _logger.LogError(ex, "Failed to list backups: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list backups",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Restores from a backup.
        /// </summary>
        [HttpPost("restore/{backupId}")]
        public async Task<IActionResult> RestoreFromBackup(
            string backupId,
            [FromBody] RestoreBackupRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Restore request is required");
                }

                await _backupService.RestoreFromBackupAsync(
                    backupId,
                    request.TenantId,
                    request.RestoreType,
                    request.IncludeData,
                    request.IncludeCredentials,
                    request.IncludeLogs);

                return Ok(new
                {
                    message = "Restore completed successfully",
                    backupId,
                    restoredAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore from backup {BackupId}: {Message}", backupId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to restore from backup",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a backup.
        /// </summary>
        [HttpDelete("{backupId}")]
        public async Task<IActionResult> DeleteBackup(string backupId)
        {
            try
            {
                await _backupService.DeleteBackupAsync(backupId);

                return Ok(new
                {
                    message = "Backup deleted successfully",
                    backupId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete backup {BackupId}: {Message}", backupId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete backup",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Downloads a backup.
        /// </summary>
        [HttpGet("{backupId}/download")]
        public async Task<IActionResult> DownloadBackup(string backupId)
        {
            try
            {
                var backupStream = await _backupService.DownloadBackupAsync(backupId);

                if (backupStream == null)
                {
                    return NotFound(new
                    {
                        error = "Backup not found",
                        backupId
                    });
                }

                return File(backupStream, "application/zip", $"backup_{backupId}.zip");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download backup {BackupId}: {Message}", backupId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to download backup",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create backup request model.
    /// </summary>
    public class CreateBackupRequest
    {
        /// <summary>
        /// Tenant ID for the backup.
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Type of backup to create.
        /// </summary>
        public string BackupType { get; set; } = "full";

        /// <summary>
        /// Whether to include data in the backup.
        /// </summary>
        public bool IncludeData { get; set; } = true;

        /// <summary>
        /// Whether to include credentials in the backup.
        /// </summary>
        public bool IncludeCredentials { get; set; } = false;

        /// <summary>
        /// Whether to include logs in the backup.
        /// </summary>
        public bool IncludeLogs { get; set; } = true;
    }

    /// <summary>
    /// Restore backup request model.
    /// </summary>
    public class RestoreBackupRequest
    {
        /// <summary>
        /// Tenant ID to restore to.
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Type of restore to perform.
        /// </summary>
        public string RestoreType { get; set; } = "full";

        /// <summary>
        /// Whether to include data in the restore.
        /// </summary>
        public bool IncludeData { get; set; } = true;

        /// <summary>
        /// Whether to include credentials in the restore.
        /// </summary>
        public bool IncludeCredentials { get; set; } = false;

        /// <summary>
        /// Whether to include logs in the restore.
        /// </summary>
        public bool IncludeLogs { get; set; } = true;
    }
}