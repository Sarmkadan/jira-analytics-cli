// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Middleware;

/// <summary>
/// Fluent builder for constructing the middleware pipeline.
/// Manages middleware registration and execution order for clean architecture.
/// </summary>
public class PipelineBuilder
{
    private readonly List<Func<Func<PipelineContext, Task>, Func<PipelineContext, Task>>> _middlewares = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PipelineBuilder> _logger;

    public PipelineBuilder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<PipelineBuilder>>();
    }

    /// <summary>
    /// Registers a middleware component in the pipeline.
    /// Middleware executes in registration order (first registered = outermost layer).
    /// </summary>
    public PipelineBuilder Use<TMiddleware>(params object[] args) where TMiddleware : class
    {
        _middlewares.Add(next =>
        {
            var middlewareInstance = ActivatorUtilities.CreateInstance<TMiddleware>(_serviceProvider, args);
            var method = typeof(TMiddleware).GetMethod("InvokeAsync");

            if (method == null)
                throw new InvalidOperationException($"Middleware {typeof(TMiddleware).Name} must have InvokeAsync method");

            return async context =>
            {
                await (Task)method.Invoke(middlewareInstance, new object[] { context })!;
            };
        });

        _logger.LogDebug("Middleware registered: {MiddlewareName}", typeof(TMiddleware).Name);
        return this;
    }

    /// <summary>
    /// Builds the final middleware pipeline and returns the entry point.
    /// Combines all registered middleware in the correct order.
    /// </summary>
    public Func<PipelineContext, Task> Build(Func<PipelineContext, Task> finalHandler)
    {
        Func<PipelineContext, Task> pipeline = finalHandler;

        // Build middleware chain in reverse order (last registered = innermost)
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            pipeline = middleware(pipeline);
        }

        _logger.LogInformation("Middleware pipeline built with {Count} components", _middlewares.Count);
        return pipeline;
    }
}
