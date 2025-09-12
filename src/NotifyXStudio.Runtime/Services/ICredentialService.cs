namespace NotifyXStudio.Runtime.Services
{
    /// <summary>
    /// Service for managing and retrieving encrypted credentials.
    /// </summary>
    public interface ICredentialService
    {
        /// <summary>
        /// Gets a decrypted credential secret by ID and tenant.
        /// </summary>
        Task<string?> GetDecryptedSecretAsync(string credentialId, string tenantId);

        /// <summary>
        /// Validates that a credential exists and is accessible.
        /// </summary>
        Task<bool> ValidateCredentialAsync(string credentialId, string tenantId);

        /// <summary>
        /// Gets credential metadata without decrypting the secret.
        /// </summary>
        Task<CredentialMetadata?> GetCredentialMetadataAsync(string credentialId, string tenantId);

        /// <summary>
        /// Refreshes an OAuth credential if needed.
        /// </summary>
        Task<bool> RefreshCredentialAsync(string credentialId, string tenantId);
    }

    /// <summary>
    /// Metadata about a credential without the secret.
    /// </summary>
    public class CredentialMetadata
    {
        public string Id { get; init; } = string.Empty;
        public string TenantId { get; init; } = string.Empty;
        public string ConnectorType { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public List<string> Scopes { get; init; } = new();
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public bool IsActive { get; init; }
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
}