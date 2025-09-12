using NotifyXStudio.Core.Models;
using NotifyXStudio.Core.Resolvers;

namespace NotifyXStudio.Core.Services
{
    /// <summary>
    /// Service for managing connector registry and manifests.
    /// </summary>
    public interface IConnectorRegistryService
    {
        /// <summary>
        /// Gets the complete connector registry.
        /// </summary>
        Task<ConnectorRegistry> GetRegistryAsync();

        /// <summary>
        /// Gets a specific connector manifest by ID.
        /// </summary>
        Task<ConnectorManifest?> GetManifestAsync(string connectorId);

        /// <summary>
        /// Gets a specific connector manifest by ID and version.
        /// </summary>
        Task<ConnectorManifest?> GetManifestAsync(string connectorId, string version);

        /// <summary>
        /// Gets available versions for a specific connector.
        /// </summary>
        Task<IEnumerable<ConnectorVersion>> GetConnectorVersionsAsync(string connectorId);

        /// <summary>
        /// Validates a connector manifest.
        /// </summary>
        Task<ValidationResult> ValidateManifestAsync(ConnectorManifest manifest);

        /// <summary>
        /// Tests a connector configuration.
        /// </summary>
        Task<ConnectorTestResult> TestConnectorAsync(string connectorId, System.Text.Json.JsonElement config, System.Text.Json.JsonElement? credentials = null);

        /// <summary>
        /// Refreshes the registry from remote sources.
        /// </summary>
        Task RefreshRegistryAsync();

        /// <summary>
        /// Adds a custom connector to the registry.
        /// </summary>
        Task AddCustomConnectorAsync(ConnectorManifest manifest, string tenantId);

        /// <summary>
        /// Removes a custom connector from the registry.
        /// </summary>
        Task RemoveCustomConnectorAsync(string connectorId, string tenantId);

        /// <summary>
        /// Gets custom connectors for a tenant.
        /// </summary>
        Task<IEnumerable<ConnectorRegistryEntry>> GetCustomConnectorsAsync(string tenantId);
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
    /// Result of connector testing.
    /// </summary>
    public class ConnectorTestResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public System.Text.Json.JsonElement? Output { get; init; }
        public long DurationMs { get; init; }
    }
}