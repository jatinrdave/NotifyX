using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Core.Services;

/// <summary>
/// Timezone scheduling service implementation.
/// </summary>
public class TimezoneSchedulingService : ITimezoneSchedulingService
{
    private readonly ILogger<TimezoneSchedulingService> _logger;
    private readonly INotificationService _notificationService;

    public TimezoneSchedulingService(ILogger<TimezoneSchedulingService> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<bool> ScheduleForTimezoneAsync(NotificationEvent notification, string timezone, DateTime localTime, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Scheduling notification for timezone: {Timezone}, local time: {LocalTime}", timezone, localTime);

            var utcTime = await ConvertToUtcAsync(localTime, timezone, cancellationToken);
            
            var scheduledNotification = notification with
            {
                ScheduledFor = utcTime,
                Metadata = new Dictionary<string, object>(notification.Metadata)
                {
                    ["scheduledTimezone"] = timezone,
                    ["scheduledLocalTime"] = localTime
                }
            };

            var result = await _notificationService.ScheduleAsync(scheduledNotification, cancellationToken);
            
            _logger.LogInformation("Scheduled notification {NotificationId} for timezone {Timezone} at {UtcTime} UTC", 
                notification.Id, timezone, utcTime);

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule notification for timezone: {Timezone}", timezone);
            return false;
        }
    }

    public async Task<bool> ScheduleForMultipleTimezonesAsync(NotificationEvent notification, Dictionary<string, DateTime> timezoneSchedules, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Scheduling notification for {Count} timezones", timezoneSchedules.Count);

            var tasks = timezoneSchedules.Select(async kvp =>
            {
                var timezone = kvp.Key;
                var localTime = kvp.Value;
                
                // Create a copy of the notification for each timezone
                var timezoneNotification = notification with
                {
                    Id = $"{notification.Id}-{timezone}",
                    Metadata = new Dictionary<string, object>(notification.Metadata)
                    {
                        ["targetTimezone"] = timezone,
                        ["originalNotificationId"] = notification.Id
                    }
                };

                return await ScheduleForTimezoneAsync(timezoneNotification, timezone, localTime, cancellationToken);
            });

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r);

            _logger.LogInformation("Scheduled notification for {SuccessCount}/{TotalCount} timezones", 
                successCount, timezoneSchedules.Count);

            return successCount == timezoneSchedules.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule notification for multiple timezones");
            return false;
        }
    }

    public async Task<IEnumerable<DateTime>> GetOptimalDeliveryTimesAsync(string timezone, NotificationPriority priority, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting optimal delivery times for timezone: {Timezone}, priority: {Priority}", timezone, priority);

            var timeZoneInfo = await GetTimezoneInfoAsync(timezone, cancellationToken);
            var now = DateTime.UtcNow;
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(now, TimeZoneInfo.FindSystemTimeZoneById(timezone));

            var optimalTimes = new List<DateTime>();

            // Define optimal delivery times based on priority
            var optimalHours = priority switch
            {
                NotificationPriority.Critical => new[] { 9, 10, 11, 14, 15, 16 }, // Multiple times for critical
                NotificationPriority.High => new[] { 9, 10, 14, 15 }, // Business hours
                NotificationPriority.Normal => new[] { 10, 14 }, // Standard times
                NotificationPriority.Low => new[] { 14 }, // Afternoon only
                _ => new[] { 10, 14 }
            };

            // Generate optimal times for the next 7 days
            for (int day = 0; day < 7; day++)
            {
                var targetDate = localNow.Date.AddDays(day);
                
                foreach (var hour in optimalHours)
                {
                    var optimalTime = targetDate.AddHours(hour);
                    
                    // Skip if the time has already passed today
                    if (day == 0 && optimalTime <= localNow)
                    {
                        continue;
                    }

                    // Convert to UTC
                    var utcTime = TimeZoneInfo.ConvertTimeToUtc(optimalTime, TimeZoneInfo.FindSystemTimeZoneById(timezone));
                    optimalTimes.Add(utcTime);
                }
            }

            _logger.LogInformation("Generated {Count} optimal delivery times for timezone {Timezone}", 
                optimalTimes.Count, timezone);

            return optimalTimes.OrderBy(t => t);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get optimal delivery times for timezone: {Timezone}", timezone);
            return new List<DateTime>();
        }
    }

    public async Task<DateTime> ConvertToLocalTimeAsync(DateTime utcTime, string timezone, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZoneInfo);
            
            _logger.LogDebug("Converted UTC time {UtcTime} to local time {LocalTime} for timezone {Timezone}", 
                utcTime, localTime, timezone);

            return localTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert UTC time to local time for timezone: {Timezone}", timezone);
            return utcTime; // Fallback to UTC
        }
    }

    public async Task<DateTime> ConvertToUtcAsync(DateTime localTime, string timezone, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(localTime, timeZoneInfo);
            
            _logger.LogDebug("Converted local time {LocalTime} to UTC time {UtcTime} for timezone {Timezone}", 
                localTime, utcTime, timezone);

            return utcTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert local time to UTC for timezone: {Timezone}", timezone);
            return localTime; // Fallback to original time
        }
    }

    public async Task<TimezoneInfo> GetTimezoneInfoAsync(string timezone, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting timezone info for: {Timezone}", timezone);

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var now = DateTime.UtcNow;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(now, timeZoneInfo);
            var offset = timeZoneInfo.GetUtcOffset(now);

            // Check for daylight saving time
            var isDaylightSaving = timeZoneInfo.IsDaylightSavingTime(now);
            DateTime? daylightStart = null;
            DateTime? daylightEnd = null;

            if (timeZoneInfo.SupportsDaylightSavingTime)
            {
                // Find daylight saving time transitions for current year
                var year = now.Year;
                var transitions = GetDaylightSavingTransitions(timeZoneInfo, year);
                if (transitions.Count >= 2)
                {
                    daylightStart = transitions[0];
                    daylightEnd = transitions[1];
                }
            }

            var timezoneInfo = new TimezoneInfo
            {
                Id = timezone,
                Name = timeZoneInfo.Id,
                DisplayName = timeZoneInfo.DisplayName,
                Offset = offset,
                SupportsDaylightSaving = timeZoneInfo.SupportsDaylightSavingTime,
                DaylightSavingStart = daylightStart,
                DaylightSavingEnd = daylightEnd,
                Metadata = new Dictionary<string, object>
                {
                    ["isDaylightSaving"] = isDaylightSaving,
                    ["currentLocalTime"] = localTime,
                    ["currentUtcTime"] = now
                }
            };

            _logger.LogInformation("Retrieved timezone info for {Timezone}: {DisplayName}, Offset: {Offset}", 
                timezone, timezoneInfo.DisplayName, offset);

            return timezoneInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get timezone info for: {Timezone}", timezone);
            
            // Return default UTC timezone info
            return new TimezoneInfo
            {
                Id = "UTC",
                Name = "UTC",
                DisplayName = "Coordinated Universal Time",
                Offset = TimeSpan.Zero,
                SupportsDaylightSaving = false
            };
        }
    }

    private static List<DateTime> GetDaylightSavingTransitions(TimeZoneInfo timeZoneInfo, int year)
    {
        var transitions = new List<DateTime>();
        
        try
        {
            // This is a simplified approach - in a real implementation, you'd need to
            // properly calculate the exact transition dates based on the timezone rules
            var january = new DateTime(year, 1, 1);
            var july = new DateTime(year, 7, 1);
            
            // Check if daylight saving is active at different times of the year
            var januaryOffset = timeZoneInfo.GetUtcOffset(january);
            var julyOffset = timeZoneInfo.GetUtcOffset(july);
            
            if (januaryOffset != julyOffset)
            {
                // There are daylight saving transitions
                // This is a simplified calculation - real implementation would be more complex
                transitions.Add(new DateTime(year, 3, 1)); // Approximate spring transition
                transitions.Add(new DateTime(year, 11, 1)); // Approximate fall transition
            }
        }
        catch
        {
            // If we can't determine transitions, return empty list
        }

        return transitions;
    }
}