using Microsoft.EntityFrameworkCore;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly NotifyXStudioDbContext _context;

        public WorkflowRepository(NotifyXStudioDbContext context)
        {
            _context = context;
        }

        public async Task<Workflow?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Workflows
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<List<Workflow>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Workflows
                .ToListAsync(cancellationToken);
        }

        public async Task<Workflow> CreateAsync(Workflow workflow, CancellationToken cancellationToken = default)
        {
            _context.Workflows.Add(workflow);
            await _context.SaveChangesAsync(cancellationToken);
            return workflow;
        }

        public async Task<Workflow> UpdateAsync(Workflow workflow, CancellationToken cancellationToken = default)
        {
            _context.Workflows.Update(workflow);
            await _context.SaveChangesAsync(cancellationToken);
            return workflow;
        }

        public async System.Threading.Tasks.Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var workflow = await GetByIdAsync(id, cancellationToken);
            if (workflow != null)
            {
                _context.Workflows.Remove(workflow);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
