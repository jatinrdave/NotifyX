using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Core.Services;
using System.Text.Json;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for managing connectors and their manifests.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConnectorsController : ControllerBase
    {
        private readonly IConnectorRegistryService _registryService;
        private readonly IConnectorResolver _resolver;
        private readonly ILogger<ConnectorsController> _logger;

        public ConnectorsController(
            IConnectorRegistryService registryService,
            IConnectorResolver resolver,
            ILogger<ConnectorsController> logger)
        {
            _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Lists all available connectors from the registry.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConnectorRegistryEntry>>> ListConnectors(
            [FromQuery] string? category = null,
            [FromQuery] string? search = null,
            [FromQuery] List<string>? tags = null)
        {
            try
            {
                var registry = await _registryService.GetRegistryAsync();
                var connectors = registry.Connectors.AsEnumerable();

                if (!string.IsNullOrEmpty(category))
                {
                    connectors = connectors.Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    connectors = connectors.Where(c => 
                        c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        c.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                if (tags?.Any() == true)
                {
                    connectors = connectors.Where(c => tags.Any(tag => c.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
                }

                return Ok(connectors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list connectors");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets a specific connector manifest by ID.
        /// </summary>
        [HttpGet("{connectorId}")]
        public async Task<ActionResult<ConnectorManifest>> GetConnector(string connectorId)
        {
            try
            {
                var manifest = await _registryService.GetManifestAsync(connectorId);
                
                if (manifest == null)
                {
                    return NotFound();
                }

                return Ok(manifest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get connector {ConnectorId}", connectorId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets connector categories for grouping in UI.
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            try
            {
                var registry = await _registryService.GetRegistryAsync();
                var categories = registry.Connectors
                    .Select(c => c.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get connector categories");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Resolves dependencies for a set of connectors.
        /// </summary>
        [HttpPost("resolve")]
        public async Task<ActionResult<ResolutionResult>> ResolveDependencies([FromBody] ResolveDependenciesRequest request)
        {
            try
            {
                var result = await _resolver.ResolveAsync(request.RequestedConnectors, request.ResolutionStrategy, request.Lockfile);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve dependencies");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets available versions for a specific connector.
        /// </summary>
        [HttpGet("{connectorId}/versions")]
        public async Task<ActionResult<IEnumerable<ConnectorVersion>>> GetConnectorVersions(string connectorId)
        {
            try
            {
                var versions = await _registryService.GetConnectorVersionsAsync(connectorId);
                return Ok(versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get versions for connector {ConnectorId}", connectorId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Validates a connector manifest.
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<ValidationResult>> ValidateManifest([FromBody] ConnectorManifest manifest)
        {
            try
            {
                var result = await _registryService.ValidateManifestAsync(manifest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate manifest");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Tests a connector configuration.
        /// </summary>
        [HttpPost("{connectorId}/test")]
        public async Task<ActionResult<ConnectorTestResult>> TestConnector(string connectorId, [FromBody] ConnectorTestRequest request)
        {
            try
            {
                var result = await _registryService.TestConnectorAsync(connectorId, request.Config, request.Credentials);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test connector {ConnectorId}", connectorId);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    /// <summary>
    /// Request to resolve connector dependencies.
    /// </summary>
    public class ResolveDependenciesRequest
    {
        public List<DependencySpec> RequestedConnectors { get; init; } = new();
        public ResolutionStrategy ResolutionStrategy { get; init; } = ResolutionStrategy.HighestCompatible;
        public Dictionary<string, string>? Lockfile { get; init; }
    }

    /// <summary>
    /// Connector version information.
    /// </summary>
    public class ConnectorVersion
    {
        public string Version { get; init; } = string.Empty;
        public DateTime PublishedAt { get; init; }
        public string Description { get; init; } = string.Empty;
        public List<string> Changes { get; init; } = new();
        public bool IsLatest { get; init; }
        public bool IsStable { get; init; }
    }

    /// <summary>
    /// Result of manifest validation.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; init; }
        public List<string> Errors { get; init; } = new();
        public List<string> Warnings { get; init; } = new();
    }

    /// <summary>
    /// Request to test a connector.
    /// </summary>
    public class ConnectorTestRequest
    {
        public JsonElement Config { get; init; }
        public JsonElement? Credentials { get; init; }
    }

    /// <summary>
    /// Result of connector testing.
    /// </summary>
    public class ConnectorTestResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public JsonElement? Output { get; init; }
        public long DurationMs { get; init; }
    }
}