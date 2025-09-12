using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Connectors;
using NotifyXStudio.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace NotifyXStudio.Connectors.Slack
{
    /// <summary>
    /// Adapter for sending messages to Slack.
    /// </summary>
    public class SlackSendMessageAdapter : IConnectorAdapter
    {
        public string Type => "slack.sendMessage";

        private readonly HttpClient _httpClient;
        private readonly ILogger<SlackSendMessageAdapter> _logger;

        public SlackSendMessageAdapter(HttpClient httpClient, ILogger<SlackSendMessageAdapter> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConnectorExecutionResult> ExecuteAsync(ConnectorExecutionContext context, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation("Executing Slack send message for TenantId: {TenantId}", context.TenantId);

                // Parse configuration
                var config = ParseConfig(context.NodeConfig);
                var inputs = ParseInputs(context.Inputs);

                // Validate required fields
                if (string.IsNullOrEmpty(config.Channel))
                    throw new ArgumentException("Channel is required");

                if (string.IsNullOrEmpty(config.Text) && (config.Blocks == null || config.Blocks.Count == 0))
                    throw new ArgumentException("Either text or blocks is required");

                // Build Slack API request
                var request = BuildSlackRequest(config, inputs, context.CredentialSecret);

                // Execute request
                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                // Parse Slack API response
                var slackResponse = JsonSerializer.Deserialize<SlackApiResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var result = new
                {
                    messageTs = slackResponse?.Ts,
                    channel = slackResponse?.Channel,
                    ok = slackResponse?.Ok ?? false,
                    error = slackResponse?.Error,
                    success = slackResponse?.Ok ?? false,
                    duration = (long)duration
                };

                _logger.LogInformation("Slack message sent successfully in {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = slackResponse?.Ok ?? false,
                    Output = JsonSerializer.SerializeToElement(result),
                    ErrorMessage = slackResponse?.Ok == false ? slackResponse.Error : null,
                    DurationMs = (long)duration,
                    Metadata = new Dictionary<string, object>
                    {
                        ["channel"] = config.Channel,
                        ["messageTs"] = slackResponse?.Ts ?? "",
                        ["slackResponse"] = slackResponse?.Ok ?? false
                    }
                };
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "Slack send message failed after {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"Slack API error: {ex.Message}",
                    DurationMs = (long)duration
                };
            }
        }

        private SlackConfig ParseConfig(JsonElement nodeConfig)
        {
            var config = new SlackConfig();

            if (nodeConfig.TryGetProperty("channel", out var channelProp))
                config.Channel = channelProp.GetString() ?? "";

            if (nodeConfig.TryGetProperty("text", out var textProp))
                config.Text = textProp.GetString() ?? "";

            if (nodeConfig.TryGetProperty("blocks", out var blocksProp))
                config.Blocks = JsonSerializer.Deserialize<List<object>>(blocksProp.GetRawText()) ?? new();

            if (nodeConfig.TryGetProperty("attachments", out var attachmentsProp))
                config.Attachments = JsonSerializer.Deserialize<List<object>>(attachmentsProp.GetRawText()) ?? new();

            if (nodeConfig.TryGetProperty("threadTs", out var threadProp))
                config.ThreadTs = threadProp.GetString() ?? "";

            if (nodeConfig.TryGetProperty("username", out var usernameProp))
                config.Username = usernameProp.GetString() ?? "";

            if (nodeConfig.TryGetProperty("iconEmoji", out var emojiProp))
                config.IconEmoji = emojiProp.GetString() ?? "";

            if (nodeConfig.TryGetProperty("iconUrl", out var iconProp))
                config.IconUrl = iconProp.GetString() ?? "";

            return config;
        }

        private Dictionary<string, object> ParseInputs(JsonElement inputs)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(inputs.GetRawText()) ?? new();
        }

        private HttpRequestMessage BuildSlackRequest(SlackConfig config, Dictionary<string, object> inputs, string? credentialSecret)
        {
            if (string.IsNullOrEmpty(credentialSecret))
                throw new ArgumentException("Slack bot token is required");

            // Resolve template variables
            var resolvedChannel = ResolveTemplate(config.Channel, inputs);
            var resolvedText = ResolveTemplate(config.Text, inputs);

            // Build Slack API payload
            var payload = new Dictionary<string, object>
            {
                ["channel"] = resolvedChannel,
                ["text"] = resolvedText
            };

            if (config.Blocks?.Count > 0)
                payload["blocks"] = config.Blocks;

            if (config.Attachments?.Count > 0)
                payload["attachments"] = config.Attachments;

            if (!string.IsNullOrEmpty(config.ThreadTs))
                payload["thread_ts"] = ResolveTemplate(config.ThreadTs, inputs);

            if (!string.IsNullOrEmpty(config.Username))
                payload["username"] = ResolveTemplate(config.Username, inputs);

            if (!string.IsNullOrEmpty(config.IconEmoji))
                payload["icon_emoji"] = ResolveTemplate(config.IconEmoji, inputs);

            if (!string.IsNullOrEmpty(config.IconUrl))
                payload["icon_url"] = ResolveTemplate(config.IconUrl, inputs);

            // Create HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, "https://slack.com/api/chat.postMessage");
            
            // Add authorization header
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", credentialSecret);
            
            // Add content
            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return request;
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

        private class SlackConfig
        {
            public string Channel { get; set; } = "";
            public string Text { get; set; } = "";
            public List<object> Blocks { get; set; } = new();
            public List<object> Attachments { get; set; } = new();
            public string ThreadTs { get; set; } = "";
            public string Username { get; set; } = "";
            public string IconEmoji { get; set; } = "";
            public string IconUrl { get; set; } = "";
        }

        private class SlackApiResponse
        {
            public bool Ok { get; set; }
            public string? Ts { get; set; }
            public string? Channel { get; set; }
            public string? Error { get; set; }
        }
    }
}