using Microsoft.SemanticKernel;
using Microsoft.Identity.Web;

namespace espc25.local.llm.Server.Services;

/// <summary>
/// Service interface for configuring and managing Semantic Kernel tools
/// </summary>
public interface ISemanticKernelToolService
{
    /// <summary>
    /// Configures the OpenAPI tool with OBO (On-Behalf-Of) authentication
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance</param>
    /// <param name="accessToken">The user's access token for OBO flow</param>
    /// <returns>Task</returns>
    Task ConfigureOpenApiToolAsync(Kernel kernel, string accessToken);

    /// <summary>
    /// Configures the MCP server tool for learn.microsoft.com
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance</param>
    /// <returns>Task</returns>
    Task ConfigureMcpServerToolAsync(Kernel kernel);

    /// <summary>
    /// Configures the Open-Meteo Weather API tool
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance</param>
    /// <returns>Task</returns>
    Task ConfigureWeatherToolAsync(Kernel kernel);

    /// <summary>
    /// Initializes all tools for the kernel
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance</param>
    /// <param name="accessToken">Optional access token for authenticated tools</param>
    /// <returns>Task</returns>
    Task InitializeToolsAsync(Kernel kernel, string? accessToken = null);
}
