using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence.Repositories
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default);
        Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);
        Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
    }
}