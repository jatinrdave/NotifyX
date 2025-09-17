using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Connectors;
using NotifyXStudio.Core.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Jint;

namespace NotifyXStudio.Connectors.Logic
{
    /// <summary>
    /// Adapter for IF condition logic.
    /// </summary>
    public class IfConditionAdapter : IConnectorAdapter
    {
        public string Type => "if.condition";

        private readonly ILogger<IfConditionAdapter> _logger;

        public IfConditionAdapter(ILogger<IfConditionAdapter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConnectorExecutionResult> ExecuteAsync(ConnectorExecutionContext context, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation("Executing IF condition for TenantId: {TenantId}", context.TenantId);

                // Parse configuration
                var config = ParseConfig(context.NodeConfig);
                var inputs = ParseInputs(context.Inputs);

                // Evaluate condition
                var conditionResult = await EvaluateConditionAsync(config, inputs);

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                // Prepare output based on condition result
                var output = new Dictionary<string, object>
                {
                    ["condition"] = conditionResult,
                    ["true"] = conditionResult ? inputs : new Dictionary<string, object>(),
                    ["false"] = !conditionResult ? inputs : new Dictionary<string, object>(),
                    ["duration"] = (long)duration
                };

                _logger.LogInformation("IF condition evaluated to {Result} in {Duration}ms", conditionResult, duration);

                return new ConnectorExecutionResult
                {
                    Success = true,
                    Output = JsonSerializer.SerializeToElement(output),
                    DurationMs = (long)duration,
                    Metadata = new Dictionary<string, object>
                    {
                        ["conditionResult"] = conditionResult,
                        ["evaluatedAt"] = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "IF condition evaluation failed after {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"Condition evaluation error: {ex.Message}",
                    DurationMs = (long)duration
                };
            }
        }

        private IfConditionConfig ParseConfig(JsonElement nodeConfig)
        {
            var config = new IfConditionConfig();

            if (nodeConfig.TryGetProperty("conditions", out var conditionsProp))
            {
                if (conditionsProp.TryGetProperty("options", out var optionsProp))
                {
                    var options = new ConditionOptions();

                    if (optionsProp.TryGetProperty("caseSensitive", out var caseProp))
                        options.CaseSensitive = caseProp.GetBoolean();

                    if (optionsProp.TryGetProperty("leftValue", out var leftProp))
                        options.LeftValue = leftProp.GetString() ?? "";

                    if (optionsProp.TryGetProperty("rightValue", out var rightProp))
                        options.RightValue = rightProp.GetString() ?? "";

                    if (optionsProp.TryGetProperty("operation", out var opProp))
                        options.Operation = opProp.GetString() ?? "equal";

                    config.Options = options;
                }
            }

            return config;
        }

        private Dictionary<string, object> ParseInputs(JsonElement inputs)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(inputs.GetRawText()) ?? new();
        }

        private async Task<bool> EvaluateConditionAsync(IfConditionConfig config, Dictionary<string, object> inputs)
        {
            if (config.Options == null)
                return false;

            var options = config.Options;

            // Resolve template variables in left and right values
            var leftValue = ResolveTemplate(options.LeftValue, inputs);
            var rightValue = ResolveTemplate(options.RightValue, inputs);

            // Convert values to appropriate types for comparison
            var left = ConvertValue(leftValue);
            var right = ConvertValue(rightValue);

            // Perform comparison based on operation
            return options.Operation.ToLowerInvariant() switch
            {
                "equal" => CompareEqual(left, right, options.CaseSensitive),
                "notEqual" => !CompareEqual(left, right, options.CaseSensitive),
                "contains" => Contains(left, right, options.CaseSensitive),
                "notContains" => !Contains(left, right, options.CaseSensitive),
                "startsWith" => StartsWith(left, right, options.CaseSensitive),
                "endsWith" => EndsWith(left, right, options.CaseSensitive),
                "regex" => RegexMatch(left, right),
                "larger" => CompareNumeric(left, right) > 0,
                "smaller" => CompareNumeric(left, right) < 0,
                "largerEqual" => CompareNumeric(left, right) >= 0,
                "smallerEqual" => CompareNumeric(left, right) <= 0,
                "isEmpty" => IsEmpty(left),
                "isNotEmpty" => !IsEmpty(left),
                _ => false
            };
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

        private object ConvertValue(string value)
        {
            // Try to convert to number
            if (double.TryParse(value, out var number))
                return number;

            // Try to convert to boolean
            if (bool.TryParse(value, out var boolean))
                return boolean;

            // Return as string
            return value;
        }

        private bool CompareEqual(object left, object right, bool caseSensitive)
        {
            if (left is string leftStr && right is string rightStr)
            {
                return caseSensitive 
                    ? leftStr.Equals(rightStr, StringComparison.Ordinal)
                    : leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase);
            }

            return left?.Equals(right) ?? right == null;
        }

        private bool Contains(object left, object right, bool caseSensitive)
        {
            if (left is string leftStr && right is string rightStr)
            {
                return caseSensitive
                    ? leftStr.Contains(rightStr, StringComparison.Ordinal)
                    : leftStr.Contains(rightStr, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private bool StartsWith(object left, object right, bool caseSensitive)
        {
            if (left is string leftStr && right is string rightStr)
            {
                return caseSensitive
                    ? leftStr.StartsWith(rightStr, StringComparison.Ordinal)
                    : leftStr.StartsWith(rightStr, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private bool EndsWith(object left, object right, bool caseSensitive)
        {
            if (left is string leftStr && right is string rightStr)
            {
                return caseSensitive
                    ? leftStr.EndsWith(rightStr, StringComparison.Ordinal)
                    : leftStr.EndsWith(rightStr, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private bool RegexMatch(object left, object right)
        {
            if (left is string leftStr && right is string rightStr)
            {
                try
                {
                    return System.Text.RegularExpressions.Regex.IsMatch(leftStr, rightStr);
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private int CompareNumeric(object left, object right)
        {
            if (left is double leftNum && right is double rightNum)
            {
                return leftNum.CompareTo(rightNum);
            }

            if (left is string leftStr && right is string rightStr)
            {
                if (double.TryParse(leftStr, out var leftNum2) && double.TryParse(rightStr, out var rightNum2))
                {
                    return leftNum2.CompareTo(rightNum2);
                }
            }

            return 0;
        }

        private bool IsEmpty(object value)
        {
            return value switch
            {
                null => true,
                string str => string.IsNullOrWhiteSpace(str),
                System.Collections.ICollection collection => collection.Count == 0,
                _ => false
            };
        }

        private class IfConditionConfig
        {
            public ConditionOptions? Options { get; set; }
        }

        private class ConditionOptions
        {
            public bool CaseSensitive { get; set; } = true;
            public string LeftValue { get; set; } = "";
            public string RightValue { get; set; } = "";
            public string Operation { get; set; } = "equal";
        }
    }
}