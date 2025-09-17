using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Runtime.Services
{
    /// <summary>
    /// Service for evaluating expressions and templates in workflow nodes.
    /// </summary>
    public interface IExpressionEngine
    {
        /// <summary>
        /// Evaluates an expression with the given context variables.
        /// </summary>
        Task<object> EvaluateAsync(string expression, Dictionary<string, object> context);

        /// <summary>
        /// Validates an expression without executing it.
        /// </summary>
        Task<ValidationResult> ValidateExpressionAsync(string expression);

        /// <summary>
        /// Resolves template variables in a string.
        /// </summary>
        Task<string> ResolveTemplateAsync(string template, Dictionary<string, object> context);

        /// <summary>
        /// Gets available functions and variables for expression building.
        /// </summary>
        Task<ExpressionContext> GetExpressionContextAsync();
    }

    /// <summary>
    /// Context for expression evaluation.
    /// </summary>
    public class ExpressionContext
    {
        public Dictionary<string, object> Variables { get; init; } = new();
        public List<ExpressionFunction> Functions { get; init; } = new();
        public List<string> AvailableTypes { get; init; } = new();
    }

    /// <summary>
    /// A function available in expressions.
    /// </summary>
    public class ExpressionFunction
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public List<ExpressionParameter> Parameters { get; init; } = new();
        public string ReturnType { get; init; } = string.Empty;
    }

    /// <summary>
    /// A parameter for an expression function.
    /// </summary>
    public class ExpressionParameter
    {
        public string Name { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public bool Required { get; init; }
        public string Description { get; init; } = string.Empty;
    }
}