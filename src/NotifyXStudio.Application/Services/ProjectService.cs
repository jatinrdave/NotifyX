using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Persistence.Repositories;

namespace NotifyXStudio.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(IProjectRepository projectRepository, ILogger<ProjectService> logger)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Project> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting project by ID: {ProjectId}", id);
            var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
            if (project == null)
            {
                throw new ArgumentException($"Project with ID {id} not found", nameof(id));
            }
            return project;
        }

        public async Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating project: {ProjectName}", project.Name);
            var createdProject = await _projectRepository.CreateAsync(project, cancellationToken);
            _logger.LogInformation("Project created successfully with ID: {ProjectId}", createdProject.Id);
            return createdProject;
        }

        public async Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating project: {ProjectId}", project.Id);
            var updatedProject = await _projectRepository.UpdateAsync(project, cancellationToken);
            _logger.LogInformation("Project updated successfully: {ProjectId}", project.Id);
            return updatedProject;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting project: {ProjectId}", id);
            var exists = await _projectRepository.ExistsAsync(id, cancellationToken);
            if (exists)
            {
                await _projectRepository.DeleteAsync(id, cancellationToken);
                _logger.LogInformation("Project deleted successfully: {ProjectId}", id);
                return true;
            }
            _logger.LogWarning("Project not found for deletion: {ProjectId}", id);
            return false;
        }

        public async Task<IEnumerable<Project>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Listing projects for tenant: {TenantId}, page: {Page}", tenantId, page);
            if (string.IsNullOrEmpty(tenantId))
            {
                return await _projectRepository.GetAllAsync(cancellationToken);
            }
            return await _projectRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        }

        // Additional methods needed by controllers
        public async Task<Project> CreateProjectAsync(Project project, CancellationToken cancellationToken = default)
        {
            return await CreateAsync(project, cancellationToken);
        }

        public async Task<Project> GetProjectAsync(string id, CancellationToken cancellationToken = default)
        {
            return await GetByIdAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<Project>> ListProjectsAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            return await ListAsync(tenantId, page, pageSize, cancellationToken);
        }

        public async Task<int> GetProjectCountAsync(string? tenantId = null, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting project count for tenant: {TenantId}", tenantId);
            var projects = await ListAsync(tenantId, 1, int.MaxValue, cancellationToken);
            return projects.Count();
        }

        public async Task<Project> UpdateProjectAsync(Project project, CancellationToken cancellationToken = default)
        {
            return await UpdateAsync(project, cancellationToken);
        }

        public async Task<Project> DeleteProjectAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting project: {ProjectId}", id);

            var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
            if (project == null)
            {
                throw new ArgumentException($"Project with ID {id} not found", nameof(id));
            }

            await _projectRepository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Project deleted successfully: {ProjectId}", id);

            return project;
        }

        public async Task<Project> GetProjectStatusAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting project status: {ProjectId}", id);
            var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
            if (project == null)
            {
                throw new ArgumentException($"Project with ID {id} not found", nameof(id));
            }
            return project;
        }

        public async Task<IEnumerable<Project>> GetProjectBuildsAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting project builds: {ProjectId}", id);
            // This would typically query related builds, but for now return empty
            return new List<Project>();
        }

        public async Task<IEnumerable<Project>> GetProjectDeploymentsAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting project deployments: {ProjectId}", id);
            // This would typically query related deployments, but for now return empty
            return new List<Project>();
        }

        public async Task<Project> GetProjectStatsAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting project stats: {ProjectId}", id);
            var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
            if (project == null)
            {
                throw new ArgumentException($"Project with ID {id} not found", nameof(id));
            }
            return project;
        }

        public async Task<IEnumerable<Project>> GetProjectTypesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting project types");
            // This would typically return project type definitions, but for now return empty
            return new List<Project>();
        }

        public async Task<Project> CreateProjectAsync(string name, string description, string? tenantId, string? status, string? tags, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating project: {ProjectName}", name);

            var project = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "",
                UpdatedBy = ""
            };

            return await _projectRepository.CreateAsync(project, cancellationToken);
        }

        public async Task<Project> UpdateProjectAsync(string id, string? name, string? description, string? status, string? tags, string? tenantId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating project: {ProjectId}", id);

            var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
            if (project == null)
            {
                throw new ArgumentException($"Project with ID {id} not found", nameof(id));
            }

            var updatedProject = project with
            {
                Name = name ?? project.Name,
                Description = description ?? project.Description,
                TenantId = tenantId ?? project.TenantId,
                UpdatedAt = DateTime.UtcNow
            };

            return await _projectRepository.UpdateAsync(updatedProject, cancellationToken);
        }

        public async Task<IEnumerable<Project>> GetProjectResourcesAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting project resources: {ProjectId}", id);
            // This would typically query related resources, but for now return empty
            return new List<Project>();
        }
    }
}