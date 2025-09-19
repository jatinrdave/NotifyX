using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for configuration operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly ILogger<ConfigController> _logger;
        private readonly IConfigService _configService;

        public ConfigController(ILogger<ConfigController> logger, IConfigService configService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        /// <summary>
        /// Gets configuration value.
        /// </summary>
        [HttpGet("{key}")]
        public async Task<IActionResult> GetConfig(string key)
        {
            try
            {
                var config = await _configService.GetConfigAsync(key);

                if (config == null)
                {
                    return NotFound(new
                    {
                        error = "Configuration not found",
                        key
                    });
                }

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get config for key {Key}: {Message}", key, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve configuration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Sets configuration value.
        /// </summary>
        [HttpPut("{key}")]
        public async Task<IActionResult> SetConfig(string key, [FromBody] SetConfigRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Configuration request is required");
                }

                await _configService.SetConfigAsync(key, request.Value, request.Description);

                return Ok(new
                {
                    message = "Configuration updated successfully",
                    key,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set config for key {Key}: {Message}", key, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update configuration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes configuration value.
        /// </summary>
        [HttpDelete("{key}")]
        public async Task<IActionResult> DeleteConfig(string key)
        {
            try
            {
                await _configService.DeleteConfigAsync(key);

                return Ok(new
                {
                    message = "Configuration deleted successfully",
                    key,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete config for key {Key}: {Message}", key, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete configuration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists all configurations.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListConfigs([FromQuery] string? prefix)
        {
            try
            {
                var configs = await _configService.ListConfigsAsync(prefix);

                return Ok(new
                {
                    configs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list configs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list configurations",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets configuration schema.
        /// </summary>
        [HttpGet("schema")]
        public async Task<IActionResult> GetConfigSchema()
        {
            try
            {
                var schema = await _configService.GetConfigSchemaAsync();

                return Ok(new
                {
                    schema
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get config schema: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve configuration schema",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Validates configuration.
        /// </summary>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateConfig([FromBody] ValidateConfigRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Validation request is required");
                }

                var result = await _configService.ValidateConfigAsync(request.Key, request.Value);

                return Ok(new
                {
                    valid = result.IsValid,
                    errors = result.Errors,
                    warnings = result.Warnings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate config: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to validate configuration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Exports configuration.
        /// </summary>
        [HttpPost("export")]
        public async Task<IActionResult> ExportConfig([FromBody] ExportConfigRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Export request is required");
                }

                var exportData = await _configService.ExportConfigAsync(request.Prefix, request.Format);

                return Ok(new
                {
                    config = exportData,
                    exportedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export config: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to export configuration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Imports configuration.
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportConfig([FromBody] ImportConfigRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Import request is required");
                }

                var importedCount = await _configService.ImportConfigAsync(request.Config, request.Overwrite);

                return Ok(new
                {
                    message = "Configuration imported successfully",
                    importedCount,
                    importedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import config: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to import configuration",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Set config request model.
    /// </summary>
    public class SetConfigRequest
    {
        /// <summary>
        /// Configuration value.
        /// </summary>
        public object Value { get; set; } = new();

        /// <summary>
        /// Configuration description.
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Validate config request model.
    /// </summary>
    public class ValidateConfigRequest
    {
        /// <summary>
        /// Configuration key.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Configuration value.
        /// </summary>
        public object Value { get; set; } = new();
    }

    /// <summary>
    /// Export config request model.
    /// </summary>
    public class ExportConfigRequest
    {
        /// <summary>
        /// Configuration prefix filter.
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        /// Export format.
        /// </summary>
        public string Format { get; set; } = "json";
    }

    /// <summary>
    /// Import config request model.
    /// </summary>
    public class ImportConfigRequest
    {
        /// <summary>
        /// Configuration data to import.
        /// </summary>
        public Dictionary<string, object> Config { get; set; } = new();

        /// <summary>
        /// Whether to overwrite existing configurations.
        /// </summary>
        public bool Overwrite { get; set; } = false;
    }
}