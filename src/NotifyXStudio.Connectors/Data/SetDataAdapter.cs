using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Connectors;
using NotifyXStudio.Core.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace NotifyXStudio.Connectors.Data
{
    /// <summary>
    /// Adapter for setting and modifying data fields.
    /// </summary>
    public class SetDataAdapter : IConnectorAdapter
    {
        public string Type => "set.data";

        private readonly ILogger<SetDataAdapter> _logger;

        public SetDataAdapter(ILogger<SetDataAdapter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConnectorExecutionResult> ExecuteAsync(ConnectorExecutionContext context, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation("Executing Set Data for TenantId: {TenantId}", context.TenantId);

                // Parse configuration
                var config = ParseConfig(context.NodeConfig);
                var inputs = ParseInputs(context.Inputs);

                // Process data assignments
                var result = await ProcessDataAssignmentsAsync(config, inputs);

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                _logger.LogInformation("Set Data completed in {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = true,
                    Output = JsonSerializer.SerializeToElement(result),
                    DurationMs = (long)duration,
                    Metadata = new Dictionary<string, object>
                    {
                        ["assignmentsCount"] = config.Assignments.Count,
                        ["processedAt"] = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "Set Data failed after {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"Set Data error: {ex.Message}",
                    DurationMs = (long)duration
                };
            }
        }

        private SetDataConfig ParseConfig(JsonElement nodeConfig)
        {
            var config = new SetDataConfig();

            if (nodeConfig.TryGetProperty("assignments", out var assignmentsProp))
            {
                config.Assignments = JsonSerializer.Deserialize<List<DataAssignment>>(assignmentsProp.GetRawText()) ?? new();
            }

            if (nodeConfig.TryGetProperty("options", out var optionsProp))
            {
                var options = new SetDataOptions();

                if (optionsProp.TryGetProperty("include", out var includeProp))
                    options.Include = includeProp.GetString() ?? "all";

                config.Options = options;
            }

            return config;
        }

        private Dictionary<string, object> ParseInputs(JsonElement inputs)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(inputs.GetRawText()) ?? new();
        }

        private async Task<Dictionary<string, object>> ProcessDataAssignmentsAsync(SetDataConfig config, Dictionary<string, object> inputs)
        {
            var result = new Dictionary<string, object>();

            // Determine what data to include
            switch (config.Options?.Include?.ToLowerInvariant())
            {
                case "assignments":
                    // Only include assigned fields
                    break;
                case "defined":
                    // Include only defined fields from inputs
                    foreach (var input in inputs.Where(kvp => kvp.Value != null))
                    {
                        result[input.Key] = input.Value;
                    }
                    break;
                case "all":
                default:
                    // Include all input data
                    foreach (var input in inputs)
                    {
                        result[input.Key] = input.Value;
                    }
                    break;
            }

            // Process assignments
            foreach (var assignment in config.Assignments)
            {
                if (string.IsNullOrEmpty(assignment.Name))
                    continue;

                // Resolve template in value
                var resolvedValue = ResolveTemplate(assignment.Value, inputs);

                // Convert value to appropriate type
                var convertedValue = ConvertValue(resolvedValue, assignment.Type);

                // Set the value
                result[assignment.Name] = convertedValue;
            }

            return result;
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

        private object ConvertValue(string value, string type)
        {
            return type.ToLowerInvariant() switch
            {
                "number" => ConvertToNumber(value),
                "boolean" => ConvertToBoolean(value),
                "object" => ConvertToObject(value),
                "array" => ConvertToArray(value),
                "date" => ConvertToDate(value),
                "string" or _ => value
            };
        }

        private object ConvertToNumber(string value)
        {
            if (int.TryParse(value, out var intValue))
                return intValue;
            
            if (double.TryParse(value, out var doubleValue))
                return doubleValue;
            
            if (decimal.TryParse(value, out var decimalValue))
                return decimalValue;

            return 0;
        }

        private object ConvertToBoolean(string value)
        {
            if (bool.TryParse(value, out var boolValue))
                return boolValue;

            // Handle common boolean representations
            return value.ToLowerInvariant() switch
            {
                "true" or "1" or "yes" or "on" => true,
                "false" or "0" or "no" or "off" => false,
                _ => false
            };
        }

        private object ConvertToObject(string value)
        {
            try
            {
                return JsonSerializer.Deserialize<object>(value) ?? new object();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        private object ConvertToArray(string value)
        {
            try
            {
                return JsonSerializer.Deserialize<object[]>(value) ?? new object[0];
            }
            catch
            {
                return new object[0];
            }
        }

        private object ConvertToDate(string value)
        {
            if (DateTime.TryParse(value, out var dateValue))
                return dateValue;

            return DateTime.UtcNow;
        }

        private class SetDataConfig
        {
            public List<DataAssignment> Assignments { get; set; } = new();
            public SetDataOptions? Options { get; set; }
        }

        private class DataAssignment
        {
            public string Name { get; set; } = "";
            public string Value { get; set; } = "";
            public string Type { get; set; } = "string";
        }

        private class SetDataOptions
        {
            public string Include { get; set; } = "all";
        }
    }
}