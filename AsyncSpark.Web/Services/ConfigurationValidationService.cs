namespace AsyncSpark.Web.Services;

/// <summary>
/// Service for validating application configuration
/// </summary>
public interface IConfigurationValidationService
{
    /// <summary>Validates the application configuration and returns a result with warnings and errors.</summary>
    ConfigurationValidationResult ValidateConfiguration();
}

/// <summary>
/// Configuration validation result
/// </summary>
public class ConfigurationValidationResult
{
    /// <summary>Gets or sets a value indicating whether the configuration is valid (no errors).</summary>
    public bool IsValid { get; set; } = true;

    /// <summary>Gets or sets the list of non-fatal configuration errors.</summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>Gets or sets the list of configuration warnings for optional settings.</summary>
    public List<string> Warnings { get; set; } = [];
}

/// <summary>
/// Validates application configuration.
/// All optional settings produce warnings; nothing here is treated as a fatal error
/// so the site always starts regardless of missing or misconfigured values.
/// </summary>
public class ConfigurationValidationService : IConfigurationValidationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationValidationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public ConfigurationValidationService(IConfiguration configuration, ILogger<ConfigurationValidationService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates the application configuration. Warnings indicate missing optional config;
    /// errors indicate unexpected problems. Neither prevents the site from starting.
    /// </summary>
    /// <returns>A <see cref="ConfigurationValidationResult"/> with warnings and errors.</returns>
    public ConfigurationValidationResult ValidateConfiguration()
    {
        var result = new ConfigurationValidationResult { IsValid = true };

        try
        {
            CheckWeatherApiKey(result);
            CheckConnectionString(result);
            CheckAsyncSection(result);
        }
        catch (Exception ex)
        {
            // Validation itself must not throw — degrade gracefully
            result.Warnings.Add($"Configuration validation encountered an unexpected error: {ex.Message}");
            _logger.LogError(ex, "Unexpected error during configuration validation");
        }

        LogResults(result);
        return result;
    }

    private void CheckWeatherApiKey(ConfigurationValidationResult result)
    {
        var key = _configuration["OpenWeatherMapApiKey"];
        if (string.IsNullOrWhiteSpace(key) || key == "KEYMISSING")
            result.Warnings.Add("OpenWeatherMapApiKey is not configured — weather endpoints will return errors.");
    }

    private void CheckConnectionString(ConfigurationValidationResult result)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // No database configured — this project does not require one, so it's informational only
            _logger.LogDebug("No DefaultConnection string configured — running without a database");
            return;
        }

        // A connection string is present; verify it is parseable (does not open a connection)
        try
        {
            if (!connectionString.Contains('='))
                result.Warnings.Add("DefaultConnection string appears malformed (no '=' found).");
            else
                _logger.LogInformation("DefaultConnection string is present and parseable");
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"DefaultConnection string could not be parsed: {ex.Message}");
        }
    }

    private void CheckAsyncSection(ConfigurationValidationResult result)
    {
        if (!_configuration.GetSection("Async").Exists())
            result.Warnings.Add("Async configuration section is missing — default values will be used.");
    }

    private void LogResults(ConfigurationValidationResult result)
    {
        foreach (var warning in result.Warnings)
            _logger.LogWarning("Configuration warning: {Warning}", warning);

        foreach (var error in result.Errors)
            _logger.LogError("Configuration error: {Error}", error);

        if (result.Errors.Count != 0)
            result.IsValid = false;

        _logger.LogInformation(
            "Configuration validation complete — valid: {IsValid}, warnings: {Warnings}, errors: {Errors}",
            result.IsValid, result.Warnings.Count, result.Errors.Count);
    }
}
