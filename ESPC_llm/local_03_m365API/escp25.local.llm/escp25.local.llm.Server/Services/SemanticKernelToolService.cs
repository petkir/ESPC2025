using Microsoft.SemanticKernel;
using Microsoft.Identity.Web;
using System.Text.Json;

namespace escp25.local.llm.Server.Services;

/// <summary>
/// Service for configuring and managing Semantic Kernel tools including OpenAPI and MCP server tools
/// </summary>
public class SemanticKernelToolService : ISemanticKernelToolService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SemanticKernelToolService> _logger;
    private readonly HttpClient _httpClient;

    public SemanticKernelToolService(
        IConfiguration configuration, 
        ILogger<SemanticKernelToolService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Configures the OpenAPI tool with OBO (On-Behalf-Of) authentication
    /// This tool allows the kernel to make authenticated API calls using the user's token
    /// </summary>
    public async Task ConfigureOpenApiToolAsync(Kernel kernel, string accessToken)
    {
        try
        {
            var openApiConfig = _configuration.GetSection("Tools:OpenApi");
            var apiUrl = openApiConfig["ApiUrl"];
            var openApiSpecUrl = openApiConfig["SpecUrl"];

            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(openApiSpecUrl))
            {
                _logger.LogWarning("OpenAPI tool configuration is missing. Skipping OpenAPI tool setup.");
                return;
            }

            // Configure HTTP client with bearer token for OBO authentication
            var httpClientWithAuth = new HttpClient();
            httpClientWithAuth.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Note: This is a placeholder implementation
            // The actual implementation would depend on the specific OpenAPI plugin available
            // For now, we'll create a simple wrapper that can be extended
            var graphApiPlugin = new GraphApiPlugin(httpClientWithAuth, _logger);
            kernel.Plugins.Add(graphApiPlugin);

            _logger.LogInformation("OpenAPI tool configured successfully with OBO authentication");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure OpenAPI tool");
            throw;
        }
    }

    /// <summary>
    /// Configures the MCP server tool for learn.microsoft.com
    /// This provides access to Microsoft Learn documentation and resources
    /// </summary>
    public async Task ConfigureMcpServerToolAsync(Kernel kernel)
    {
        try
        {
            var mcpConfig = _configuration.GetSection("Tools:MCP:Servers:microsoft.docs.mcp");
            var serverUrl = mcpConfig["url"] ?? "https://learn.microsoft.com/api/mcp";

            // Create the Microsoft Learn plugin with the MCP server endpoint
            var mcpPlugin = new MicrosoftLearnPlugin(_httpClient, serverUrl, _logger);
            kernel.Plugins.Add(mcpPlugin);

            _logger.LogInformation("MCP server tool configured successfully for learn.microsoft.com at {ServerUrl}", serverUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure MCP server tool for learn.microsoft.com");
            throw;
        }
    }

    /// <summary>
    /// Configures the Open-Meteo Weather API tool
    /// This provides access to weather forecasts, historical data, and marine conditions
    /// </summary>
    public async Task ConfigureWeatherToolAsync(Kernel kernel)
    {
        try
        {
            // Create the weather plugin - no authentication required for Open-Meteo
            var weatherPlugin = new OpenMeteoWeatherPlugin(_httpClient, _logger);
            kernel.Plugins.Add(weatherPlugin);

            _logger.LogInformation("Open-Meteo Weather API tool configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure Open-Meteo Weather API tool");
            throw;
        }
    }

    /// <summary>
    /// Initializes all configured tools for the kernel
    /// </summary>
    public async Task InitializeToolsAsync(Kernel kernel, string? accessToken = null)
    {
        _logger.LogInformation("Initializing Semantic Kernel tools...");

        var tasks = new List<Task>();

        // Initialize MCP server tool (doesn't require authentication)
        if (_configuration.GetSection("Tools:MCP").Exists())
        {
            tasks.Add(ConfigureMcpServerToolAsync(kernel));
        }

        // Initialize OpenAPI tool if access token is provided
        if (!string.IsNullOrEmpty(accessToken) && _configuration.GetSection("Tools:OpenApi").Exists())
        {
            tasks.Add(ConfigureOpenApiToolAsync(kernel, accessToken));
        }

        // Initialize Weather tool (doesn't require authentication)
        tasks.Add(ConfigureWeatherToolAsync(kernel));

        // Execute all tool configurations in parallel
        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation("All Semantic Kernel tools initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize some Semantic Kernel tools");
            // Continue execution even if some tools fail to initialize
        }
    }
}
