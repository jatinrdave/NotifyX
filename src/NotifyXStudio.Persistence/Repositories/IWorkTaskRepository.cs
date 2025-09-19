using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence.Repositories
{
    public interface IWorkTaskRepository
    {
        Task<WorkTask?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkTask>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkTask>> GetByProjectIdAsync(string projectId, CancellationToken cancellationToken = default);
        Task<WorkTask> CreateAsync(WorkTask workTask, CancellationToken cancellationToken = default);
        Task<WorkTask> UpdateAsync(WorkTask workTask, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
    }
}