using MySqlConnector;
using System.Data;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Connectors;
using NotifyXStudio.Core.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace NotifyXStudio.Connectors.Database
{
    /// <summary>
    /// Adapter for executing MySQL queries.
    /// </summary>
    public class MySqlQueryAdapter : IConnectorAdapter
    {
        public string Type => "mysql.query";

        private readonly ILogger<MySqlQueryAdapter> _logger;

        public MySqlQueryAdapter(ILogger<MySqlQueryAdapter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConnectorExecutionResult> ExecuteAsync(ConnectorExecutionContext context, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            MySqlConnection? connection = null;
            
            try
            {
                _logger.LogInformation("Executing MySQL query for TenantId: {TenantId}", context.TenantId);

                // Parse configuration
                var config = ParseConfig(context.NodeConfig);
                var inputs = ParseInputs(context.Inputs);

                // Validate required fields
                if (string.IsNullOrEmpty(config.Query))
                    throw new ArgumentException("Query is required");

                // Build connection string from credential secret
                var connectionString = BuildConnectionString(context.CredentialSecret);
                
                // Execute query
                var result = await ExecuteQueryAsync(connectionString, config, inputs, cancellationToken);

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                _logger.LogInformation("MySQL query executed successfully in {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = true,
                    Output = JsonSerializer.SerializeToElement(result),
                    DurationMs = (long)duration,
                    Metadata = new Dictionary<string, object>
                    {
                        ["operation"] = config.Operation,
                        ["affectedRows"] = result.AffectedRows,
                        ["fieldCount"] = result.FieldCount
                    }
                };
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "MySQL query failed after {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"MySQL error: {ex.Message}",
                    DurationMs = (long)duration
                };
            }
            finally
            {
                connection?.Dispose();
            }
        }

        private MySqlConfig ParseConfig(JsonElement nodeConfig)
        {
            var config = new MySqlConfig();

            if (nodeConfig.TryGetProperty("query", out var queryProp))
                config.Query = queryProp.GetString() ?? "";

            if (nodeConfig.TryGetProperty("parameters", out var paramsProp))
                config.Parameters = JsonSerializer.Deserialize<List<object>>(paramsProp.GetRawText()) ?? new();

            if (nodeConfig.TryGetProperty("operation", out var opProp))
                config.Operation = opProp.GetString() ?? "select";

            if (nodeConfig.TryGetProperty("timeout", out var timeoutProp))
                config.Timeout = timeoutProp.GetInt32();

            if (nodeConfig.TryGetProperty("returnFields", out var fieldsProp))
                config.ReturnFields = JsonSerializer.Deserialize<List<string>>(fieldsProp.GetRawText()) ?? new();

            return config;
        }

        private Dictionary<string, object> ParseInputs(JsonElement inputs)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(inputs.GetRawText()) ?? new();
        }

        private string BuildConnectionString(string? credentialSecret)
        {
            if (string.IsNullOrEmpty(credentialSecret))
                throw new ArgumentException("Database connection string is required");

            // In a real implementation, you would decrypt and parse the credential secret
            // For now, assume it's a JSON object with connection details
            var connectionDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(credentialSecret) ?? new();

            var builder = new MySqlConnectionStringBuilder
            {
                Server = connectionDetails.GetValueOrDefault("host", "localhost"),
                Port = uint.Parse(connectionDetails.GetValueOrDefault("port", "3306")),
                Database = connectionDetails.GetValueOrDefault("database", ""),
                UserID = connectionDetails.GetValueOrDefault("username", ""),
                Password = connectionDetails.GetValueOrDefault("password", ""),
                SslMode = connectionDetails.GetValueOrDefault("ssl", "false").ToLower() == "true" 
                    ? MySqlSslMode.Required 
                    : MySqlSslMode.None
            };

            return builder.ConnectionString;
        }

        private async Task<MySqlResult> ExecuteQueryAsync(string connectionString, MySqlConfig config, Dictionary<string, object> inputs, CancellationToken cancellationToken)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Resolve query template
            var resolvedQuery = ResolveTemplate(config.Query, inputs);

            using var command = new MySqlCommand(resolvedQuery, connection);
            command.CommandTimeout = config.Timeout / 1000; // Convert to seconds

            // Add parameters if provided
            for (int i = 0; i < config.Parameters.Count; i++)
            {
                var paramValue = ResolveTemplate(config.Parameters[i]?.ToString() ?? "", inputs);
                command.Parameters.AddWithValue($"@param{i}", paramValue);
            }

            var result = new MySqlResult();

            if (config.Operation.ToLowerInvariant() == "select")
            {
                // Execute SELECT query
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                var rows = new List<Dictionary<string, object>>();

                while (await reader.ReadAsync(cancellationToken))
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var fieldName = reader.GetName(i);
                        var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        row[fieldName] = value ?? DBNull.Value;
                    }
                    rows.Add(row);
                }

                result.Rows = rows;
                result.FieldCount = reader.FieldCount;
            }
            else
            {
                // Execute INSERT, UPDATE, DELETE query
                result.AffectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
                
                // Get last insert ID for INSERT operations
                if (config.Operation.ToLowerInvariant() == "insert")
                {
                    result.InsertId = (long)command.LastInsertedId;
                }
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

        private class MySqlConfig
        {
            public string Query { get; set; } = "";
            public List<object> Parameters { get; set; } = new();
            public string Operation { get; set; } = "select";
            public int Timeout { get; set; } = 30000;
            public List<string> ReturnFields { get; set; } = new();
        }

        private class MySqlResult
        {
            public List<Dictionary<string, object>> Rows { get; set; } = new();
            public int AffectedRows { get; set; }
            public long InsertId { get; set; }
            public int FieldCount { get; set; }
        }
    }
}