namespace NotifyXStudio.Api.Configuration;

/// <summary>
/// Feature flags for controlling application behavior
/// </summary>
public static class FeatureFlags
{
    /// <summary>
    /// Whether to enable stub service endpoints (returns 501 Not Implemented)
    /// </summary>
    public const string EnableStubEndpoints = "EnableStubEndpoints";
    
    /// <summary>
    /// Whether to enable real notification services
    /// </summary>
    public const string EnableRealNotifications = "EnableRealNotifications";
    
    /// <summary>
    /// Whether to enable real workflow services
    /// </summary>
    public const string EnableRealWorkflows = "EnableRealWorkflows";
    
    /// <summary>
    /// Whether to enable real audit services
    /// </summary>
    public const string EnableRealAudit = "EnableRealAudit";
}