using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Core.Services;

/// <summary>
/// Observability service implementation for metrics, tracing, and monitoring.
/// </summary>
public class ObservabilityService : IObservabilityService
{
    private readonly ILogger<ObservabilityService> _logger;
    private readonly Dictionary<string, double> _counters = new();
    private readonly Dictionary<string, double> _gauges = new();
    private readonly Dictionary<string, List<double>> _histograms = new();

    public ObservabilityService(ILogger<ObservabilityService> logger)
    {
        _logger = logger;
    }

    public async Task RecordMetricAsync(string name, double value, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Recording metric: {Name} = {Value}", name, value);

            // Simulate metric recording
            await Task.Delay(1, cancellationToken);

            // In a real implementation, this would send metrics to a monitoring system
            _logger.LogTrace("Metric recorded: {Name} = {Value} with tags: {Tags}", name, value, tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record metric: {Name}", name);
        }
    }

    public async Task IncrementCounterAsync(string name, double increment = 1.0, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Incrementing counter: {Name} by {Increment}", name, increment);

            lock (_counters)
            {
                _counters[name] = _counters.GetValueOrDefault(name, 0) + increment;
            }

            await RecordMetricAsync(name, _counters[name], tags, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to increment counter: {Name}", name);
        }
    }

    public async Task RecordHistogramAsync(string name, double value, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Recording histogram: {Name} = {Value}", name, value);

            lock (_histograms)
            {
                if (!_histograms.ContainsKey(name))
                {
                    _histograms[name] = new List<double>();
                }
                _histograms[name].Add(value);
            }

            await RecordMetricAsync(name, value, tags, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record histogram: {Name}", name);
        }
    }

    public async Task SetGaugeAsync(string name, double value, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Setting gauge: {Name} = {Value}", name, value);

            lock (_gauges)
            {
                _gauges[name] = value;
            }

            await RecordMetricAsync(name, value, tags, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set gauge: {Name}", name);
        }
    }

    public async Task<ISpan> StartSpanAsync(string name, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Starting span: {Name}", name);

            // Simulate span creation
            await Task.Delay(1, cancellationToken);

            var span = new Span
            {
                Id = Guid.NewGuid().ToString(),
                TraceId = Guid.NewGuid().ToString(),
                Name = name,
                StartTime = DateTime.UtcNow,
                Attributes = attributes ?? new Dictionary<string, object>()
            };

            _logger.LogTrace("Span started: {SpanId} in trace {TraceId}", span.Id, span.TraceId);

            return span;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start span: {Name}", name);
            throw;
        }
    }

    public async Task RecordEventAsync(string name, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Recording event: {Name}", name);

            // Simulate event recording
            await Task.Delay(1, cancellationToken);

            _logger.LogTrace("Event recorded: {Name} with attributes: {Attributes}", name, attributes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record event: {Name}", name);
        }
    }

    public async Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting observability service health status");

            // Simulate health check
            await Task.Delay(10, cancellationToken);

            // Check if metrics are being recorded
            var hasMetrics = _counters.Count > 0 || _gauges.Count > 0 || _histograms.Count > 0;
            
            return hasMetrics ? HealthStatus.Healthy : HealthStatus.Degraded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get observability service health status");
            return HealthStatus.Unhealthy;
        }
    }

    public async Task<ServiceMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting service metrics");

            // Simulate metrics collection
            await Task.Delay(10, cancellationToken);

            var histograms = new Dictionary<string, HistogramData>();
            
            lock (_histograms)
            {
                foreach (var kvp in _histograms)
                {
                    var values = kvp.Value;
                    if (values.Count > 0)
                    {
                        histograms[kvp.Key] = new HistogramData
                        {
                            Count = values.Count,
                            Sum = values.Sum(),
                            Min = values.Min(),
                            Max = values.Max(),
                            Mean = values.Average(),
                            Percentiles = new Dictionary<double, double>
                            {
                                [50.0] = CalculatePercentile(values, 0.5),
                                [95.0] = CalculatePercentile(values, 0.95),
                                [99.0] = CalculatePercentile(values, 0.99)
                            }
                        };
                    }
                }
            }

            var metrics = new ServiceMetrics
            {
                Counters = new Dictionary<string, double>(_counters),
                Gauges = new Dictionary<string, double>(_gauges),
                Histograms = histograms
            };

            _logger.LogInformation("Retrieved metrics: {CounterCount} counters, {GaugeCount} gauges, {HistogramCount} histograms",
                metrics.Counters.Count, metrics.Gauges.Count, metrics.Histograms.Count);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service metrics");
            throw;
        }
    }

    private static double CalculatePercentile(List<double> values, double percentile)
    {
        if (values.Count == 0) return 0;
        
        var sortedValues = values.OrderBy(x => x).ToList();
        var index = (int)Math.Ceiling(percentile * sortedValues.Count) - 1;
        return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
    }

    private class Span : ISpan
    {
        public string Id { get; init; } = string.Empty;
        public string TraceId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; private set; }
        public Dictionary<string, object> Attributes { get; init; } = new();
        public List<SpanEvent> Events { get; init; } = new();
        public SpanStatus Status { get; private set; } = SpanStatus.Unset;
        public Exception? Exception { get; private set; }
        public bool IsEnded { get; private set; }

        public void SetAttributes(Dictionary<string, object> attributes)
        {
            foreach (var kvp in attributes)
            {
                Attributes[kvp.Key] = kvp.Value;
            }
        }

        public void AddEvent(string name, Dictionary<string, object>? attributes = null)
        {
            Events.Add(new SpanEvent
            {
                Name = name,
                Timestamp = DateTime.UtcNow,
                Attributes = attributes ?? new Dictionary<string, object>()
            });
        }

        public void SetStatus(SpanStatus status, string? description = null)
        {
            Status = status;
            if (!string.IsNullOrEmpty(description))
            {
                Attributes["status.description"] = description;
            }
        }

        public void RecordException(Exception exception)
        {
            Exception = exception;
            SetStatus(SpanStatus.Error, exception.Message);
            AddEvent("exception", new Dictionary<string, object>
            {
                ["exception.type"] = exception.GetType().Name,
                ["exception.message"] = exception.Message,
                ["exception.stacktrace"] = exception.StackTrace ?? string.Empty
            });
        }

        public void End()
        {
            if (IsEnded) return;
            
            EndTime = DateTime.UtcNow;
            IsEnded = true;
        }

        public void Dispose()
        {
            End();
        }
    }
}