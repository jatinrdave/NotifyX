using Microsoft.EntityFrameworkCore;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence
{
    public class RunRepository : IRunRepository
    {
        private readonly NotifyXStudioDbContext _context;

        public RunRepository(NotifyXStudioDbContext context)
        {
            _context = context;
        }

        public async Task<WorkflowRun?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowRuns
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<List<WorkflowRun>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowRuns
                .ToListAsync(cancellationToken);
        }

        public async Task<List<WorkflowRun>> GetByWorkflowIdAsync(string workflowId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowRuns
                .Where(r => r.WorkflowId == workflowId)
                .ToListAsync(cancellationToken);
        }

        public async Task<WorkflowRun> CreateAsync(WorkflowRun run, CancellationToken cancellationToken = default)
        {
            _context.WorkflowRuns.Add(run);
            await _context.SaveChangesAsync(cancellationToken);
            return run;
        }

        public async Task<WorkflowRun> UpdateAsync(WorkflowRun run, CancellationToken cancellationToken = default)
        {
            _context.WorkflowRuns.Update(run);
            await _context.SaveChangesAsync(cancellationToken);
            return run;
        }

        public async System.Threading.Tasks.Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var run = await GetByIdAsync(id, cancellationToken);
            if (run != null)
            {
                _context.WorkflowRuns.Remove(run);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
