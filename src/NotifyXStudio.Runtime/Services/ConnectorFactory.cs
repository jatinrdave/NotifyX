using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Connectors;

namespace NotifyXStudio.Runtime.Services
{
    /// <summary>
    /// Factory for creating connector adapters using dependency injection.
    /// </summary>
    public class ConnectorFactory : IConnectorFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConnectorFactory> _logger;
        private readonly Dictionary<string, Type> _registeredTypes = new();

        public ConnectorFactory(IServiceProvider serviceProvider, ILogger<ConnectorFactory> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IConnectorAdapter? Create(string connectorType)
        {
            try
            {
                if (!_registeredTypes.TryGetValue(connectorType, out var adapterType))
                {
                    _logger.LogWarning("No adapter registered for connector type: {ConnectorType}", connectorType);
                    return null;
                }

                var adapter = _serviceProvider.GetService(adapterType) as IConnectorAdapter;
                if (adapter == null)
                {
                    _logger.LogError("Failed to create adapter for connector type: {ConnectorType}", connectorType);
                    return null;
                }

                _logger.LogDebug("Created adapter for connector type: {ConnectorType}", connectorType);
                return adapter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating adapter for connector type: {ConnectorType}", connectorType);
                return null;
            }
        }

        public IEnumerable<string> GetAvailableTypes()
        {
            return _registeredTypes.Keys.ToList();
        }

        public void Register<T>(string connectorType) where T : class, IConnectorAdapter
        {
            _registeredTypes[connectorType] = typeof(T);
            _logger.LogInformation("Registered connector adapter: {ConnectorType} -> {AdapterType}", 
                connectorType, typeof(T).Name);
        }

        public void Unregister(string connectorType)
        {
            if (_registeredTypes.Remove(connectorType))
            {
                _logger.LogInformation("Unregistered connector adapter: {ConnectorType}", connectorType);
            }
        }

        public bool IsAvailable(string connectorType)
        {
            return _registeredTypes.ContainsKey(connectorType);
        }
    }
}