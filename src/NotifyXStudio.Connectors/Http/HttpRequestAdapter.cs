using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Connectors;
using NotifyXStudio.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace NotifyXStudio.Connectors.Http
{
    /// <summary>
    /// Adapter for making HTTP requests to REST APIs.
    /// </summary>
    public class HttpRequestAdapter : IConnectorAdapter
    {
        public string Type => "http.request";

        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpRequestAdapter> _logger;

        public HttpRequestAdapter(HttpClient httpClient, ILogger<HttpRequestAdapter> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConnectorExecutionResult> ExecuteAsync(ConnectorExecutionContext context, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation("Executing HTTP request for TenantId: {TenantId}", context.TenantId);

                // Parse configuration
                var config = ParseConfig(context.NodeConfig);
                var inputs = ParseInputs(context.Inputs);

                // Build HTTP request
                var request = BuildHttpRequest(config, inputs);

                // Execute request
                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                // Parse response
                var result = new
                {
                    statusCode = (int)response.StatusCode,
                    headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
                    body = responseBody,
                    json = TryParseJson(responseBody),
                    duration = (long)duration,
                    success = response.IsSuccessStatusCode
                };

                _logger.LogInformation("HTTP request completed with status {StatusCode} in {Duration}ms", 
                    response.StatusCode, duration);

                return new ConnectorExecutionResult
                {
                    Success = response.IsSuccessStatusCode,
                    Output = JsonSerializer.SerializeToElement(result),
                    ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {response.StatusCode}: {responseBody}",
                    DurationMs = (long)duration,
                    Metadata = new Dictionary<string, object>
                    {
                        ["statusCode"] = (int)response.StatusCode,
                        ["method"] = config.Method,
                        ["url"] = config.Url
                    }
                };
            }
            catch (HttpRequestException ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "HTTP request failed after {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"HTTP request failed: {ex.Message}",
                    DurationMs = (long)duration
                };
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "HTTP request timed out after {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = false,
                    ErrorMessage = "HTTP request timed out",
                    DurationMs = (long)duration
                };
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "HTTP request failed with unexpected error after {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"Unexpected error: {ex.Message}",
                    DurationMs = (long)duration
                };
            }
        }

        private HttpRequestConfig ParseConfig(JsonElement nodeConfig)
        {
            var config = new HttpRequestConfig();

            if (nodeConfig.TryGetProperty("url", out var urlProp))
                config.Url = urlProp.GetString() ?? "";

            if (nodeConfig.TryGetProperty("method", out var methodProp))
                config.Method = methodProp.GetString() ?? "GET";

            if (nodeConfig.TryGetProperty("headers", out var headersProp))
                config.Headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersProp.GetRawText()) ?? new();

            if (nodeConfig.TryGetProperty("body", out var bodyProp))
                config.Body = bodyProp.GetString() ?? "";

            if (nodeConfig.TryGetProperty("timeout", out var timeoutProp))
                config.Timeout = timeoutProp.GetInt32();

            if (nodeConfig.TryGetProperty("followRedirects", out var redirectProp))
                config.FollowRedirects = redirectProp.GetBoolean();

            if (nodeConfig.TryGetProperty("ignoreSSL", out var sslProp))
                config.IgnoreSSL = sslProp.GetBoolean();

            return config;
        }

        private Dictionary<string, object> ParseInputs(JsonElement inputs)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(inputs.GetRawText()) ?? new();
        }

        private HttpRequestMessage BuildHttpRequest(HttpRequestConfig config, Dictionary<string, object> inputs)
        {
            // Resolve URL with input variables
            var resolvedUrl = ResolveTemplate(config.Url, inputs);
            var request = new HttpRequestMessage(new HttpMethod(config.Method), resolvedUrl);

            // Add headers
            foreach (var header in config.Headers)
            {
                var resolvedValue = ResolveTemplate(header.Value, inputs);
                request.Headers.Add(header.Key, resolvedValue);
            }

            // Add body for POST, PUT, PATCH requests
            if (ShouldHaveBody(config.Method) && !string.IsNullOrEmpty(config.Body))
            {
                var resolvedBody = ResolveTemplate(config.Body, inputs);
                request.Content = new StringContent(resolvedBody, Encoding.UTF8, "application/json");
            }

            return request;
        }

        private bool ShouldHaveBody(string method)
        {
            return method.ToUpperInvariant() is "POST" or "PUT" or "PATCH";
        }

        private string ResolveTemplate(string template, Dictionary<string, object> inputs)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var result = template;
            foreach (var input in inputs)
            {
                var placeholder = $"{{{{{input.Key}}}}}";
                var value = input.Value?.ToString() ?? "";
                result = result.Replace(placeholder, value);
            }

            return result;
        }

        private object? TryParseJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<object>(json);
            }
            catch
            {
                return null;
            }
        }

        private class HttpRequestConfig
        {
            public string Url { get; set; } = "";
            public string Method { get; set; } = "GET";
            public Dictionary<string, string> Headers { get; set; } = new();
            public string Body { get; set; } = "";
            public int Timeout { get; set; } = 30000;
            public bool FollowRedirects { get; set; } = true;
            public bool IgnoreSSL { get; set; } = false;
        }
    }
}