using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence
{
    public interface IRunRepository
    {
        System.Threading.Tasks.Task<WorkflowRun?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<List<WorkflowRun>> GetAllAsync(CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<List<WorkflowRun>> GetByWorkflowIdAsync(string workflowId, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<WorkflowRun> CreateAsync(WorkflowRun run, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<WorkflowRun> UpdateAsync(WorkflowRun run, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
