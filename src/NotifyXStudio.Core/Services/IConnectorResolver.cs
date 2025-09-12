using NotifyXStudio.Core.Models;
using NotifyXStudio.Core.Resolvers;

namespace NotifyXStudio.Core.Services
{
    /// <summary>
    /// Service for resolving connector dependencies.
    /// </summary>
    public interface IConnectorResolver
    {
        /// <summary>
        /// Resolves dependencies for the requested connectors.
        /// </summary>
        Task<ResolutionResult> ResolveAsync(
            IEnumerable<DependencySpec> requested,
            ResolutionStrategy strategy = ResolutionStrategy.HighestCompatible,
            Dictionary<string, string>? lockfile = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a lockfile from a resolution result.
        /// </summary>
        Task<Lockfile> GenerateLockfileAsync(ResolutionResult result);

        /// <summary>
        /// Explains why a dependency resolution failed.
        /// </summary>
        Task<ResolutionDiagnostics> ExplainFailureAsync(IEnumerable<DependencySpec> requested);

        /// <summary>
        /// Validates a lockfile against the current registry.
        /// </summary>
        Task<LockfileValidationResult> ValidateLockfileAsync(Lockfile lockfile);

        /// <summary>
        /// Updates a lockfile to use the latest compatible versions.
        /// </summary>
        Task<Lockfile> UpdateLockfileAsync(Lockfile lockfile, ResolutionStrategy strategy = ResolutionStrategy.HighestCompatible);
    }

    /// <summary>
    /// Lockfile for pinning resolved versions.
    /// </summary>
    public class Lockfile
    {
        public string Version { get; init; } = "1.0";
        public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
        public string GeneratedBy { get; init; } = string.Empty;
        public Dictionary<string, string> ResolvedVersions { get; init; } = new();
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Diagnostics for failed dependency resolution.
    /// </summary>
    public class ResolutionDiagnostics
    {
        public bool HasConflicts { get; init; }
        public List<ConflictInfo> Conflicts { get; init; } = new();
        public List<string> Suggestions { get; init; } = new();
        public Dictionary<string, List<string>> AvailableVersions { get; init; } = new();
    }

    /// <summary>
    /// Information about a dependency conflict.
    /// </summary>
    public class ConflictInfo
    {
        public string ConnectorId { get; init; } = string.Empty;
        public List<string> ConflictingVersions { get; init; } = new();
        public string Reason { get; init; } = string.Empty;
        public List<string> SuggestedVersions { get; init; } = new();
    }

    /// <summary>
    /// Result of lockfile validation.
    /// </summary>
    public class LockfileValidationResult
    {
        public bool IsValid { get; init; }
        public List<string> Errors { get; init; } = new();
        public List<string> Warnings { get; init; } = new();
        public List<string> OutdatedVersions { get; init; } = new();
        public List<string> MissingConnectors { get; init; } = new();
    }
}