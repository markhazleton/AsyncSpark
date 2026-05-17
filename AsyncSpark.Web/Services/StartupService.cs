namespace AsyncSpark.Web.Services;

/// <summary>
/// Service that runs during application startup to perform initialization tasks.
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// Executes startup tasks. Implementations must not throw — failures are logged and the site starts in degraded mode.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of startup service — all failures are logged and degraded, never fatal.
/// The site must always start regardless of configuration or cache issues.
/// </summary>
public class StartupService : IStartupService
{
    private readonly IConfigurationValidationService _configValidationService;
    private readonly ILogger<StartupService> _logger;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupService"/> class.
    /// </summary>
    /// <param name="configValidationService">The configuration validation service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="memoryCache">The memory cache instance.</param>
    public StartupService(
        IConfigurationValidationService configValidationService,
        ILogger<StartupService> logger,
        IMemoryCache memoryCache)
    {
        _configValidationService = configValidationService ?? throw new ArgumentNullException(nameof(configValidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    /// <summary>
    /// Executes startup tasks: validates configuration and pre-warms the cache.
    /// Neither step is fatal — the site starts in degraded mode if either fails.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting application initialization...");

        ValidateConfiguration();
        await PreWarmCacheAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Application initialization completed");
    }

    private void ValidateConfiguration()
    {
        try
        {
            var configResult = _configValidationService.ValidateConfiguration();

            foreach (var warning in configResult.Warnings)
                _logger.LogWarning("Startup configuration warning: {Warning}", warning);

            foreach (var error in configResult.Errors)
                _logger.LogError("Startup configuration error (non-fatal): {Error}", error);

            if (!configResult.IsValid)
                _logger.LogWarning("Configuration validation reported errors — site will start in degraded mode");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration validation threw unexpectedly — site will start in degraded mode");
        }
    }

    private async Task PreWarmCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            var applicationStatus = new ApplicationStatus(Assembly.GetExecutingAssembly());
            _memoryCache.Set("ApplicationStatusCache", applicationStatus, TimeSpan.FromHours(24));
            _logger.LogInformation("Application status cached successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache pre-warm failed — site will start without pre-warmed cache");
        }

        try
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Startup cancelled during initialization");
        }
    }
}

/// <summary>
/// Hosted service that runs the startup service.
/// Startup failures are logged but never allowed to prevent the host from starting.
/// </summary>
public class StartupHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StartupHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupHostedService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="logger">The logger instance.</param>
    public StartupHostedService(IServiceProvider serviceProvider, ILogger<StartupHostedService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// Exceptions are caught and logged — the host is never crashed by a startup service failure.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var startupService = scope.ServiceProvider.GetRequiredService<IStartupService>();
            await startupService.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Normal — host is shutting down before fully starting
            _logger.LogWarning("Startup service was cancelled");
        }
        catch (Exception ex)
        {
            // Do NOT re-throw — re-throwing here crashes the host process.
            // The site starts in degraded mode; /health will surface the problem.
            _logger.LogError(ex, "Startup service failed — application will run in degraded mode");
        }
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Startup hosted service stopping");
        return Task.CompletedTask;
    }
}
