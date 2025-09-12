using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for version operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VersionController : ControllerBase
    {
        private readonly ILogger<VersionController> _logger;
        private readonly IVersionService _versionService;

        public VersionController(ILogger<VersionController> logger, IVersionService versionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _versionService = versionService ?? throw new ArgumentNullException(nameof(versionService));
        }

        /// <summary>
        /// Gets application version information.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVersion()
        {
            try
            {
                var version = await _versionService.GetVersionAsync();

                return Ok(new
                {
                    version,
                    retrievedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get version: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve version information",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets component versions.
        /// </summary>
        [HttpGet("components")]
        public async Task<IActionResult> GetComponentVersions()
        {
            try
            {
                var componentVersions = await _versionService.GetComponentVersionsAsync();

                return Ok(new
                {
                    componentVersions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get component versions: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve component versions",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets dependency versions.
        /// </summary>
        [HttpGet("dependencies")]
        public async Task<IActionResult> GetDependencyVersions()
        {
            try
            {
                var dependencyVersions = await _versionService.GetDependencyVersionsAsync();

                return Ok(new
                {
                    dependencyVersions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get dependency versions: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve dependency versions",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets build information.
        /// </summary>
        [HttpGet("build")]
        public async Task<IActionResult> GetBuildInfo()
        {
            try
            {
                var buildInfo = await _versionService.GetBuildInfoAsync();

                return Ok(new
                {
                    buildInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get build info: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve build information",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets runtime information.
        /// </summary>
        [HttpGet("runtime")]
        public async Task<IActionResult> GetRuntimeInfo()
        {
            try
            {
                var runtimeInfo = await _versionService.GetRuntimeInfoAsync();

                return Ok(new
                {
                    runtimeInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get runtime info: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve runtime information",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets environment information.
        /// </summary>
        [HttpGet("environment")]
        public async Task<IActionResult> GetEnvironmentInfo()
        {
            try
            {
                var environmentInfo = await _versionService.GetEnvironmentInfoAsync();

                return Ok(new
                {
                    environmentInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get environment info: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve environment information",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets update information.
        /// </summary>
        [HttpGet("updates")]
        public async Task<IActionResult> GetUpdateInfo()
        {
            try
            {
                var updateInfo = await _versionService.GetUpdateInfoAsync();

                return Ok(new
                {
                    updateInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get update info: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve update information",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Checks for updates.
        /// </summary>
        [HttpPost("check-updates")]
        public async Task<IActionResult> CheckForUpdates()
        {
            try
            {
                var updateCheck = await _versionService.CheckForUpdatesAsync();

                return Ok(new
                {
                    updateCheck,
                    checkedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check for updates: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to check for updates",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets version history.
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetVersionHistory()
        {
            try
            {
                var versionHistory = await _versionService.GetVersionHistoryAsync();

                return Ok(new
                {
                    versionHistory
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get version history: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve version history",
                    message = ex.Message
                });
            }
        }
    }
}