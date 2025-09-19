using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Persistence.Repositories;

namespace NotifyXStudio.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<User> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user by ID: {UserId}", id);
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {id} not found", nameof(id));
            }
            return user;
        }

        public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating user: {Email}", user.Email);
            var createdUser = await _userRepository.CreateAsync(user, cancellationToken);
            _logger.LogInformation("User created successfully with ID: {UserId}", createdUser.Id);
            return createdUser;
        }

        public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating user: {UserId}", user.Id);
            var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);
            _logger.LogInformation("User updated successfully: {UserId}", user.Id);
            return updatedUser;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting user: {UserId}", id);
            var exists = await _userRepository.ExistsAsync(id, cancellationToken);
            if (exists)
            {
                await _userRepository.DeleteAsync(id, cancellationToken);
                _logger.LogInformation("User deleted successfully: {UserId}", id);
                return true;
            }
            _logger.LogWarning("User not found for deletion: {UserId}", id);
            return false;
        }

        public async Task<IEnumerable<User>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Listing users for tenant: {TenantId}, page: {Page}", tenantId, page);
            if (string.IsNullOrEmpty(tenantId))
            {
                return await _userRepository.GetAllAsync(cancellationToken);
            }
            return await _userRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUserActivityAsync(string userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user activity for: {UserId}, page: {Page}", userId, page);
            // This would typically query user activity logs, but for now return empty
            return new List<User>();
        }

        public async Task<int> GetUserActivityCountAsync(string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user activity count for: {UserId}", userId);
            return 0;
        }

        public async Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            return await CreateAsync(user, cancellationToken);
        }

        public async Task<User> CreateUserAsync(string email, string firstName, string lastName, string? tenantId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating user with email: {Email}", email);

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "",
                UpdatedBy = ""
            };

            return await _userRepository.CreateAsync(user, cancellationToken);
        }

        public async Task<User> CreateUserAsync(string email, string firstName, string lastName, string? tenantId, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating user with email: {Email} and metadata", email);

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "",
                UpdatedBy = ""
            };

            return await _userRepository.CreateAsync(user, cancellationToken);
        }

        public async Task<User> GetUserAsync(string id, CancellationToken cancellationToken = default)
        {
            return await GetByIdAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<User>> ListUsersAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            return await ListAsync(tenantId, page, pageSize, cancellationToken);
        }

        public async Task<IEnumerable<User>> ListUsersAsync(string? tenantId, string? role, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Listing users with role filter: {Role}", role);
            return await ListAsync(tenantId, page, pageSize, cancellationToken);
        }

        public async Task<int> GetUserCountAsync(string? tenantId, string? role, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user count for tenant: {TenantId}, role: {Role}", tenantId, role);
            var users = await ListAsync(tenantId, 1, int.MaxValue, cancellationToken);
            return users.Count();
        }

        public async Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            return await UpdateAsync(user, cancellationToken);
        }

        public async Task<User> UpdateUserAsync(string id, string? firstName, string? lastName, string? email, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating user: {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {id} not found", nameof(id));
            }

            var updatedUser = user with
            {
                FirstName = firstName ?? user.FirstName,
                LastName = lastName ?? user.LastName,
                Email = email ?? user.Email,
                UpdatedAt = DateTime.UtcNow
            };

            return await _userRepository.UpdateAsync(updatedUser, cancellationToken);
        }

        public async Task<User> DeleteUserAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting user: {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {id} not found", nameof(id));
            }

            await _userRepository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("User deleted successfully: {UserId}", id);

            return user;
        }

        public async Task<IEnumerable<User>> GetUserActivityAsync(string userId, string? filter = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user activity with filter: {Filter}", filter);
            return await GetUserActivityAsync(userId, page, pageSize, cancellationToken);
        }

        public async Task<int> GetUserActivityCountAsync(string userId, string? filter = null, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user activity count with filter: {Filter}", filter);
            return await GetUserActivityCountAsync(userId, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUserActivityAsync(string userId, DateTime startDate, DateTime endDate, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user activity for date range: {StartDate} to {EndDate}", startDate, endDate);
            return await GetUserActivityAsync(userId, page, pageSize, cancellationToken);
        }

        public async Task<int> GetUserActivityCountAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user activity count for date range: {StartDate} to {EndDate}", startDate, endDate);
            return await GetUserActivityCountAsync(userId, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting user permissions for: {UserId}", userId);
            // This would typically query user permissions, but for now return empty
            return new List<User>();
        }

        public async Task<User> UpdateUserPermissionsAsync(string userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating user permissions for: {UserId}", userId);
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found", nameof(userId));
            }
            return user;
        }

        public async Task<User> UpdateUserAsync(string id, string? firstName, string? lastName, string? email, CancellationToken cancellationToken = default)
        {
            return await UpdateUserAsync(id, firstName, lastName, email, null, cancellationToken);
        }
    }
}