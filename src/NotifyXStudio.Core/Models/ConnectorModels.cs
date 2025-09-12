using System.Text.Json;

namespace NotifyXStudio.Core.Models
{
    /// <summary>
    /// Connector manifest describing a node's capabilities and configuration.
    /// </summary>
    public class ConnectorManifest
    {
        /// <summary>Schema version for validation.</summary>
        public string Schema { get; init; } = "https://notifyx.dev/schemas/connector-manifest.json";

        /// <summary>Unique connector identifier.</summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>Human-readable connector name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Semantic version of the connector.</summary>
        public string Version { get; init; } = string.Empty;

        /// <summary>Node type: trigger, action, or transform.</summary>
        public ConnectorType Type { get; init; }

        /// <summary>Category for grouping in UI.</summary>
        public string Category { get; init; } = string.Empty;

        /// <summary>Description of what the connector does.</summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>Icon URL for UI display.</summary>
        public string Icon { get; init; } = string.Empty;

        /// <summary>Input parameters definition.</summary>
        public List<ConnectorInput> Inputs { get; init; } = new();

        /// <summary>Output parameters definition.</summary>
        public List<ConnectorOutput> Outputs { get; init; } = new();

        /// <summary>Authentication configuration.</summary>
        public ConnectorAuth Auth { get; init; } = new();

        /// <summary>UI configuration for the node.</summary>
        public ConnectorUI UI { get; init; } = new();

        /// <summary>Additional metadata.</summary>
        public ConnectorMetadata Metadata { get; init; } = new();
    }

    /// <summary>
    /// Input parameter definition for a connector.
    /// </summary>
    public class ConnectorInput
    {
        /// <summary>Parameter name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Parameter type.</summary>
        public string Type { get; init; } = string.Empty;

        /// <summary>Whether the parameter is required.</summary>
        public bool Required { get; init; }

        /// <summary>Default value if not provided.</summary>
        public JsonElement? Default { get; init; }

        /// <summary>Description of the parameter.</summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>Validation rules for the parameter.</summary>
        public Dictionary<string, object> Validation { get; init; } = new();
    }

    /// <summary>
    /// Output parameter definition for a connector.
    /// </summary>
    public class ConnectorOutput
    {
        /// <summary>Output name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Output type.</summary>
        public string Type { get; init; } = string.Empty;

        /// <summary>Description of the output.</summary>
        public string Description { get; init; } = string.Empty;
    }

    /// <summary>
    /// Authentication configuration for a connector.
    /// </summary>
    public class ConnectorAuth
    {
        /// <summary>Authentication type.</summary>
        public AuthType Type { get; init; }

        /// <summary>Authentication fields configuration.</summary>
        public Dictionary<string, string> Fields { get; init; } = new();
    }

    /// <summary>
    /// UI configuration for a connector node.
    /// </summary>
    public class ConnectorUI
    {
        /// <summary>Node color in hex format.</summary>
        public string Color { get; init; } = "#4F46E5";

        /// <summary>Icon shape for the node.</summary>
        public IconShape IconShape { get; init; } = IconShape.Circle;

        /// <summary>Group name for organizing in palette.</summary>
        public string Group { get; init; } = string.Empty;
    }

    /// <summary>
    /// Additional metadata for a connector.
    /// </summary>
    public class ConnectorMetadata
    {
        /// <summary>Tags for categorization and search.</summary>
        public List<string> Tags { get; init; } = new();

        /// <summary>Documentation URL.</summary>
        public string DocumentationUrl { get; init; } = string.Empty;

        /// <summary>Creator of the connector.</summary>
        public string CreatedBy { get; init; } = string.Empty;
    }

    /// <summary>
    /// Connector registry containing multiple connector manifests.
    /// </summary>
    public class ConnectorRegistry
    {
        /// <summary>Registry schema version.</summary>
        public string Schema { get; init; } = "https://notifyx.dev/schemas/connector-registry.json";

        /// <summary>Registry version.</summary>
        public string RegistryVersion { get; init; } = "1.2.0";

        /// <summary>Last update timestamp.</summary>
        public DateTime LastUpdated { get; init; }

        /// <summary>List of available connectors.</summary>
        public List<ConnectorRegistryEntry> Connectors { get; init; } = new();
    }

    /// <summary>
    /// Entry in the connector registry.
    /// </summary>
    public class ConnectorRegistryEntry
    {
        /// <summary>Connector identifier.</summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>Connector name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Connector description.</summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>Connector version.</summary>
        public string Version { get; init; } = string.Empty;

        /// <summary>Connector category.</summary>
        public string Category { get; init; } = string.Empty;

        /// <summary>URL to the connector manifest.</summary>
        public string ManifestUrl { get; init; } = string.Empty;

        /// <summary>Icon URL.</summary>
        public string Icon { get; init; } = string.Empty;

        /// <summary>Tags for categorization.</summary>
        public List<string> Tags { get; init; } = new();

        /// <summary>Author information.</summary>
        public ConnectorAuthor Author { get; init; } = new();

        /// <summary>Compatibility requirements.</summary>
        public ConnectorCompatibility Compatibility { get; init; } = new();

        /// <summary>Dependencies and requirements.</summary>
        public ConnectorDependencies Dependencies { get; init; } = new();

        /// <summary>Conflict resolution rules.</summary>
        public ConnectorConflictRules ConflictRules { get; init; } = new();
    }

    /// <summary>
    /// Author information for a connector.
    /// </summary>
    public class ConnectorAuthor
    {
        /// <summary>Author name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Author URL.</summary>
        public string Url { get; init; } = string.Empty;
    }

    /// <summary>
    /// Compatibility requirements for a connector.
    /// </summary>
    public class ConnectorCompatibility
    {
        /// <summary>Required frontend versions.</summary>
        public List<string> Frontend { get; init; } = new();

        /// <summary>Required backend versions.</summary>
        public List<string> Backend { get; init; } = new();

        /// <summary>Required NotifyX core versions.</summary>
        public List<string> NotifyXCore { get; init; } = new();
    }

    /// <summary>
    /// Dependencies for a connector.
    /// </summary>
    public class ConnectorDependencies
    {
        /// <summary>Runtime dependencies.</summary>
        public ConnectorRuntimeDependencies Runtime { get; init; } = new();

        /// <summary>Peer connector dependencies.</summary>
        public List<string> Peer { get; init; } = new();

        /// <summary>External API dependencies.</summary>
        public List<ApiDependency> Apis { get; init; } = new();
    }

    /// <summary>
    /// Runtime dependencies for a connector.
    /// </summary>
    public class ConnectorRuntimeDependencies
    {
        /// <summary>NPM package dependencies.</summary>
        public NpmDependency? Npm { get; init; }

        /// <summary>NuGet package dependencies.</summary>
        public NuGetDependency? Nuget { get; init; }
    }

    /// <summary>
    /// NPM package dependency.
    /// </summary>
    public class NpmDependency
    {
        /// <summary>Package name.</summary>
        public string PackageName { get; init; } = string.Empty;

        /// <summary>Version range.</summary>
        public string Version { get; init; } = string.Empty;
    }

    /// <summary>
    /// NuGet package dependency.
    /// </summary>
    public class NuGetDependency
    {
        /// <summary>Package name.</summary>
        public string PackageName { get; init; } = string.Empty;

        /// <summary>Version range.</summary>
        public string Version { get; init; } = string.Empty;
    }

    /// <summary>
    /// External API dependency.
    /// </summary>
    public class ApiDependency
    {
        /// <summary>API name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>API URL.</summary>
        public string Url { get; init; } = string.Empty;

        /// <summary>Required authentication type.</summary>
        public string AuthType { get; init; } = string.Empty;
    }

    /// <summary>
    /// Conflict resolution rules for a connector.
    /// </summary>
    public class ConnectorConflictRules
    {
        /// <summary>Incompatible connector versions.</summary>
        public List<string> IncompatibleWith { get; init; } = new();

        /// <summary>Preferred versions for dependencies.</summary>
        public Dictionary<string, string> PreferVersion { get; init; } = new();

        /// <summary>Resolution strategy to use.</summary>
        public ResolutionStrategy ResolutionStrategy { get; init; } = ResolutionStrategy.HighestCompatible;
    }

    /// <summary>
    /// Connector type enumeration.
    /// </summary>
    public enum ConnectorType
    {
        Trigger,
        Action,
        Transform
    }

    /// <summary>
    /// Authentication type enumeration.
    /// </summary>
    public enum AuthType
    {
        None,
        ApiKey,
        OAuth2,
        Jwt
    }

    /// <summary>
    /// Icon shape enumeration.
    /// </summary>
    public enum IconShape
    {
        Circle,
        Square,
        Hexagon
    }

    /// <summary>
    /// Resolution strategy enumeration.
    /// </summary>
    public enum ResolutionStrategy
    {
        HighestCompatible,
        PreferStable,
        FailFast
    }
}