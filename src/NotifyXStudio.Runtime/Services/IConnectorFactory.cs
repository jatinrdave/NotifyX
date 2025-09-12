using NotifyXStudio.Core.Connectors;

namespace NotifyXStudio.Runtime.Services
{
    /// <summary>
    /// Factory for creating connector adapters.
    /// </summary>
    public interface IConnectorFactory
    {
        /// <summary>
        /// Creates a connector adapter for the specified type.
        /// </summary>
        IConnectorAdapter? Create(string connectorType);

        /// <summary>
        /// Gets all available connector types.
        /// </summary>
        IEnumerable<string> GetAvailableTypes();

        /// <summary>
        /// Registers a connector adapter.
        /// </summary>
        void Register<T>(string connectorType) where T : class, IConnectorAdapter;

        /// <summary>
        /// Unregisters a connector adapter.
        /// </summary>
        void Unregister(string connectorType);

        /// <summary>
        /// Checks if a connector type is available.
        /// </summary>
        bool IsAvailable(string connectorType);
    }
}