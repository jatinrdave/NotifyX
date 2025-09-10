using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for testing operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly ITestService _testService;

        public TestController(ILogger<TestController> logger, ITestService testService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _testService = testService ?? throw new ArgumentNullException(nameof(testService));
        }

        /// <summary>
        /// Runs system tests.
        /// </summary>
        [HttpPost("run")]
        public async Task<IActionResult> RunTests([FromBody] RunTestsRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Test request is required");
                }

                var testRunId = await _testService.RunTestsAsync(
                    request.TestTypes,
                    request.TestSuites,
                    request.TestCases,
                    request.Parameters);

                return Ok(new
                {
                    testRunId,
                    message = "Test run initiated successfully",
                    startedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run tests: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to run tests",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets test run status.
        /// </summary>
        [HttpGet("runs/{testRunId}")]
        public async Task<IActionResult> GetTestRunStatus(string testRunId)
        {
            try
            {
                var testRun = await _testService.GetTestRunStatusAsync(testRunId);

                if (testRun == null)
                {
                    return NotFound(new
                    {
                        error = "Test run not found",
                        testRunId
                    });
                }

                return Ok(testRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get test run status for {TestRunId}: {Message}", testRunId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve test run status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists test runs.
        /// </summary>
        [HttpGet("runs")]
        public async Task<IActionResult> ListTestRuns(
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow;

                var testRuns = await _testService.ListTestRunsAsync(status, start, end, page, pageSize);
                var totalCount = await _testService.GetTestRunCountAsync(status, start, end);

                return Ok(new
                {
                    testRuns,
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
                _logger.LogError(ex, "Failed to list test runs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list test runs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets test results.
        /// </summary>
        [HttpGet("runs/{testRunId}/results")]
        public async Task<IActionResult> GetTestResults(string testRunId)
        {
            try
            {
                var testResults = await _testService.GetTestResultsAsync(testRunId);

                return Ok(new
                {
                    testRunId,
                    testResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get test results for {TestRunId}: {Message}", testRunId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve test results",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets test statistics.
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetTestStats(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var stats = await _testService.GetTestStatsAsync(start, end);

                return Ok(new
                {
                    dateRange = new { start, end },
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get test stats: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve test statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available test types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetTestTypes()
        {
            try
            {
                var testTypes = await _testService.GetTestTypesAsync();

                return Ok(new
                {
                    testTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get test types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve test types",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available test suites.
        /// </summary>
        [HttpGet("suites")]
        public async Task<IActionResult> GetTestSuites()
        {
            try
            {
                var testSuites = await _testService.GetTestSuitesAsync();

                return Ok(new
                {
                    testSuites
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get test suites: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve test suites",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available test cases.
        /// </summary>
        [HttpGet("cases")]
        public async Task<IActionResult> GetTestCases([FromQuery] string? testSuite)
        {
            try
            {
                var testCases = await _testService.GetTestCasesAsync(testSuite);

                return Ok(new
                {
                    testCases
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get test cases: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve test cases",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Cancels a test run.
        /// </summary>
        [HttpPost("runs/{testRunId}/cancel")]
        public async Task<IActionResult> CancelTestRun(string testRunId)
        {
            try
            {
                await _testService.CancelTestRunAsync(testRunId);

                return Ok(new
                {
                    message = "Test run cancelled successfully",
                    testRunId,
                    cancelledAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel test run {TestRunId}: {Message}", testRunId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to cancel test run",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a test run.
        /// </summary>
        [HttpDelete("runs/{testRunId}")]
        public async Task<IActionResult> DeleteTestRun(string testRunId)
        {
            try
            {
                await _testService.DeleteTestRunAsync(testRunId);

                return Ok(new
                {
                    message = "Test run deleted successfully",
                    testRunId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete test run {TestRunId}: {Message}", testRunId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete test run",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Run tests request model.
    /// </summary>
    public class RunTestsRequest
    {
        /// <summary>
        /// Test types to run.
        /// </summary>
        public List<string> TestTypes { get; set; } = new();

        /// <summary>
        /// Test suites to run.
        /// </summary>
        public List<string> TestSuites { get; set; } = new();

        /// <summary>
        /// Specific test cases to run.
        /// </summary>
        public List<string> TestCases { get; set; } = new();

        /// <summary>
        /// Test parameters.
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }
    }
}