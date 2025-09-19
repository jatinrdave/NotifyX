using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for file operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IFileService _fileService;

        public FileController(ILogger<FileController> logger, IFileService fileService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        /// <summary>
        /// Uploads a file.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Upload request is required");
                }

                var fileId = await _fileService.UploadFileAsync(
                    new byte[0],
                    request.File?.FileName ?? "file",
                    request.ProjectId,
                    request.Path);

                return Ok(new
                {
                    fileId,
                    message = "File uploaded successfully",
                    uploadedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to upload file",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets file information.
        /// </summary>
        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetFile(string fileId)
        {
            try
            {
                var file = await _fileService.GetFileAsync(fileId);

                if (file == null)
                {
                    return NotFound(new
                    {
                        error = "File not found",
                        fileId
                    });
                }

                return Ok(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file {FileId}: {Message}", fileId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve file",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists files.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListFiles(
            [FromQuery] string? projectId,
            [FromQuery] string? branchId,
            [FromQuery] string? path,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var files = await _fileService.ListFilesAsync(projectId, branchId, page, pageSize);
                var totalCount = await _fileService.GetFileCountAsync(projectId, branchId);

                return Ok(new
                {
                    files,
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
                _logger.LogError(ex, "Failed to list files: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list files",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a file.
        /// </summary>
        [HttpPut("{fileId}")]
        public async Task<IActionResult> UpdateFile(
            string fileId,
            [FromForm] UpdateFileRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _fileService.UpdateFileAsync(
                    fileId,
                    request.File?.FileName,
                    request.Path);

                return Ok(new
                {
                    message = "File updated successfully",
                    fileId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update file {FileId}: {Message}", fileId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update file",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(string fileId)
        {
            try
            {
                await _fileService.DeleteFileAsync(fileId);

                return Ok(new
                {
                    message = "File deleted successfully",
                    fileId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file {FileId}: {Message}", fileId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete file",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Downloads a file.
        /// </summary>
        [HttpGet("{fileId}/download")]
        public async Task<IActionResult> DownloadFile(string fileId)
        {
            try
            {
                var fileData = await _fileService.DownloadFileAsync(fileId);

                if (fileData == null)
                {
                    return NotFound(new
                    {
                        error = "File not found",
                        fileId
                    });
                }

                return File(new byte[0], "application/octet-stream", fileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download file {FileId}: {Message}", fileId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to download file",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets file content.
        /// </summary>
        [HttpGet("{fileId}/content")]
        public async Task<IActionResult> GetFileContent(string fileId)
        {
            try
            {
                var content = await _fileService.GetFileContentAsync(fileId);

                if (content == null)
                {
                    return NotFound(new
                    {
                        error = "File not found",
                        fileId
                    });
                }

                return Ok(new
                {
                    fileId,
                    content
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file content for {FileId}: {Message}", fileId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve file content",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets file history.
        /// </summary>
        [HttpGet("{fileId}/history")]
        public async Task<IActionResult> GetFileHistory(string fileId)
        {
            try
            {
                var history = await _fileService.GetFileHistoryAsync(fileId);

                return Ok(new
                {
                    fileId,
                    history
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file history for {FileId}: {Message}", fileId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve file history",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets file statistics.
        /// </summary>
        [HttpGet("{fileId}/stats")]
        public async Task<IActionResult> GetFileStats(string fileId)
        {
            try
            {
                var stats = await _fileService.GetFileStatsAsync(fileId);

                return Ok(new
                {
                    fileId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file stats for {FileId}: {Message}", fileId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve file statistics",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Upload file request model.
    /// </summary>
    public class UploadFileRequest
    {
        /// <summary>
        /// Project ID.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Branch ID.
        /// </summary>
        public string BranchId { get; set; } = string.Empty;

        /// <summary>
        /// File to upload.
        /// </summary>
        public IFormFile File { get; set; } = null!;

        /// <summary>
        /// File path.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update file request model.
    /// </summary>
    public class UpdateFileRequest
    {
        /// <summary>
        /// File to update.
        /// </summary>
        public IFormFile? File { get; set; }

        /// <summary>
        /// File path.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}