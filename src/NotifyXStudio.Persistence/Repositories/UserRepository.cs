using Microsoft.EntityFrameworkCore;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly NotifyXStudioDbContext _context;

        public UserRepository(NotifyXStudioDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .OrderBy(u => u.Email)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Where(u => u.TenantId == tenantId)
                .OrderBy(u => u.Email)
                .ToListAsync(cancellationToken);
        }

        public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            var entity = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                TenantId = user.TenantId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = user.CreatedBy,
                UpdatedBy = user.UpdatedBy
            };

            _context.Users.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Users.FindAsync(user.Id);
            if (entity == null)
            {
                throw new InvalidOperationException($"User with ID {user.Id} not found.");
            }

            // Update properties
            var updatedEntity = entity with
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                TenantId = user.TenantId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = user.UpdatedBy
            };

            _context.Entry(entity).CurrentValues.SetValues(updatedEntity);
            await _context.SaveChangesAsync(cancellationToken);
            return updatedEntity;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Users.FindAsync(id);
            if (entity != null)
            {
                _context.Users.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email, cancellationToken);
        }
    }
}