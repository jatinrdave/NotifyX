using NotifyXStudio.Core.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TaskModel = NotifyXStudio.Core.Models.Task;

namespace NotifyXStudio.Core.Services
{
    // User Management Services
    public partial interface IUserService
    {
        Task<User> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
        Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUserActivityAsync(string userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetUserActivityCountAsync(string userId, CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default);
        Task<User> CreateUserAsync(string email, string firstName, string lastName, string? tenantId, CancellationToken cancellationToken = default);
        Task<User> CreateUserAsync(string email, string firstName, string lastName, string? tenantId, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default);
        Task<User> GetUserAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> ListUsersAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> ListUsersAsync(string? tenantId, string? role, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetUserCountAsync(string? tenantId, string? role, CancellationToken cancellationToken = default);
        Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task<User> DeleteUserAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUserActivityAsync(string userId, string? filter = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetUserActivityCountAsync(string userId, string? filter = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUserActivityAsync(string userId, DateTime startDate, DateTime endDate, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetUserActivityCountAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
        Task<User> UpdateUserPermissionsAsync(string userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<IEnumerable<User>> ListUsersAsync(string? tenantId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetUserCountAsync(string? tenantId, string? status, CancellationToken cancellationToken = default);
        Task<User> UpdateUserAsync(string id, string? firstName, string? lastName, string? email, CancellationToken cancellationToken = default);
    }

    // Project Management Services
    public interface IProjectService
    {
        Task<Project> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);
        Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<Project> CreateProjectAsync(Project project, CancellationToken cancellationToken = default);
        Task<Project> GetProjectAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> ListProjectsAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetProjectCountAsync(string? tenantId = null, CancellationToken cancellationToken = default);
        Task<Project> UpdateProjectAsync(Project project, CancellationToken cancellationToken = default);
        Task<Project> DeleteProjectAsync(string id, CancellationToken cancellationToken = default);
        Task<Project> GetProjectStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetProjectBuildsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetProjectDeploymentsAsync(string id, CancellationToken cancellationToken = default);
        Task<Project> GetProjectStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetProjectTypesAsync(CancellationToken cancellationToken = default);
        Task<Project> CreateProjectAsync(string name, string description, string? tenantId, string? status, string? tags, CancellationToken cancellationToken = default);
        Task<Project> UpdateProjectAsync(string id, string? name, string? description, string? status, string? tags, string? tenantId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetProjectResourcesAsync(string id, CancellationToken cancellationToken = default);
    }

    // Task Management Services
    public partial interface ITaskService
    {
        Task<TaskModel> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<TaskModel> CreateAsync(TaskModel task, CancellationToken cancellationToken = default);
        Task<TaskModel> UpdateAsync(TaskModel task, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TaskModel>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<TaskModel> CreateTaskAsync(string title, string? description, string? status, string? priority, string? assigneeId, string? projectId, DateTime? dueDate, CancellationToken cancellationToken = default);
        Task<TaskModel> GetTaskAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TaskModel>> ListTasksAsync(string? projectId = null, string? status = null, string? priority = null, string? assigneeId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetTaskCountAsync(string? projectId = null, string? status = null, string? priority = null, CancellationToken cancellationToken = default);
        Task<TaskModel> UpdateTaskAsync(string id, string? title, string? description, string? status, string? priority, string? assigneeId, string? projectId, DateTime? dueDate, CancellationToken cancellationToken = default);
        Task<TaskModel> DeleteTaskAsync(string id, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetTaskStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Dictionary<string, object>>> GetTaskIssuesAsync(string id, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetTaskStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetTaskTypesAsync(CancellationToken cancellationToken = default);
    }

    // Issue Management Services
    public interface IIssueService
    {
        Task<Issue> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Issue> CreateAsync(Issue issue, CancellationToken cancellationToken = default);
        Task<Issue> UpdateAsync(Issue issue, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Issue>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Issue> CreateIssueAsync(string title, string description, string? projectId, string? assigneeId, string? priority, CancellationToken cancellationToken = default);
        Task<Issue> GetIssueAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Issue>> ListIssuesAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetIssueCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<Issue> UpdateIssueAsync(string id, string? title, string? description, string? status, string? priority, CancellationToken cancellationToken = default);
        Task<Issue> DeleteIssueAsync(string id, CancellationToken cancellationToken = default);
        Task<Issue> GetIssueStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Issue>> GetIssueCommentsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Issue>> GetIssueAttachmentsAsync(string id, CancellationToken cancellationToken = default);
        Task<Issue> GetIssueStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Issue>> GetIssueTypesAsync(CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<Issue> CreateIssueAsync(string title, string description, string? projectId, string? assigneeId, string? priority, string? status, CancellationToken cancellationToken = default);
        Task<Issue> CreateIssueAsync(string title, string description, string? projectId, string? assigneeId, string? priority, string? status, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
        Task<IEnumerable<Issue>> ListIssuesAsync(string? projectId, string? status, string? priority, string? assigneeId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetIssueCountAsync(string? projectId, string? status, string? priority, string? assigneeId, CancellationToken cancellationToken = default);
        Task<Issue> UpdateIssueAsync(string id, string? title, string? description, string? status, string? priority, string? assigneeId, string? tags, CancellationToken cancellationToken = default);
        Task<Issue> UpdateIssueAsync(string id, string? title, string? description, string? status, string? priority, string? assigneeId, string? tags, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
    }

    // Story Management Services
    public partial interface IStoryService
    {
        Task<Story> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Story> CreateAsync(Story story, CancellationToken cancellationToken = default);
        Task<Story> UpdateAsync(Story story, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Story>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Story> CreateStoryAsync(string title, string? description, string? status, string? priority, string? assigneeId, string? projectId, CancellationToken cancellationToken = default);
        Task<Story> GetStoryAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Story>> ListStoriesAsync(string? projectId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetStoryCountAsync(string? projectId, string? status, CancellationToken cancellationToken = default);
        Task<Story> UpdateStoryAsync(string id, string? title, string? description, string? status, string? priority, string? assigneeId, string? projectId, CancellationToken cancellationToken = default);
        Task<Story> DeleteStoryAsync(string id, CancellationToken cancellationToken = default);
        Task<Story> GetStoryStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Story>> GetStoryIssuesAsync(string id, CancellationToken cancellationToken = default);
        Task<Story> GetStoryStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Story>> GetStoryTypesAsync(CancellationToken cancellationToken = default);
    }

    // Epic Management Services
    public interface IEpicService
    {
        Task<Epic> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Epic> CreateAsync(Epic epic, CancellationToken cancellationToken = default);
        Task<Epic> UpdateAsync(Epic epic, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Epic>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<IEnumerable<Epic>> ListEpicsAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetEpicCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<Epic> UpdateEpicAsync(string id, string? name, string? description, string? status, CancellationToken cancellationToken = default);
        Task<Epic> UpdateEpicAsync(string id, string? name, string? description, string? status, string? priority, string? projectId, CancellationToken cancellationToken = default);
        Task<Epic> DeleteEpicAsync(string id, CancellationToken cancellationToken = default);
        Task<Epic> GetEpicStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Epic>> GetEpicIssuesAsync(string id, CancellationToken cancellationToken = default);
        Task<Epic> GetEpicStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Epic>> GetEpicTypesAsync(CancellationToken cancellationToken = default);
    }

    // Subtask Management Services
    public partial interface ISubtaskService
    {
        Task<Subtask> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Subtask> CreateAsync(Subtask subtask, CancellationToken cancellationToken = default);
        Task<Subtask> UpdateAsync(Subtask subtask, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subtask>> ListAsync(string? taskId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Subtask> UpdateSubtaskAsync(string id, string? title, string? description, string? status, string? assigneeId, CancellationToken cancellationToken = default);
        Task<Subtask> DeleteSubtaskAsync(string id, CancellationToken cancellationToken = default);
        Task<Subtask> GetSubtaskStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subtask>> GetSubtaskIssuesAsync(string id, CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<Subtask> CreateSubtaskAsync(string title, string? description, string? status, string? assigneeId, CancellationToken cancellationToken = default);
        Task<Subtask> GetSubtaskAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subtask>> ListSubtasksAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetSubtaskCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<Subtask> UpdateSubtaskAsync(string id, string? title, string? description, string? status, string? priority, string? assigneeId, string? projectId, string? parentTaskId, CancellationToken cancellationToken = default);
        
        // Additional methods for controller compatibility
        Task<Subtask> GetSubtaskStatsAsync(string subtaskId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subtask>> GetSubtaskTypesAsync(CancellationToken cancellationToken = default);
    }

    // Milestone Management Services
    public interface IMilestoneService
    {
        Task<Milestone> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Milestone> CreateAsync(Milestone milestone, CancellationToken cancellationToken = default);
        Task<Milestone> UpdateAsync(Milestone milestone, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Milestone>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Milestone> CreateMilestoneAsync(string name, string description, DateTime? dueDate, string? projectId, CancellationToken cancellationToken = default);
        Task<Milestone> GetMilestoneAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Milestone>> ListMilestonesAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetMilestoneCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<Milestone> UpdateMilestoneAsync(string id, string? name, string? description, DateTime? dueDate, string? status, CancellationToken cancellationToken = default);
        Task<Milestone> DeleteMilestoneAsync(string id, CancellationToken cancellationToken = default);
        Task<Milestone> GetMilestoneStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Milestone>> GetMilestoneIssuesAsync(string id, CancellationToken cancellationToken = default);
        Task<Milestone> GetMilestoneStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Milestone>> GetMilestoneTypesAsync(CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<Milestone> CreateMilestoneAsync(string name, string description, DateTime? dueDate, string? projectId, string? status, CancellationToken cancellationToken = default);
        Task<Milestone> CreateMilestoneAsync(string name, string description, string? dueDate, string? projectId, string? status, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
        Task<IEnumerable<Milestone>> ListMilestonesAsync(string? projectId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetMilestoneCountAsync(string? projectId, string? status, CancellationToken cancellationToken = default);
        Task<Milestone> UpdateMilestoneAsync(string id, string? name, string? description, DateTime? dueDate, string? status, string? tags, CancellationToken cancellationToken = default);
        Task<Milestone> UpdateMilestoneAsync(string id, string? name, string? description, DateTime? dueDate, string? status, string? tags, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
    }

    // Release Management Services
    public partial interface IReleaseService
    {
        Task<Release> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Release> CreateAsync(Release release, CancellationToken cancellationToken = default);
        Task<Release> UpdateAsync(Release release, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Release>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Release> CreateReleaseAsync(Release release, CancellationToken cancellationToken = default);
        Task<Release> GetReleaseAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Release>> ListReleasesAsync(string? projectId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetReleaseCountAsync(string? projectId, string? status, CancellationToken cancellationToken = default);
        Task<Release> UpdateReleaseAsync(string id, string? name, string? description, string? status, CancellationToken cancellationToken = default);
        Task<Release> DeleteReleaseAsync(string id, CancellationToken cancellationToken = default);
        Task<Release> PublishReleaseAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Release>> GetReleasesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<Release> CreateReleaseAsync(string name, string version, string? description, string? projectId, CancellationToken cancellationToken = default);
    }

    // Iteration Management Services
    public interface IIterationService
    {
        Task<Iteration> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Iteration> CreateAsync(Iteration iteration, CancellationToken cancellationToken = default);
        Task<Iteration> UpdateAsync(Iteration iteration, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Iteration>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<Iteration>> GetIterationTypesAsync(CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<Iteration> CreateIterationAsync(string name, string description, DateTime? startDate, DateTime? endDate, string? projectId, CancellationToken cancellationToken = default);
        Task<Iteration> CreateIterationAsync(string name, string description, DateTime? startDate, DateTime? endDate, string? projectId, string? status, CancellationToken cancellationToken = default);
        Task<Iteration> GetIterationAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Iteration>> ListIterationsAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<Iteration>> ListIterationsAsync(string? projectId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetIterationCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<int> GetIterationCountAsync(string? projectId, string? status, CancellationToken cancellationToken = default);
        Task<Iteration> UpdateIterationAsync(string id, string? name, string? description, DateTime? startDate, DateTime? endDate, string? status, CancellationToken cancellationToken = default);
        Task<Iteration> UpdateIterationAsync(string id, string? name, string? description, DateTime? startDate, DateTime? endDate, string? status, string? projectId, CancellationToken cancellationToken = default);
        Task<Iteration> DeleteIterationAsync(string id, CancellationToken cancellationToken = default);
        Task<Iteration> GetIterationStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Iteration>> GetIterationIssuesAsync(string id, CancellationToken cancellationToken = default);
        Task<Iteration> GetIterationStatsAsync(string id, CancellationToken cancellationToken = default);
    }

    // Tag Management Services
    public partial interface ITagService
    {
        Task<Tag> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Tag> CreateAsync(Tag tag, CancellationToken cancellationToken = default);
        Task<Tag> UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tag>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<Tag> CreateTagAsync(string name, string? description, CancellationToken cancellationToken = default);
        Task<Tag> CreateTagAsync(string name, string? description, string? color, string? category, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default);
        Task<Tag> GetTagAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tag>> ListTagsAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tag>> ListTagsAsync(string? projectId, string? category, string? color, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetTagCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<int> GetTagCountAsync(string? projectId, string? category, CancellationToken cancellationToken = default);
        Task<Tag> UpdateTagAsync(string id, string? name, string? description, CancellationToken cancellationToken = default);
        Task<Tag> UpdateTagAsync(string id, string? name, string? description, string? color, string? category, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default);
        Task<bool> DeleteTagAsync(string id, CancellationToken cancellationToken = default);
        
        // Additional methods for controller compatibility
        Task<Tag> GetTagStatusAsync(string tagId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tag>> GetTagCommitsAsync(string tagId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tag>> GetTagBuildsAsync(string tagId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tag>> GetTagDeploymentsAsync(string tagId, CancellationToken cancellationToken = default);
        Task<Tag> GetTagStatsAsync(string tagId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tag>> GetTagTypesAsync(CancellationToken cancellationToken = default);
    }

    // Role Management Services
    public partial interface IRoleService
    {
        Task<Role> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default);
        Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Role> CreateRoleAsync(Role role, CancellationToken cancellationToken = default);
        Task<Role> GetRoleAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> ListRolesAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetRoleCountAsync(string? tenantId = null, CancellationToken cancellationToken = default);
        Task<Role> UpdateRoleAsync(Role role, CancellationToken cancellationToken = default);
        Task<Role> DeleteRoleAsync(string id, CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<Role> CreateRoleAsync(string name, string? description, string? permissions, string? tenantId, CancellationToken cancellationToken = default);
        Task<Role> UpdateRoleAsync(string id, string? name, string? description, string? permissions, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetRolePermissionsAsync(string roleId, CancellationToken cancellationToken = default);
        Task<Role> UpdateRolePermissionsAsync(string roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetAvailablePermissionsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetRoleUsersAsync(string roleId, CancellationToken cancellationToken = default);
        Task<int> GetRoleUserCountAsync(string roleId, CancellationToken cancellationToken = default);
    }

    // Permission Management Services
    public interface IPermissionService
    {
        Task<Permission> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Permission> CreateAsync(Permission permission, CancellationToken cancellationToken = default);
        Task<Permission> UpdateAsync(Permission permission, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Permission> CreatePermissionAsync(Permission permission, CancellationToken cancellationToken = default);
        Task<Permission> GetPermissionAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> ListPermissionsAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetPermissionCountAsync(string? tenantId = null, CancellationToken cancellationToken = default);
        Task<Permission> UpdatePermissionAsync(Permission permission, CancellationToken cancellationToken = default);
        Task<Permission> DeletePermissionAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> GetAvailableResourcesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> GetAvailableActionsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> GetPermissionMatrixAsync(string? tenantId = null, CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<IEnumerable<Permission>> ListPermissionsAsync(string? tenantId, string? resource, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetPermissionCountAsync(string? tenantId, string? resource, CancellationToken cancellationToken = default);
        Task<Permission> UpdatePermissionAsync(string id, string? name, string? description, string? resource, string? action, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<Permission> CreatePermissionAsync(string name, string description, string resource, string action, string? tenantId, CancellationToken cancellationToken = default);
    }

    // Tenant Management Services
    public partial interface ITenantService
    {
        Task<Tenant> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default);
        Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tenant>> ListAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Tenant> GetTenantUsageAsync(string tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Tenant> UpdateTenantSettingsAsync(string tenantId, Dictionary<string, object> settings, CancellationToken cancellationToken = default);
        Task<Tenant> GetTenantUsageAsync(string tenantId, CancellationToken cancellationToken = default);
        Task<Tenant> UpdateTenantSettingsAsync(Tenant tenant, Dictionary<string, object> settings, CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<Tenant> CreateTenantAsync(string name, string description, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tenant>> ListTenantsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetTenantCountAsync(CancellationToken cancellationToken = default);
        Task<Tenant> UpdateTenantAsync(string id, string? name, string? description, string? status, CancellationToken cancellationToken = default);
    }

    // Notification Services
    public interface INotificationService
    {
        Task<Notification> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken = default);
        Task<Notification> UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> ListAsync(string? userId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Notification> SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
        Task<Notification> GetNotificationStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetNotificationHistoryAsync(string userId, string? type, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetNotificationCountAsync(string userId, string? type, string? status, CancellationToken cancellationToken = default);
        Task<Notification> GetNotificationStatsAsync(string userId, CancellationToken cancellationToken = default);
        Task<Notification> CancelNotificationAsync(string id, CancellationToken cancellationToken = default);
        Task<Notification> RetryNotificationAsync(string id, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<Notification> SendNotificationAsync(string title, string message, string type, string? userId, string? projectId, CancellationToken cancellationToken = default);
    }

    // Event Services
    public interface IEventService
    {
        Task<Event> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Event> CreateAsync(Event eventModel, CancellationToken cancellationToken = default);
        Task<Event> UpdateAsync(Event eventModel, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Event>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Event> SubscribeToEventsAsync(string userId, string eventType, string? projectId, string? filters, CancellationToken cancellationToken = default);
        Task<Event> UnsubscribeFromEventsAsync(string userId, string eventType, string? projectId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Event>> GetEventSubscriptionsAsync(string userId, CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<Event> GetEventAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Event>> ListEventsAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<Event>> ListEventsAsync(string? projectId, string? eventType, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetEventCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<int> GetEventCountAsync(string? projectId, string? eventType, string? status, CancellationToken cancellationToken = default);
        Task<Event> GetEventStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<Event> GetEventStatsAsync(string? projectId, string? eventType, CancellationToken cancellationToken = default);
        Task<IEnumerable<Event>> GetEventTypesAsync(CancellationToken cancellationToken = default);
        Task<Event> CreateEventAsync(string title, string description, string eventType, List<string> tags, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
        Task<Event> UnsubscribeFromEventsAsync(string userId, CancellationToken cancellationToken = default);
        Task<Event> CreateEventAsync(Guid id, List<string> tags, string? description, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
    }

    // File Services
    public interface IFileService
    {
        Task<NotifyXStudio.Core.Models.File> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.File> CreateAsync(NotifyXStudio.Core.Models.File file, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.File> UpdateAsync(NotifyXStudio.Core.Models.File file, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.File>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.File>> GetFileHistoryAsync(string id, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.File> GetFileStatsAsync(string id, CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<NotifyXStudio.Core.Models.File> UploadFileAsync(object file, string? projectId, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.File> UploadFileAsync(byte[] content, string fileName, string? projectId, string? description, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.File> GetFileAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.File>> ListFilesAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.File>> ListFilesAsync(string? projectId, string? type, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetFileCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<int> GetFileCountAsync(string? projectId, string? type, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.File> UpdateFileAsync(NotifyXStudio.Core.Models.File file, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.File> UpdateFileAsync(string id, string? name, string? description, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.File> DeleteFileAsync(string id, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.File> DownloadFileAsync(string id, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.File> GetFileContentAsync(string id, CancellationToken cancellationToken = default);
    }

    // Log Services
    public interface ILogService
    {
        Task<Log> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Log> CreateAsync(Log log, CancellationToken cancellationToken = default);
        Task<Log> UpdateAsync(Log log, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Log>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<Log>> GetLogsAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetLogCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<Log> GetLogStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Log>> GetLogLevelsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Log>> GetLogSourcesAsync(CancellationToken cancellationToken = default);
        Task<Log> ExportLogsAsync(string? projectId, DateTime? startDate, DateTime? endDate, string? format, CancellationToken cancellationToken = default);
        Task<Log> DeleteOldLogsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<IEnumerable<Log>> GetLogsAsync(string? projectId, string? level, string? source, DateTime? startDate, DateTime? endDate, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<Log>> GetLogsAsync(DateTime startDate, DateTime endDate, string? level, string? source, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetLogCountAsync(string? projectId, string? level, string? source, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
        Task<Log> ExportLogsAsync(string? projectId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Log> ExportLogsAsync(string? projectId, DateTime? startDate, DateTime? endDate, string? format, string? level, CancellationToken cancellationToken = default);
    }

    // Audit Services
    public interface IAuditService
    {
        Task<Audit> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Audit> CreateAsync(Audit audit, CancellationToken cancellationToken = default);
        Task<Audit> UpdateAsync(Audit audit, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Audit>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<Audit>> GetAuditLogsAsync(string tenantId, string? action, string? resource, DateTime? startDate, DateTime? endDate, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetAuditLogCountAsync(string tenantId, string? action, string? resource, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetAuditStatsAsync(string tenantId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    }

    // Configuration Services
    public interface IConfigService
    {
        Task<Config> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Config> CreateAsync(Config config, CancellationToken cancellationToken = default);
        Task<Config> UpdateAsync(Config config, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Config>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<Config> UpdateConfigAsync(string id, string? name, string? value, string? description, Dictionary<string, object> settings, CancellationToken cancellationToken = default);
    }

    // System Services
    public partial interface ISystemService
    {
        Task<NotifyXStudio.Core.Models.System> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.System> CreateAsync(NotifyXStudio.Core.Models.System system, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.System> UpdateAsync(NotifyXStudio.Core.Models.System system, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.System>> ListAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<bool> DismissSystemAlertAsync(string alertId, CancellationToken cancellationToken = default);
        Task<Config> GetSystemConfigAsync(CancellationToken cancellationToken = default);
        Task<bool> UpdateSystemConfigAsync(Config config, CancellationToken cancellationToken = default);
        Task<bool> RestartSystemAsync(CancellationToken cancellationToken = default);
        Task<bool> ShutdownSystemAsync(CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<NotifyXStudio.Core.Models.System> GetSystemInfoAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Log>> GetSystemLogsAsync(string? level, string? source, string? message, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetSystemLogCountAsync(string? level, string? source, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.System> GetSystemStatusAsync(CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.System> GetSystemMetricsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.System>> GetSystemLogsAsync(CancellationToken cancellationToken = default);
        Task<int> GetSystemLogCountAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.System>> GetSystemAlertsAsync(CancellationToken cancellationToken = default);
    }

    // Status Services
    public partial interface IStatusService
    {
        Task<Status> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Status> CreateAsync(Status status, CancellationToken cancellationToken = default);
        Task<Status> UpdateAsync(Status status, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Status>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Status> GetPerformanceStatusAsync(CancellationToken cancellationToken = default);
        Task<Status> GetSecurityStatusAsync(CancellationToken cancellationToken = default);
        Task<Status> GetComplianceStatusAsync(CancellationToken cancellationToken = default);
        Task<Status> GetMaintenanceStatusAsync(CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<Status> GetStatusAsync(CancellationToken cancellationToken = default);
        Task<Status> GetComponentStatusAsync(string componentId, CancellationToken cancellationToken = default);
        Task<Status> GetServiceStatusAsync(string serviceId, CancellationToken cancellationToken = default);
        Task<Status> GetDatabaseStatusAsync(CancellationToken cancellationToken = default);
        Task<Status> GetQueueStatusAsync(CancellationToken cancellationToken = default);
        Task<Status> GetExternalServiceStatusAsync(string serviceName, CancellationToken cancellationToken = default);
    }

    // Monitor Services
    public partial interface IMonitorService
    {
        Task<NotifyXStudio.Core.Models.Monitor> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> CreateAsync(NotifyXStudio.Core.Models.Monitor monitor, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> UpdateAsync(NotifyXStudio.Core.Models.Monitor monitor, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.Monitor>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> GetMonitoringConfigAsync(CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> UpdateMonitoringConfigAsync(NotifyXStudio.Core.Models.Monitor config, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> GetMonitoringDashboardAsync(CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> GetMonitoringMetricsAsync(CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> GetMonitoringAlertsAsync(CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> GetMonitoringThresholdsAsync(CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> UpdateMonitoringThresholdsAsync(NotifyXStudio.Core.Models.Monitor thresholds, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> GetMonitoringReportsAsync(CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> GenerateMonitoringReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<NotifyXStudio.Core.Models.Monitor> GetMonitoringMetricsAsync(string? projectId, string? metricType, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> GetMonitoringReportsAsync(string? projectId, string? reportType, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> GenerateMonitoringReportAsync(string? projectId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Monitor> UpdateMonitoringThresholdsAsync(Dictionary<string, object> thresholds, CancellationToken cancellationToken = default);
    }

    // Alert Services
    public interface IAlertService
    {
        Task<Alert> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Alert> CreateAsync(Alert alert, CancellationToken cancellationToken = default);
        Task<Alert> UpdateAsync(Alert alert, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Alert>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    }

    // Report Services
    public interface IReportService
    {
        Task<Report> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Report> CreateAsync(Report report, CancellationToken cancellationToken = default);
        Task<Report> UpdateAsync(Report report, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Report>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Report> GenerateReportAsync(Report report, CancellationToken cancellationToken = default);
        Task<Report> GetReportAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Report>> ListReportsAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetReportCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<Report> DownloadReportAsync(string id, CancellationToken cancellationToken = default);
        Task<Report> DeleteReportAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Report>> GetReportTypesAsync(CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<IEnumerable<Report>> GetReportTemplatesAsync(CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<Report> GenerateReportAsync(string name, string type, string? projectId, string? description, CancellationToken cancellationToken = default);
    }

    // Dashboard Services
    public interface IDashboardService
    {
        Task<Dashboard> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Dashboard> CreateAsync(Dashboard dashboard, CancellationToken cancellationToken = default);
        Task<Dashboard> UpdateAsync(Dashboard dashboard, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Dashboard>> ListAsync(string? userId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetDashboardDataAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Dashboard>> GetDashboardWidgetsAsync(string userId, CancellationToken cancellationToken = default);
        Task<Dashboard> UpdateDashboardLayoutAsync(string userId, Dictionary<string, object> layout, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetDashboardMetricsAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Alert>> GetDashboardAlertsAsync(string userId, CancellationToken cancellationToken = default);
    }

    // Integration Services
    public interface IIntegrationService
    {
        Task<Integration> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Integration> CreateAsync(Integration integration, CancellationToken cancellationToken = default);
        Task<Integration> UpdateAsync(Integration integration, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Integration>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<Integration>> ListIntegrationsAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetIntegrationCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<Integration> UpdateIntegrationAsync(string id, string? name, string? description, string? status, string? config, CancellationToken cancellationToken = default);
        Task<Integration> DeleteIntegrationAsync(string id, CancellationToken cancellationToken = default);
        Task<Integration> TestIntegrationAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Integration>> GetIntegrationLogsAsync(string id, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetIntegrationLogCountAsync(string id, CancellationToken cancellationToken = default);
        Task<Integration> GetIntegrationStatsAsync(string id, CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<Integration> CreateIntegrationAsync(string name, string? description, string? type, string? config, string? projectId, CancellationToken cancellationToken = default);
        Task<Integration> GetIntegrationAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Integration>> ListIntegrationsAsync(Guid? projectId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetIntegrationCountAsync(Guid? projectId, string? status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Integration>> GetIntegrationLogsAsync(string id, string? level, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetIntegrationLogCountAsync(string id, string? level, CancellationToken cancellationToken = default);
        Task<Integration> GetIntegrationStatsAsync(string? projectId, string? type, CancellationToken cancellationToken = default);
    }

    // Webhook Services
    public partial interface IWebhookService
    {
        Task<Webhook> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Webhook> CreateAsync(Webhook webhook, CancellationToken cancellationToken = default);
        Task<Webhook> UpdateAsync(Webhook webhook, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Webhook>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Webhook> CreateWebhookAsync(string url, string? secret, string? events, string? projectId, string? description, CancellationToken cancellationToken = default);
        Task<Webhook> UpdateWebhookAsync(string id, string? url, string? secret, string? events, string? description, CancellationToken cancellationToken = default);
        Task<Webhook> UpdateWebhookAsync(string id, string? url, string? secret, List<string> events, string? description, Dictionary<string, string> headers, CancellationToken cancellationToken = default);
        Task<IEnumerable<Webhook>> GetWebhookLogsAsync(string id, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<Webhook>> GetWebhookLogsAsync(string id, string? level, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetWebhookLogCountAsync(string id, CancellationToken cancellationToken = default);
        Task<int> GetWebhookLogCountAsync(string id, string? level, CancellationToken cancellationToken = default);
        
        // Additional methods for controller compatibility
        Task<Webhook> CreateWebhookAsync(Webhook webhook, CancellationToken cancellationToken = default);
        Task<Webhook> GetWebhookAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Webhook>> ListWebhooksAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetWebhookCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<Webhook> UpdateWebhookAsync(Webhook webhook, CancellationToken cancellationToken = default);
        Task<bool> DeleteWebhookAsync(string id, CancellationToken cancellationToken = default);
        Task<Webhook> TestWebhookAsync(string webhookId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Webhook>> GetWebhookLogsAsync(string webhookId, CancellationToken cancellationToken = default);
    }

    // Queue Services
    public partial interface IQueueService
    {
        Task<Queue> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Queue> CreateAsync(Queue queue, CancellationToken cancellationToken = default);
        Task<Queue> UpdateAsync(Queue queue, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Queue>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Queue> GetQueueInfoAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Queue>> ListQueuesAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Queue> GetQueueStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Queue>> GetQueueMessagesAsync(string id, CancellationToken cancellationToken = default);
        Task<Queue> PurgeQueueAsync(string id, CancellationToken cancellationToken = default);
        Task<Queue> PauseQueueAsync(string id, CancellationToken cancellationToken = default);
        Task<Queue> ResumeQueueAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Queue>> GetDeadLetterQueueMessagesAsync(string id, CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<IEnumerable<Queue>> ReplayDeadLetterQueueMessagesAsync(string id, CancellationToken cancellationToken = default);
    }

    // Repository Services
    public partial interface IRepositoryService
    {
        Task<Repository> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Repository> CreateAsync(Repository repository, CancellationToken cancellationToken = default);
        Task<Repository> UpdateAsync(Repository repository, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Repository>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Repository> CreateRepositoryAsync(string name, string? description, string? url, string? projectId, CancellationToken cancellationToken = default);
        Task<Repository> GetRepositoryAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Repository>> ListRepositoriesAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetRepositoryCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<Repository> UpdateRepositoryAsync(string id, string? name, string? description, string? url, string? status, CancellationToken cancellationToken = default);
        Task<Repository> DeleteRepositoryAsync(string id, CancellationToken cancellationToken = default);
        Task<Repository> GetRepositoryStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Repository>> GetRepositoryBranchesAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Repository>> GetRepositoryCommitsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Repository>> GetRepositoryFilesAsync(string id, CancellationToken cancellationToken = default);
        Task<Repository> GetRepositoryStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Repository>> GetRepositoryTypesAsync(CancellationToken cancellationToken = default);
    }

    // Branch Services
    public interface IBranchService
    {
        Task<Branch> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Branch> CreateAsync(Branch branch, CancellationToken cancellationToken = default);
        Task<Branch> UpdateAsync(Branch branch, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Branch>> ListAsync(string? repositoryId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Branch> CreateBranchAsync(string name, string? description, string? repositoryId, string? parentBranchId, CancellationToken cancellationToken = default);
        Task<Branch> GetBranchAsync(string branchId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Branch>> ListBranchesAsync(string repositoryId, string? branchType, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetBranchCountAsync(string repositoryId, string? branchType, CancellationToken cancellationToken = default);
        Task<Branch> UpdateBranchAsync(string branchId, string? name, string? description, string? status, CancellationToken cancellationToken = default);
        Task DeleteBranchAsync(string branchId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetBranchStatusAsync(string branchId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Commit>> GetBranchCommitsAsync(string branchId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<Build>> GetBranchBuildsAsync(string branchId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deploy>> GetBranchDeploymentsAsync(string branchId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetBranchStatsAsync(string branchId, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetBranchTypesAsync(CancellationToken cancellationToken = default);
    }

    // Commit Services
    public interface ICommitService
    {
        Task<Commit> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Commit> CreateAsync(Commit commit, CancellationToken cancellationToken = default);
        Task<Commit> UpdateAsync(Commit commit, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Commit>> ListAsync(string? repositoryId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Commit> CreateCommitAsync(string repositoryId, string message, string? author, string? branchId, List<string>? files, CancellationToken cancellationToken = default);
        Task<Commit> GetCommitAsync(string commitId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Commit>> ListCommitsAsync(string repositoryId, string? branchId, string? author, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetCommitCountAsync(string repositoryId, string? branchId, string? author, CancellationToken cancellationToken = default);
        Task<Commit> UpdateCommitAsync(string commitId, string? message, string? status, CancellationToken cancellationToken = default);
        Task DeleteCommitAsync(string commitId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetCommitStatusAsync(string commitId, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.File>> GetCommitFilesAsync(string commitId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Build>> GetCommitBuildsAsync(string commitId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deploy>> GetCommitDeploymentsAsync(string commitId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetCommitStatsAsync(string commitId, CancellationToken cancellationToken = default);
    }

    // Build Services
    public interface IBuildService
    {
        Task<Build> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Build> CreateAsync(Build build, CancellationToken cancellationToken = default);
        Task<Build> UpdateAsync(Build build, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Build>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<IEnumerable<Build>> GetBuildTypesAsync(CancellationToken cancellationToken = default);
        Task<Build> CancelBuildAsync(string id, CancellationToken cancellationToken = default);
        Task<Build> DeleteBuildAsync(string id, CancellationToken cancellationToken = default);
        Task<Build> BuildAsync(string projectId, string? branchId, string? buildType, Dictionary<string, object>? buildConfig, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetBuildStatusAsync(string buildId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Build>> ListBuildsAsync(string projectId, string? buildType, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetBuildCountAsync(string projectId, string? buildType, string? status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Log>> GetBuildLogsAsync(string buildId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<Build>> GetBuildArtifactsAsync(string buildId, CancellationToken cancellationToken = default);
        Task<byte[]> DownloadBuildArtifactAsync(string buildId, string artifactId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetBuildStatsAsync(string projectId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetProjectsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Branch>> GetBranchesAsync(string projectId, CancellationToken cancellationToken = default);
    }

    // Deploy Services
    public interface IDeployService
    {
        Task<Deploy> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Deploy> CreateAsync(Deploy deploy, CancellationToken cancellationToken = default);
        Task<Deploy> UpdateAsync(Deploy deploy, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deploy>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<Deploy> GetDeploymentStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deploy>> GetEnvironmentsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deploy>> GetVersionsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deploy>> GetComponentsAsync(string id, CancellationToken cancellationToken = default);
        Task<Deploy> CancelDeploymentAsync(string id, CancellationToken cancellationToken = default);
        Task<Deploy> RollbackDeploymentAsync(string id, CancellationToken cancellationToken = default);
        Task<Deploy> DeleteDeploymentAsync(string id, CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<IEnumerable<Deploy>> ListDeploymentsAsync(string? projectId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetDeploymentCountAsync(string? projectId, string? status, CancellationToken cancellationToken = default);
        Task<Deploy> DeployAsync(string projectId, string? environmentId, string? version, Dictionary<string, object>? deployConfig, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetDeploymentStatusAsync(string deploymentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deploy>> GetDeploymentLogsAsync(string id, CancellationToken cancellationToken = default);
    }

    // Environment Services
    public interface IEnvironmentService
    {
        Task<NotifyXStudio.Core.Models.Environment> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Environment> CreateAsync(NotifyXStudio.Core.Models.Environment environment, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Environment> UpdateAsync(NotifyXStudio.Core.Models.Environment environment, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.Environment>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<IEnumerable<NotifyXStudio.Core.Models.Environment>> ListEnvironmentsAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetEnvironmentCountAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Environment> UpdateEnvironmentAsync(string id, string? name, string? description, string? status, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Environment> UpdateEnvironmentAsync(string id, string? name, string? description, string? status, string? config, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Environment> DeleteEnvironmentAsync(string id, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Environment> GetEnvironmentStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.Environment>> GetEnvironmentResourcesAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.Environment>> GetEnvironmentDeploymentsAsync(string id, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Environment> GetEnvironmentStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.Environment>> GetEnvironmentTypesAsync(CancellationToken cancellationToken = default);
    }

    // Test Services
    public partial interface ITestService
    {
        Task<Test> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Test> CreateAsync(Test test, CancellationToken cancellationToken = default);
        Task<Test> UpdateAsync(Test test, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Test>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Test> RunTestsAsync(string testId, CancellationToken cancellationToken = default);
        Task<Test> RunTestsAsync(List<string> testIds, List<string>? testSuites, List<string>? testTypes, Dictionary<string, object>? testConfig, CancellationToken cancellationToken = default);
        Task<Test> GetTestRunStatusAsync(string testRunId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Test>> ListTestRunsAsync(string? testId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<Test>> ListTestRunsAsync(string? testId, DateTime? startDate, DateTime? endDate, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetTestRunCountAsync(string? testId = null, CancellationToken cancellationToken = default);
        Task<int> GetTestRunCountAsync(string? testId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Test>> GetTestResultsAsync(string testRunId, CancellationToken cancellationToken = default);
        Task<Test> GetTestStatsAsync(string testId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetTestStatsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Test>> GetTestTypesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Test>> GetTestSuitesAsync(string? projectId = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Test>> GetTestCasesAsync(string? testSuiteId = null, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<Test> RunTestsAsync(string testId, string? environment, string? config, CancellationToken cancellationToken = default);
        Task<IEnumerable<Test>> ListTestRunsAsync(string? testId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetTestRunCountAsync(string? testId, string? status, CancellationToken cancellationToken = default);
        Task<Test> GetTestResultsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<bool> CancelTestRunAsync(string testRunId, CancellationToken cancellationToken = default);
        Task<bool> DeleteTestRunAsync(string testRunId, CancellationToken cancellationToken = default);
    }

    // Version Services
    public partial interface IVersionService
    {
        Task<NotifyXStudio.Core.Models.Version> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Version> CreateAsync(NotifyXStudio.Core.Models.Version version, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Version> UpdateAsync(NotifyXStudio.Core.Models.Version version, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.Version>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<NotifyXStudio.Core.Models.Version> GetVersionAsync(CancellationToken cancellationToken = default);
        // Additional methods required by controllers
        Task<IEnumerable<NotifyXStudio.Core.Models.Version>> GetComponentVersionsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.Version>> GetDependencyVersionsAsync(CancellationToken cancellationToken = default);
        Task<BuildInfo> GetBuildInfoAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.Version>> GetVersionHistoryAsync(CancellationToken cancellationToken = default);
        Task<RuntimeInfo> GetRuntimeInfoAsync(CancellationToken cancellationToken = default);
        Task<EnvironmentInfo> GetEnvironmentInfoAsync(CancellationToken cancellationToken = default);
        Task<UpdateInfo> GetUpdateInfoAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<NotifyXStudio.Core.Models.Version>> GetVersionHistoryAsync(string componentId, CancellationToken cancellationToken = default);
        Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
        
    }

    // Backup Services
    public interface IBackupService
    {
        Task<Backup> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Backup> CreateAsync(Backup backup, CancellationToken cancellationToken = default);
        Task<Backup> UpdateAsync(Backup backup, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Backup>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Backup> CreateBackupAsync(CancellationToken cancellationToken = default);
        Task<Backup> GetBackupInfoAsync(string backupId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Backup>> ListBackupsAsync(string tenantId, string? backupType, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetBackupCountAsync(string tenantId, string? backupType, CancellationToken cancellationToken = default);
        Task<Backup> RestoreFromBackupAsync(string backupId, CancellationToken cancellationToken = default);
        Task DeleteBackupAsync(string backupId, CancellationToken cancellationToken = default);
        Task<byte[]> DownloadBackupAsync(string backupId, CancellationToken cancellationToken = default);
    }

    // Compliance Services
    public interface IComplianceService
    {
        Task<Compliance> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Compliance> CreateAsync(Compliance compliance, CancellationToken cancellationToken = default);
        Task<Compliance> UpdateAsync(Compliance compliance, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Compliance>> ListAsync(string? projectId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetComplianceStatusAsync(string projectId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Compliance>> GetComplianceViolationsAsync(string projectId, string? violationType, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetComplianceViolationCountAsync(string projectId, string? violationType, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetComplianceMetricsAsync(string projectId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
        Task<byte[]> GenerateComplianceReportAsync(string projectId, string reportType, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Compliance>> GetCompliancePoliciesAsync(string projectId, CancellationToken cancellationToken = default);
        Task UpdateCompliancePoliciesAsync(string projectId, List<Compliance> policies, CancellationToken cancellationToken = default);
    }

    // Credential Services
    public interface ICredentialService
    {
        Task<Credential> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Credential> CreateAsync(Credential credential, CancellationToken cancellationToken = default);
        Task<Credential> UpdateAsync(Credential credential, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Credential>> ListAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    }

    // Workflow Execution Services
    public partial interface IWorkflowExecutionService
    {
        Task<WorkflowExecution> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecution> CreateAsync(WorkflowExecution execution, CancellationToken cancellationToken = default);
        Task<WorkflowExecution> UpdateAsync(WorkflowExecution execution, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecution>> ListAsync(string? workflowId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<WorkflowExecution> CreateWorkflowExecutionAsync(string workflowId, string? status, string? input, string? output, string? errorMessage, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecution>> ListWorkflowExecutionsAsync(string? workflowId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionCountAsync(string? workflowId = null, CancellationToken cancellationToken = default);
        Task<WorkflowExecution> UpdateWorkflowExecutionAsync(WorkflowExecution execution, CancellationToken cancellationToken = default);
        Task<WorkflowExecution> DeleteWorkflowExecutionAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecution> GetWorkflowExecutionStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecution>> GetWorkflowExecutionIssuesAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecution> GetWorkflowExecutionStatsAsync(string id, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<WorkflowExecution> CreateWorkflowExecutionAsync(string workflowId, string? status, Dictionary<string, object> input, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecution>> ListWorkflowExecutionsAsync(string? workflowId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionCountAsync(string? workflowId, string? status, CancellationToken cancellationToken = default);
        Task<WorkflowExecution> UpdateWorkflowExecutionAsync(string id, string? status, string? input, string? output, string? errorMessage, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
    }

    // Workflow Execution Node Services
    public partial interface IWorkflowExecutionNodeService
    {
        Task<WorkflowExecutionNode> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionNode> CreateAsync(WorkflowExecutionNode node, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionNode> UpdateAsync(WorkflowExecutionNode node, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionNode>> ListAsync(string? executionId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionNode> CreateWorkflowExecutionNodeAsync(string executionId, string? nodeType, string? status, string? input, string? output, string? errorMessage, DateTime? startedAt, DateTime? completedAt, string? metadata, string? description, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionNode>> ListWorkflowExecutionNodesAsync(string? executionId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionNodeCountAsync(string? executionId = null, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionNode> UpdateWorkflowExecutionNodeAsync(WorkflowExecutionNode node, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionNode> DeleteWorkflowExecutionNodeAsync(string id, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<WorkflowExecutionNode> CreateWorkflowExecutionNodeAsync(string executionId, string? nodeType, string? status, string? input, string? output, string? errorMessage, string? startedAt, string? completedAt, Dictionary<string, object> metadata, string? description, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionNode>> ListWorkflowExecutionNodesAsync(string? executionId, string? status, string? nodeType, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionNode> UpdateWorkflowExecutionNodeAsync(string id, string? nodeType, string? status, string? input, string? output, string? errorMessage, string? startedAt, string? completedAt, Dictionary<string, object> metadata, string? description, CancellationToken cancellationToken = default);
    }

    // Workflow Execution Edge Services
    public partial interface IWorkflowExecutionEdgeService
    {
        Task<WorkflowExecutionEdge> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionEdge> CreateAsync(WorkflowExecutionEdge edge, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionEdge> UpdateAsync(WorkflowExecutionEdge edge, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionEdge>> ListAsync(string? executionId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionEdge> CreateWorkflowExecutionEdgeAsync(string sourceNodeId, string targetNodeId, string? condition, string? executionId, string? status, string? errorMessage, DateTime? executedAt, string? metadata, string? description, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionEdge> CreateWorkflowExecutionEdgeAsync(string sourceNodeId, string targetNodeId, string? condition, string? executionId, string? status, string? errorMessage, DateTime? executedAt, DateTime? completedAt, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionEdge>> ListWorkflowExecutionEdgesAsync(string? executionId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionEdge>> ListWorkflowExecutionEdgesAsync(string? executionId, string? sourceNodeId, string? targetNodeId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionEdgeCountAsync(string? executionId, string? status, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionEdgeCountAsync(string? executionId, string? sourceNodeId, string? targetNodeId, string? status, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionEdge> UpdateWorkflowExecutionEdgeAsync(WorkflowExecutionEdge edge, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionEdge> UpdateWorkflowExecutionEdgeAsync(string id, string? status, string? errorMessage, DateTime? executedAt, DateTime? completedAt, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionEdge> DeleteWorkflowExecutionEdgeAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionEdge> GetWorkflowExecutionEdgeStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionEdge>> GetWorkflowExecutionEdgeIssuesAsync(string id, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<WorkflowExecutionEdge> CreateWorkflowExecutionEdgeAsync(string sourceNodeId, string targetNodeId, string? condition, string? executionId, string? status, string? errorMessage, string? executedAt, string? completedAt, string? metadata, string? description, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionEdge>> ListWorkflowExecutionEdgesAsync(string? executionId, string? status, string? condition, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionEdge> UpdateWorkflowExecutionEdgeAsync(string id, string? sourceNodeId, string? targetNodeId, string? condition, string? status, string? errorMessage, string? executedAt, string? completedAt, string? metadata, string? description, CancellationToken cancellationToken = default);
        
        // Additional methods for controller compatibility
        Task<WorkflowExecutionEdge> CreateWorkflowExecutionEdgeAsync(WorkflowExecutionEdge edge, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionEdge> GetWorkflowExecutionEdgeAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionEdge> GetWorkflowExecutionEdgeStatsAsync(string edgeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionEdge>> GetWorkflowExecutionEdgeTypesAsync(CancellationToken cancellationToken = default);
    }

    // Workflow Execution Log Services
    public partial interface IWorkflowExecutionLogService
    {
        Task<WorkflowExecutionLog> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionLog> CreateAsync(WorkflowExecutionLog log, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionLog> UpdateAsync(WorkflowExecutionLog log, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionLog>> ListAsync(string? executionId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        
        // Additional methods needed by controllers
        Task<WorkflowExecutionLog> CreateWorkflowExecutionLogAsync(string executionId, string? level, string? message, string? data, string? timestamp, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionLog> CreateWorkflowExecutionLogAsync(string executionId, string? level, string? message, string? data, string? timestamp, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionLog> UpdateWorkflowExecutionLogAsync(string id, string? level, string? message, string? data, string? timestamp, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionLog> UpdateWorkflowExecutionLogAsync(string id, string? level, string? message, string? data, string? timestamp, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
    }

    // Workflow Execution Trigger Services
    public partial interface IWorkflowExecutionTriggerService
    {
        Task<WorkflowExecutionTrigger> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTrigger> CreateAsync(WorkflowExecutionTrigger trigger, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTrigger> UpdateAsync(WorkflowExecutionTrigger trigger, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTrigger>> ListAsync(string? executionId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTrigger> CreateWorkflowExecutionTriggerAsync(string executionId, string? triggerType, string? triggerData, string? status, string? errorMessage, DateTime? triggeredAt, string? metadata, string? description, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTrigger>> ListWorkflowExecutionTriggersAsync(string? executionId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTrigger>> ListWorkflowExecutionTriggersAsync(string? executionId, string? triggerType, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionTriggerCountAsync(string? executionId = null, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionTriggerCountAsync(string? executionId, string? triggerType, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTrigger> UpdateWorkflowExecutionTriggerAsync(string id, string? triggerType, string? triggerData, string? status, string? errorMessage, DateTime? triggeredAt, string? metadata, string? description, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<WorkflowExecutionTrigger> CreateWorkflowExecutionTriggerAsync(string executionId, string? triggerType, string? triggerData, string? status, string? errorMessage, Dictionary<string, object> metadata, string? description, CancellationToken cancellationToken = default);
        
        // Additional methods for controller compatibility
        Task<WorkflowExecutionTrigger> CreateWorkflowExecutionTriggerAsync(WorkflowExecutionTrigger trigger, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTrigger> GetWorkflowExecutionTriggerAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTrigger> UpdateWorkflowExecutionTriggerAsync(WorkflowExecutionTrigger trigger, CancellationToken cancellationToken = default);
        Task<bool> DeleteWorkflowExecutionTriggerAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTrigger> GetWorkflowExecutionTriggerStatusAsync(string triggerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTrigger>> GetWorkflowExecutionTriggerIssuesAsync(string triggerId, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTrigger> GetWorkflowExecutionTriggerStatsAsync(string triggerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTrigger>> GetWorkflowExecutionTriggerTypesAsync(CancellationToken cancellationToken = default);
    }

    // Workflow Execution Trigger Log Services
    public partial interface IWorkflowExecutionTriggerLogService
    {
        Task<WorkflowExecutionTriggerLog> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLog> CreateAsync(WorkflowExecutionTriggerLog log, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLog> UpdateAsync(WorkflowExecutionTriggerLog log, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTriggerLog>> ListAsync(string? triggerId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLog> CreateWorkflowExecutionTriggerLogAsync(string triggerId, string? status, string? message, string? data, string? timestamp, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTriggerLog>> ListWorkflowExecutionTriggerLogsAsync(string? triggerId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionTriggerLogCountAsync(string? triggerId = null, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLog> UpdateWorkflowExecutionTriggerLogAsync(WorkflowExecutionTriggerLog log, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<IEnumerable<WorkflowExecutionTriggerLog>> ListWorkflowExecutionTriggerLogsAsync(string? triggerId, string? status, string? level, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionTriggerLogCountAsync(string? triggerId, string? status, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLog> UpdateWorkflowExecutionTriggerLogAsync(string id, string? status, string? message, string? data, string? level, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLog> UpdateWorkflowExecutionTriggerLogAsync(string id, string? status, string? message, string? data, string? level, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLog> UpdateWorkflowExecutionTriggerLogAsync(string id, string? status, string? message, string? data, string? level, string? metadata, CancellationToken cancellationToken = default);
        
        // Additional methods for controller compatibility
        Task<WorkflowExecutionTriggerLog> CreateWorkflowExecutionTriggerLogAsync(WorkflowExecutionTriggerLog log, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLog> GetWorkflowExecutionTriggerLogAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> DeleteWorkflowExecutionTriggerLogAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLog> GetWorkflowExecutionTriggerLogStatusAsync(string logId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTriggerLog>> GetWorkflowExecutionTriggerLogIssuesAsync(string logId, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLog> GetWorkflowExecutionTriggerLogStatsAsync(string logId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTriggerLog>> GetWorkflowExecutionTriggerLogLevelsAsync(CancellationToken cancellationToken = default);
    }

    // Workflow Execution Trigger Log Entry Services
    public partial interface IWorkflowExecutionTriggerLogEntryService
    {
        Task<WorkflowExecutionTriggerLogEntry> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLogEntry> CreateAsync(WorkflowExecutionTriggerLogEntry entry, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLogEntry> UpdateAsync(WorkflowExecutionTriggerLogEntry entry, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTriggerLogEntry>> ListAsync(string? logId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLogEntry> CreateWorkflowExecutionTriggerLogEntryAsync(string logId, string? level, string? message, string? data, string? timestamp, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTriggerLogEntry>> ListWorkflowExecutionTriggerLogEntriesAsync(string? logId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionTriggerLogEntryCountAsync(string? logId = null, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLogEntry> UpdateWorkflowExecutionTriggerLogEntryAsync(WorkflowExecutionTriggerLogEntry entry, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLogEntry> UpdateWorkflowExecutionTriggerLogEntryAsync(string id, string? level, string? message, string? data, string? timestamp, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLogEntry> UpdateWorkflowExecutionTriggerLogEntryAsync(string id, string? level, string? message, string? data, string? timestamp, string? metadata, CancellationToken cancellationToken = default);
        
        // Additional methods for controller compatibility
        Task<WorkflowExecutionTriggerLogEntry> CreateWorkflowExecutionTriggerLogEntryAsync(WorkflowExecutionTriggerLogEntry entry, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLogEntry> GetWorkflowExecutionTriggerLogEntryAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> DeleteWorkflowExecutionTriggerLogEntryAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLogEntry> GetWorkflowExecutionTriggerLogEntryStatusAsync(string entryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTriggerLogEntry>> GetWorkflowExecutionTriggerLogEntryIssuesAsync(string entryId, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionTriggerLogEntry> GetWorkflowExecutionTriggerLogEntryStatsAsync(string entryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionTriggerLogEntry>> GetWorkflowExecutionTriggerLogEntryLevelsAsync(CancellationToken cancellationToken = default);
    }

    // Workflow Node Services
    public partial interface IWorkflowNodeService
    {
        Task<WorkflowNode> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowNode> CreateAsync(WorkflowNode node, CancellationToken cancellationToken = default);
        Task<WorkflowNode> UpdateAsync(WorkflowNode node, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowNode>> ListAsync(string? workflowId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<WorkflowNode> CreateWorkflowNodeAsync(string name, string? description, string? nodeType, CancellationToken cancellationToken = default);
        Task<WorkflowNode> CreateWorkflowNodeAsync(string name, string? description, string? nodeType, string? workflowId, string? config, CancellationToken cancellationToken = default);
        Task<WorkflowNode> GetWorkflowNodeAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowNode>> ListWorkflowNodesAsync(string? workflowId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowNode>> ListWorkflowNodesAsync(string? workflowId, string? nodeType, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowNodeCountAsync(string? workflowId = null, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowNodeCountAsync(string? workflowId, string? nodeType, CancellationToken cancellationToken = default);
        Task<WorkflowNode> UpdateWorkflowNodeAsync(string id, string? name, string? description, string? nodeType, CancellationToken cancellationToken = default);
        Task<WorkflowNode> UpdateWorkflowNodeAsync(string id, string? name, string? description, string? nodeType, string? config, string? status, CancellationToken cancellationToken = default);
        Task<bool> DeleteWorkflowNodeAsync(string id, CancellationToken cancellationToken = default);
    }

    // Workflow Edge Services
    public partial interface IWorkflowEdgeService
    {
        Task<WorkflowEdge> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowEdge> CreateAsync(WorkflowEdge edge, CancellationToken cancellationToken = default);
        Task<WorkflowEdge> UpdateAsync(WorkflowEdge edge, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowEdge>> ListAsync(string? workflowId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<WorkflowEdge> CreateWorkflowEdgeAsync(string sourceNodeId, string targetNodeId, string? condition, string? workflowId, string? description, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowEdge>> ListWorkflowEdgesAsync(string? workflowId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowEdgeCountAsync(string? workflowId = null, CancellationToken cancellationToken = default);
        Task<WorkflowEdge> UpdateWorkflowEdgeAsync(WorkflowEdge edge, CancellationToken cancellationToken = default);
        Task<WorkflowEdge> DeleteWorkflowEdgeAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowEdge> GetWorkflowEdgeStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowEdge>> GetWorkflowEdgeIssuesAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowEdge> GetWorkflowEdgeStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowEdge>> GetWorkflowEdgeTypesAsync(CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<IEnumerable<WorkflowEdge>> ListWorkflowEdgesAsync(string? workflowId, string? condition, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowEdgeCountAsync(string? workflowId, string? condition, CancellationToken cancellationToken = default);
        Task<WorkflowEdge> UpdateWorkflowEdgeAsync(string id, string? sourceNodeId, string? targetNodeId, string? condition, string? description, CancellationToken cancellationToken = default);
    }

    // Workflow Trigger Services
    public interface IWorkflowTriggerService
    {
        Task<WorkflowTrigger> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowTrigger> CreateAsync(WorkflowTrigger trigger, CancellationToken cancellationToken = default);
        Task<WorkflowTrigger> UpdateAsync(WorkflowTrigger trigger, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowTrigger>> ListAsync(string? workflowId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    }

    // Workflow Run Services
    public interface IWorkflowRunService
    {
        Task<WorkflowRun> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowRun> CreateAsync(WorkflowRun run, CancellationToken cancellationToken = default);
        Task<WorkflowRun> UpdateAsync(WorkflowRun run, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowRun>> ListAsync(string? workflowId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<WorkflowRun> CreateWorkflowRunAsync(WorkflowRun workflowRun, CancellationToken cancellationToken = default);
        Task<WorkflowRun> CreateWorkflowRunAsync(string workflowId, string? name, string? description, CancellationToken cancellationToken = default);
        Task<WorkflowRun> GetWorkflowRunAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowRun>> ListWorkflowRunsAsync(string? workflowId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowRun>> ListWorkflowRunsAsync(string? workflowId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowRunCountAsync(string? workflowId = null, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowRunCountAsync(string? workflowId, string? status, CancellationToken cancellationToken = default);
        Task<WorkflowRun> UpdateWorkflowRunAsync(string id, string? name, string? description, string? status, string? tags, CancellationToken cancellationToken = default);
        Task<WorkflowRun> UpdateWorkflowRunAsync(string id, string? name, string? description, string? status, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
        Task<bool> DeleteWorkflowRunAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowRun> GetWorkflowRunStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowRun>> GetWorkflowRunIssuesAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowRun> GetWorkflowRunStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowRun>> GetWorkflowRunTypesAsync(CancellationToken cancellationToken = default);
    }

    // Additional interfaces for missing services
    public partial interface IWorkflowNodeService
    {
        Task<Status> GetWorkflowNodeStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Issue>> GetWorkflowNodeIssuesAsync(string id, CancellationToken cancellationToken = default);
        Task<Stats> GetWorkflowNodeStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Type>> GetWorkflowNodeTypesAsync(CancellationToken cancellationToken = default);
    }

    // Missing service interfaces that need to be added to existing services
    public partial interface IReleaseService
    {
        Task<bool> UnpublishReleaseAsync(string releaseId, CancellationToken cancellationToken = default);
        Task<Release> GetReleaseStatsAsync(string releaseId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Release>> GetReleaseComponentsAsync(string releaseId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Release>> GetReleaseDeploymentsAsync(string releaseId, CancellationToken cancellationToken = default);
    }




    public partial interface ITenantService
    {
        Task<Tenant> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
        Task<Tenant> CreateTenantAsync(string name, string? description, string? plan, CancellationToken cancellationToken = default);
        Task<Tenant> GetTenantAsync(string id, CancellationToken cancellationToken = default);
        Task<Tenant> UpdateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
        Task<Tenant> UpdateTenantAsync(string id, string? name, string? description, Dictionary<string, object>? settings, CancellationToken cancellationToken = default);
        Task<bool> DeleteTenantAsync(string id, CancellationToken cancellationToken = default);
        Task<Tenant> GetTenantSettingsAsync(string tenantId, CancellationToken cancellationToken = default);
        Task<Tenant> UpdateTenantSettingsAsync(Tenant tenant, CancellationToken cancellationToken = default);
        Task<Tenant> GetTenantLimitsAsync(string tenantId, CancellationToken cancellationToken = default);
        Task<Tenant> UpdateTenantLimitsAsync(Tenant tenant, CancellationToken cancellationToken = default);
    }


    public partial interface IWorkflowService
    {
        // Additional workflow-specific methods
        Task<Workflow> CreateWorkflowAsync(Workflow workflow, string? description = null, string? status = null, string? tags = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Workflow>> ListWorkflowsAsync(string? projectId = null, string? status = null, string? tags = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowCountAsync(string? projectId = null, string? status = null, CancellationToken cancellationToken = default);
        Task<Workflow> UpdateWorkflowAsync(Workflow workflow, string? description = null, string? status = null, string? tags = null, CancellationToken cancellationToken = default);
        Task<Workflow> CreateWorkflowAsync(string name, string? description, string? status, string? tags, string? projectId, CancellationToken cancellationToken = default);
        
        // Additional overloads for controller compatibility
        Task<Workflow> CreateWorkflowAsync(string name, string? description, string? status, string? tags, string? projectId, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
        Task<Workflow> UpdateWorkflowAsync(string id, string? name, string? description, string? status, string? tags, List<WorkflowNode>? nodes, List<WorkflowEdge>? edges, List<WorkflowTrigger>? triggers, CancellationToken cancellationToken = default);
        Task<Workflow> UpdateWorkflowAsync(string id, string? name, string? description, string? status, string? tags, CancellationToken cancellationToken = default);
    }

    public partial interface IWorkflowEdgeService
    {
        Task<WorkflowEdge> CreateWorkflowEdgeAsync(WorkflowEdge edge, CancellationToken cancellationToken = default);
        Task<WorkflowEdge> CreateWorkflowEdgeAsync(string sourceNodeId, string targetNodeId, string? condition, string? workflowId, string? description, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default);
        Task<WorkflowEdge> GetWorkflowEdgeAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowEdge>> ListWorkflowEdgesAsync(string? workflowId, string? sourceNodeId, string? targetNodeId, string? condition, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowEdgeCountAsync(string? workflowId, string? sourceNodeId, string? targetNodeId, CancellationToken cancellationToken = default);
        Task<WorkflowEdge> UpdateWorkflowEdgeAsync(string id, string? condition, string? description, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default);
    }

    public partial interface IWorkflowExecutionService
    {
        Task<WorkflowExecution> CreateWorkflowExecutionAsync(WorkflowExecution execution, CancellationToken cancellationToken = default);
        Task<WorkflowExecution> GetWorkflowExecutionAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecution>> GetWorkflowExecutionTypesAsync(CancellationToken cancellationToken = default);
    }


    public partial interface IWorkflowExecutionNodeService
    {
        Task<WorkflowExecutionNode> CreateWorkflowExecutionNodeAsync(WorkflowExecutionNode node, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionNode> CreateWorkflowExecutionNodeAsync(string executionId, string nodeId, string? status, string? input, string? output, string? errorMessage, DateTime? startedAt, DateTime? completedAt, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionNode> GetWorkflowExecutionNodeAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionNode> GetWorkflowExecutionNodeStatusAsync(string nodeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionNode>> GetWorkflowExecutionNodeIssuesAsync(string nodeId, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionNode> GetWorkflowExecutionNodeStatsAsync(string nodeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionNode>> GetWorkflowExecutionNodeTypesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionNode>> ListWorkflowExecutionNodesAsync(string? executionId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionNodeCountAsync(string? executionId, string? status, CancellationToken cancellationToken = default);
    }

    public partial interface IWorkflowExecutionLogService
    {
        Task<WorkflowExecutionLog> CreateWorkflowExecutionLogAsync(WorkflowExecutionLog log, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionLog> CreateWorkflowExecutionLogAsync(string executionId, string? level, string? message, string? source, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionLog> GetWorkflowExecutionLogAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionLog>> ListWorkflowExecutionLogsAsync(string? executionId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionLog>> ListWorkflowExecutionLogsAsync(string? executionId, string? level, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionLogCountAsync(string? executionId = null, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowExecutionLogCountAsync(string? executionId, string? level, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionLog> UpdateWorkflowExecutionLogAsync(WorkflowExecutionLog log, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionLog> UpdateWorkflowExecutionLogAsync(string id, string? level, string? message, Dictionary<string, object>? metadata, CancellationToken cancellationToken = default);
        Task<bool> DeleteWorkflowExecutionLogAsync(string id, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionLog> GetWorkflowExecutionLogStatusAsync(string logId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionLog>> GetWorkflowExecutionLogIssuesAsync(string logId, CancellationToken cancellationToken = default);
        Task<WorkflowExecutionLog> GetWorkflowExecutionLogStatsAsync(string logId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionLog>> GetWorkflowExecutionLogLevelsAsync(CancellationToken cancellationToken = default);
    }




}
