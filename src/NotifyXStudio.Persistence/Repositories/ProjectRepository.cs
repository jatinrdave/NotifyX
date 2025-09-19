using Microsoft.EntityFrameworkCore;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly NotifyXStudioDbContext _context;

        public ProjectRepository(NotifyXStudioDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Project?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Project>> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .Where(p => p.TenantId == tenantId)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
        {
            var entity = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Name = project.Name,
                Description = project.Description,
                TenantId = project.TenantId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = project.CreatedBy,
                UpdatedBy = project.UpdatedBy
            };

            _context.Projects.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Projects.FindAsync(project.Id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Project with ID {project.Id} not found.");
            }

            var updatedEntity = entity with
            {
                Name = project.Name,
                Description = project.Description,
                TenantId = project.TenantId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = project.UpdatedBy
            };

            _context.Entry(entity).CurrentValues.SetValues(updatedEntity);
            await _context.SaveChangesAsync(cancellationToken);
            return updatedEntity;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Projects.FindAsync(id);
            if (entity != null)
            {
                _context.Projects.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .AnyAsync(p => p.Id == id, cancellationToken);
        }
    }
}