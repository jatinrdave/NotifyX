using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence
{
    public interface IWorkflowRepository
    {
        System.Threading.Tasks.Task<Workflow?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<List<Workflow>> GetAllAsync(CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<Workflow> CreateAsync(Workflow workflow, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<Workflow> UpdateAsync(Workflow workflow, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
