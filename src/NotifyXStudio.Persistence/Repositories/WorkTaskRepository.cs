using Microsoft.EntityFrameworkCore;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence.Repositories
{
    public class WorkTaskRepository : IWorkTaskRepository
    {
        private readonly NotifyXStudioDbContext _context;

        public WorkTaskRepository(NotifyXStudioDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<WorkTask?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkTasks
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<WorkTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.WorkTasks
                .OrderBy(t => t.Title)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<WorkTask>> GetByProjectIdAsync(string projectId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkTasks
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.Title)
                .ToListAsync(cancellationToken);
        }

        public async Task<WorkTask> CreateAsync(WorkTask workTask, CancellationToken cancellationToken = default)
        {
            var entity = new WorkTask
            {
                Id = Guid.NewGuid().ToString(),
                Title = workTask.Title,
                Description = workTask.Description,
                ProjectId = workTask.ProjectId,
                Status = workTask.Status,
                Priority = workTask.Priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = workTask.CreatedBy,
                UpdatedBy = workTask.UpdatedBy
            };

            _context.WorkTasks.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<WorkTask> UpdateAsync(WorkTask workTask, CancellationToken cancellationToken = default)
        {
            var entity = await _context.WorkTasks.FindAsync(workTask.Id);
            if (entity == null)
            {
                throw new InvalidOperationException($"WorkTask with ID {workTask.Id} not found.");
            }

            var updatedEntity = entity with
            {
                Title = workTask.Title,
                Description = workTask.Description,
                ProjectId = workTask.ProjectId,
                Status = workTask.Status,
                Priority = workTask.Priority,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = workTask.UpdatedBy
            };

            _context.Entry(entity).CurrentValues.SetValues(updatedEntity);
            await _context.SaveChangesAsync(cancellationToken);
            return updatedEntity;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.WorkTasks.FindAsync(id);
            if (entity != null)
            {
                _context.WorkTasks.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkTasks
                .AnyAsync(t => t.Id == id, cancellationToken);
        }
    }
}