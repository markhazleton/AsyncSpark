using AsyncSpark.HttpGetCall;
using AsyncSpark.Web.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Westwind.AspNetCore.Markdown;
using Microsoft.AspNetCore.HttpOverrides;
using WebSpark.Bootswatch;
using WebSpark.HttpClientUtility;
using WebSpark.HttpClientUtility.RequestResult;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

// Add WebSpark.Bootswatch theme switcher services
Microsoft.Extensions.DependencyInjection.HttpServiceCollectionExtensions.AddHttpContextAccessor(builder.Services);

// Register WebSpark.HttpClientUtility services with caching, telemetry, and Newtonsoft.Json
// This powers both Bootswatch theme fetching and the AsyncSpark.Weather service
builder.Services.AddHttpClientUtility(options =>
{
    options.EnableCaching = true;
    options.EnableTelemetry = true;
    options.UseNewtonsoftJson = true;
});

builder.Services.AddBootswatchThemeSwitcher();

// Configure HttpClient with default timeout and retry policies
builder.Services.AddHttpClient("default", client =>
{
  client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddMemoryCache();

// Register new services
builder.Services.AddScoped<IConfigurationValidationService, ConfigurationValidationService>();
builder.Services.AddScoped<IStartupService, StartupService>();
builder.Services.AddHostedService<StartupHostedService>();

// Enhanced health checks
builder.Services.AddHealthChecks()
    .AddCheck("API Health", () => HealthCheckResult.Healthy("API is healthy"))
    .AddCheck<ConfigurationHealthCheck>("Configuration")
    .AddCheck("Memory Cache", () => HealthCheckResult.Healthy("Memory cache is configured"));

builder.Services.TryAddSingleton<IMemoryCacheManager, MemoryCacheManager>();
builder.Services.AddCustomScalar();
builder.Services.AddMarkdown();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure forwarded headers for reverse proxy scenarios
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddMvc();

// Register HttpGetCallService with proper DI
builder.Services.AddScoped<IHttpGetCallService>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<HttpGetCallService>>();
    var telemetryLogger = serviceProvider.GetRequiredService<ILogger<HttpGetCallServiceTelemetry>>();
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    IHttpGetCallService baseService = new HttpGetCallService(logger, httpClientFactory);
    IHttpGetCallService telemetryService = new HttpGetCallServiceTelemetry(telemetryLogger, baseService);
    return telemetryService;
});

// Register AsyncSpark.Weather service using WebSpark.HttpClientUtility
// Caching, telemetry, and resilience are handled by the HttpClientUtility decorator stack
builder.Services.AddScoped<IWeatherService>(serviceProvider =>
{
    string apiKey = builder.Configuration["OpenWeatherMapApiKey"] ?? "KEYMISSING";
    var requestService = serviceProvider.GetRequiredService<IHttpRequestResultService>();
    var weatherLogger = serviceProvider.GetRequiredService<ILogger<OpenWeatherMapWeatherService>>();
    return new OpenWeatherMapWeatherService(apiKey, requestService, weatherLogger);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
}
else
{
    // Enhanced exception handler for production
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Add HTTP Strict Transport Security
}

// Use forwarded headers for reverse proxy scenarios
app.UseForwardedHeaders();

// Add encoding middleware to ensure proper UTF-8 handling
app.UseEncodingMiddleware();

// Add request logging middleware early in the pipeline
app.UseRequestLogging();

// Add WebSpark.Bootswatch middleware (must be before UseStaticFiles)
app.UseBootswatchAll();

app.UseStaticFiles();
app.UseHttpsRedirection();

// Add security headers (but don't override Content-Type for API responses)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    
    // Only set Content-Type to text/html for non-API routes
    // API routes will set their own Content-Type
    await next();
    
    // After the request is processed, check if it's an HTML response
    if (context.Response.ContentType == null && !context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.Headers["Content-Type"] = "text/html; charset=utf-8";
    }
});

app.UseAuthorization();
app.UseMarkdown();
app.UseSession();

// Explicit root redirect so / always goes to the Home page
app.MapGet("/", () => Results.Redirect("/Home/Index", permanent: false))
   .ExcludeFromDescription();

// Add Scalar API documentation
app.UseCustomScalar();

// Map all controllers — attribute-routed API controllers and convention-routed MVC controllers
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHealthChecks("/health");

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("AsyncSpark.Web starting — site will start in degraded mode if any optional services are unavailable");

try
{
    app.Run();
}
catch (OperationCanceledException)
{
    // Normal shutdown via Ctrl+C or SIGTERM — not an error
    logger.LogInformation("AsyncSpark.Web shut down via cancellation");
}
catch (Exception ex)
{
    // Only truly fatal host failures reach here (e.g. port conflict, missing cert).
    // Startup service failures are caught inside StartupHostedService and do not propagate here.
    logger.LogCritical(ex, "AsyncSpark.Web terminated unexpectedly — this is a host-level failure, not a service failure");
    throw;
}
finally
{
    logger.LogInformation("AsyncSpark.Web shut down complete");
}
