using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for project operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;
        private readonly IProjectService _projectService;

        public ProjectController(ILogger<ProjectController> logger, IProjectService projectService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        }

        /// <summary>
        /// Creates a project.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Project request is required");
                }

                var projectId = await _projectService.CreateProjectAsync(
                    request.Name,
                    request.Description,
                    null,
                    request.ProjectType,
                    null);

                return Ok(new
                {
                    projectId,
                    message = "Project created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create project: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create project",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets project information.
        /// </summary>
        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProject(string projectId)
        {
            try
            {
                var project = await _projectService.GetProjectAsync(projectId);

                if (project == null)
                {
                    return NotFound(new
                    {
                        error = "Project not found",
                        projectId
                    });
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get project {ProjectId}: {Message}", projectId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve project",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists projects.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListProjects(
            [FromQuery] string? projectType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var projects = await _projectService.ListProjectsAsync(projectType, page, pageSize);
                var totalCount = await _projectService.GetProjectCountAsync(projectType);

                return Ok(new
                {
                    projects,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list projects: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list projects",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a project.
        /// </summary>
        [HttpPut("{projectId}")]
        public async Task<IActionResult> UpdateProject(
            string projectId,
            [FromBody] UpdateProjectRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _projectService.UpdateProjectAsync(
                    projectId,
                    request.Name,
                    request.Description,
                    request.ProjectType,
                    null,
                    null);

                return Ok(new
                {
                    message = "Project updated successfully",
                    projectId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update project {ProjectId}: {Message}", projectId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update project",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a project.
        /// </summary>
        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject(string projectId)
        {
            try
            {
                await _projectService.DeleteProjectAsync(projectId);

                return Ok(new
                {
                    message = "Project deleted successfully",
                    projectId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete project {ProjectId}: {Message}", projectId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete project",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets project status.
        /// </summary>
        [HttpGet("{projectId}/status")]
        public async Task<IActionResult> GetProjectStatus(string projectId)
        {
            try
            {
                var status = await _projectService.GetProjectStatusAsync(projectId);

                return Ok(new
                {
                    projectId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get project status for {ProjectId}: {Message}", projectId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve project status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets project resources.
        /// </summary>
        [HttpGet("{projectId}/resources")]
        public async Task<IActionResult> GetProjectResources(string projectId)
        {
            try
            {
                var resources = await _projectService.GetProjectResourcesAsync(projectId);

                return Ok(new
                {
                    projectId,
                    resources
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get project resources for {ProjectId}: {Message}", projectId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve project resources",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets project builds.
        /// </summary>
        [HttpGet("{projectId}/builds")]
        public async Task<IActionResult> GetProjectBuilds(string projectId)
        {
            try
            {
                var builds = await _projectService.GetProjectBuildsAsync(projectId);

                return Ok(new
                {
                    projectId,
                    builds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get project builds for {ProjectId}: {Message}", projectId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve project builds",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets project deployments.
        /// </summary>
        [HttpGet("{projectId}/deployments")]
        public async Task<IActionResult> GetProjectDeployments(string projectId)
        {
            try
            {
                var deployments = await _projectService.GetProjectDeploymentsAsync(projectId);

                return Ok(new
                {
                    projectId,
                    deployments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get project deployments for {ProjectId}: {Message}", projectId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve project deployments",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets project statistics.
        /// </summary>
        [HttpGet("{projectId}/stats")]
        public async Task<IActionResult> GetProjectStats(string projectId)
        {
            try
            {
                var stats = await _projectService.GetProjectStatsAsync(projectId);

                return Ok(new
                {
                    projectId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get project stats for {ProjectId}: {Message}", projectId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve project statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available project types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetProjectTypes()
        {
            try
            {
                var projectTypes = await _projectService.GetProjectTypesAsync();

                return Ok(new
                {
                    projectTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get project types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve project types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create project request model.
    /// </summary>
    public class CreateProjectRequest
    {
        /// <summary>
        /// Project name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Project description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Project type.
        /// </summary>
        public string ProjectType { get; set; } = "application";

        /// <summary>
        /// Project configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new();

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update project request model.
    /// </summary>
    public class UpdateProjectRequest
    {
        /// <summary>
        /// Project name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Project description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Project type.
        /// </summary>
        public string? ProjectType { get; set; }

        /// <summary>
        /// Project configuration.
        /// </summary>
        public Dictionary<string, object>? Configuration { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}