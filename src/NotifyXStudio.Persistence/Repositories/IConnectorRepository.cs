using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence
{
    public interface IConnectorRepository
    {
        System.Threading.Tasks.Task<ConnectorRegistryEntry?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<List<ConnectorRegistryEntry>> GetAllAsync(CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<ConnectorRegistryEntry> CreateAsync(ConnectorRegistryEntry connector, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<ConnectorRegistryEntry> UpdateAsync(ConnectorRegistryEntry connector, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
