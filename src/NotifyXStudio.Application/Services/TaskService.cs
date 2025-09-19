using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Persistence.Repositories;
using TaskModel = NotifyXStudio.Core.Models.WorkTask;

namespace NotifyXStudio.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly IWorkTaskRepository _taskRepository;
        private readonly ILogger<TaskService> _logger;

        public TaskService(IWorkTaskRepository taskRepository, ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TaskModel> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting task by ID: {TaskId}", id);
            var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
            if (task == null)
            {
                throw new ArgumentException($"Task with ID {id} not found", nameof(id));
            }
            return task;
        }

        public async Task<TaskModel> CreateAsync(TaskModel task, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating task: {TaskTitle}", task.Title);
            var createdTask = await _taskRepository.CreateAsync(task, cancellationToken);
            _logger.LogInformation("Task created successfully with ID: {TaskId}", createdTask.Id);
            return createdTask;
        }

        public async Task<TaskModel> UpdateAsync(TaskModel task, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating task: {TaskId}", task.Id);
            var updatedTask = await _taskRepository.UpdateAsync(task, cancellationToken);
            _logger.LogInformation("Task updated successfully: {TaskId}", task.Id);
            return updatedTask;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting task: {TaskId}", id);
            var exists = await _taskRepository.ExistsAsync(id, cancellationToken);
            if (exists)
            {
                await _taskRepository.DeleteAsync(id, cancellationToken);
                _logger.LogInformation("Task deleted successfully: {TaskId}", id);
                return true;
            }
            _logger.LogWarning("Task not found for deletion: {TaskId}", id);
            return false;
        }

        public async Task<IEnumerable<TaskModel>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Listing tasks for project: {ProjectId}, page: {Page}", projectId, page);
            if (string.IsNullOrEmpty(projectId))
            {
                return await _taskRepository.GetAllAsync(cancellationToken);
            }
            return await _taskRepository.GetByProjectIdAsync(projectId, cancellationToken);
        }

        public async Task<TaskModel> CreateTaskAsync(string title, string? description, string? status, string? priority, string? assigneeId, string? projectId, DateTime? dueDate, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating task: {TaskTitle}", title);

            var task = new WorkTask
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Description = description ?? "",
                ProjectId = projectId,
                Status = status ?? "Open",
                Priority = priority ?? "Medium",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = assigneeId ?? "",
                UpdatedBy = assigneeId ?? ""
            };

            var createdTask = await _taskRepository.CreateAsync(task, cancellationToken);
            _logger.LogInformation("Task created successfully with ID: {TaskId}", createdTask.Id);

            return createdTask;
        }

        public async Task<TaskModel> GetTaskAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting task: {TaskId}", id);
            return await _taskRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<TaskModel>> ListTasksAsync(string? projectId = null, string? status = null, string? priority = null, string? assigneeId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Listing tasks with filters - project: {ProjectId}, status: {Status}, priority: {Priority}, assignee: {AssigneeId}", projectId, status, priority, assigneeId);
            if (string.IsNullOrEmpty(projectId))
            {
                return await _taskRepository.GetAllAsync(cancellationToken);
            }
            return await _taskRepository.GetByProjectIdAsync(projectId, cancellationToken);
        }

        public async Task<int> GetTaskCountAsync(string? projectId = null, string? status = null, string? priority = null, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting task count for project: {ProjectId}, status: {Status}, priority: {Priority}", projectId, status, priority);
            IEnumerable<TaskModel> tasks;
            if (string.IsNullOrEmpty(projectId))
            {
                tasks = await _taskRepository.GetAllAsync(cancellationToken);
            }
            else
            {
                tasks = await _taskRepository.GetByProjectIdAsync(projectId, cancellationToken);
            }
            return tasks.Count();
        }

        public async Task<TaskModel> UpdateTaskAsync(string id, string? title, string? description, string? status, string? priority, string? assigneeId, string? projectId, DateTime? dueDate, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating task: {TaskId}", id);

            var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
            if (task == null)
            {
                throw new ArgumentException($"Task with ID {id} not found", nameof(id));
            }

            var updatedTask = task with
            {
                Title = title ?? task.Title,
                Description = description ?? task.Description,
                Status = status ?? task.Status,
                Priority = priority ?? task.Priority,
                ProjectId = projectId ?? task.ProjectId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = assigneeId ?? task.UpdatedBy
            };

            var result = await _taskRepository.UpdateAsync(updatedTask, cancellationToken);
            _logger.LogInformation("Task updated successfully: {TaskId}", id);

            return result;
        }

        public async Task<TaskModel> DeleteTaskAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting task: {TaskId}", id);

            var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
            if (task == null)
            {
                throw new ArgumentException($"Task with ID {id} not found", nameof(id));
            }

            await _taskRepository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Task deleted successfully: {TaskId}", id);

            return task;
        }

        public async Task<Dictionary<string, object>> GetTaskStatusAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting task status: {TaskId}", id);
            var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
            return new Dictionary<string, object>
            {
                ["status"] = task?.Status ?? "Unknown",
                ["priority"] = task?.Priority ?? "Unknown",
                ["updatedAt"] = task?.UpdatedAt ?? DateTime.MinValue
            };
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetTaskIssuesAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting task issues: {TaskId}", id);
            // This would typically query related issues, but for now return empty
            return new List<Dictionary<string, object>>();
        }

        public async Task<Dictionary<string, object>> GetTaskStatsAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting task stats: {TaskId}", id);
            var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
            return new Dictionary<string, object>
            {
                ["createdAt"] = task?.CreatedAt ?? DateTime.MinValue,
                ["updatedAt"] = task?.UpdatedAt ?? DateTime.MinValue,
                ["status"] = task?.Status ?? "Unknown"
            };
        }

        public async Task<IEnumerable<string>> GetTaskTypesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting task types");
            return new List<string> { "Bug", "Feature", "Task", "Story", "Epic" };
        }
    }
}