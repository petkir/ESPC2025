using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace espc25.local.llm.Server.Services;

/// <summary>
/// Plugin for accessing Microsoft Learn documentation and resources
/// </summary>
public class MicrosoftLearnPlugin
{
    private readonly HttpClient _httpClient;
    private readonly string _serverEndpoint;
    private readonly ILogger _logger;

    public MicrosoftLearnPlugin(HttpClient httpClient, string serverEndpoint, ILogger logger)
    {
        _httpClient = httpClient;
        _serverEndpoint = serverEndpoint;
        _logger = logger;
    }

    [KernelFunction("SearchDocumentation")]
    [Description("Search Microsoft Learn documentation for specific topics")]
    [return: Description("Search results from Microsoft Learn")]
    public async Task<string> SearchDocumentationAsync(
        [Description("Search query for Microsoft Learn documentation")] string query,
        [Description("Product or technology to focus search on (e.g., 'Azure', 'Microsoft 365', 'Power Platform')")] string? product = null)
    {
        try
        {
            // Call the actual MCP server endpoint
            var requestPayload = new
            {
                method = "search",
                @params = new
                {
                    query = query,
                    product = product ?? "All",
                    maxResults = 10
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_serverEndpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            else
            {
                // Fallback to simulated results if MCP server is not available
                _logger.LogWarning("MCP server returned {StatusCode}, falling back to simulated results", response.StatusCode);
                return GetFallbackSearchResults(query, product);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MCP server, falling back to simulated results");
            return GetFallbackSearchResults(query, product);
        }
    }

    private string GetFallbackSearchResults(string query, string? product)
    {
        var searchResults = new
        {
            query = query,
            product = product ?? "All",
            source = "fallback",
            results = new[]
            {
                new
                {
                    title = $"Microsoft Learn: {query}",
                    url = $"https://learn.microsoft.com/search?query={Uri.EscapeDataString(query)}",
                    summary = $"Documentation and tutorials related to {query}",
                    type = "documentation"
                },
                new
                {
                    title = $"Azure Documentation: {query}",
                    url = $"https://learn.microsoft.com/azure?query={Uri.EscapeDataString(query)}",
                    summary = $"Azure-specific documentation for {query}",
                    type = "azure-docs"
                }
            }
        };

        return JsonSerializer.Serialize(searchResults, new JsonSerializerOptions { WriteIndented = true });
    }

    [KernelFunction("GetLearningPath")]
    [Description("Get information about a specific learning path on Microsoft Learn")]
    [return: Description("Learning path information")]
    public async Task<string> GetLearningPathAsync(
        [Description("Name or topic of the learning path")] string topic)
    {
        try
        {
            // Call the MCP server for learning path information
            var requestPayload = new
            {
                method = "getLearningPath",
                @params = new
                {
                    topic = topic
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_serverEndpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            else
            {
                // Fallback to simulated results
                _logger.LogWarning("MCP server returned {StatusCode} for learning path, falling back to simulated results", response.StatusCode);
                return GetFallbackLearningPath(topic);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MCP server for learning path, falling back to simulated results");
            return GetFallbackLearningPath(topic);
        }
    }

    private string GetFallbackLearningPath(string topic)
    {
        var learningPath = new
        {
            topic = topic,
            title = $"Learning Path: {topic}",
            description = $"Comprehensive learning path for {topic}",
            source = "fallback",
            modules = new[]
            {
                $"Introduction to {topic}",
                $"Advanced {topic} concepts",
                $"Best practices for {topic}",
                $"Hands-on labs and exercises"
            },
            estimatedTime = "4-6 hours",
            url = $"https://learn.microsoft.com/training/paths/{topic.ToLower().Replace(" ", "-")}"
        };

        return JsonSerializer.Serialize(learningPath, new JsonSerializerOptions { WriteIndented = true });
    }

    [KernelFunction("GetCodeSamples")]
    [Description("Get code samples from Microsoft Learn for specific technologies")]
    [return: Description("Code samples and examples")]
    public async Task<string> GetCodeSamplesAsync(
        [Description("Technology or programming language")] string technology,
        [Description("Specific scenario or use case")] string? scenario = null)
    {
        try
        {
            // This is a placeholder implementation
            var codeSamples = new
            {
                technology = technology,
                scenario = scenario ?? "General usage",
                samples = new[]
                {
                    new
                    {
                        title = $"{technology} - Basic Example",
                        description = $"Basic implementation example using {technology}",
                        url = $"https://learn.microsoft.com/samples?technology={Uri.EscapeDataString(technology)}",
                        language = technology
                    },
                    new
                    {
                        title = $"{technology} - {scenario ?? "Advanced"} Example",
                        description = $"More complex example demonstrating {scenario ?? "advanced features"}",
                        url = $"https://github.com/Azure-Samples?q={Uri.EscapeDataString(technology)}",
                        language = technology
                    }
                }
            };

            return JsonSerializer.Serialize(codeSamples, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting code samples from Microsoft Learn");
            return $"Error getting code samples: {ex.Message}";
        }
    }

    [KernelFunction("GetAzureServiceInfo")]
    [Description("Get information about specific Azure services")]
    [return: Description("Azure service information and documentation")]
    public async Task<string> GetAzureServiceInfoAsync(
        [Description("Name of the Azure service")] string serviceName)
    {
        try
        {
            // This is a placeholder implementation
            var serviceInfo = new
            {
                serviceName = serviceName,
                description = $"Information about Azure {serviceName}",
                documentation = $"https://learn.microsoft.com/azure/{serviceName.ToLower().Replace(" ", "-")}",
                quickstart = $"https://learn.microsoft.com/azure/{serviceName.ToLower().Replace(" ", "-")}/quickstart",
                pricing = $"https://azure.microsoft.com/pricing/details/{serviceName.ToLower().Replace(" ", "-")}/",
                features = new[]
                {
                    $"Scalable {serviceName} capabilities",
                    $"Enterprise-grade security",
                    $"Global availability",
                    $"Pay-as-you-use pricing"
                }
            };

            return JsonSerializer.Serialize(serviceInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure service information");
            return $"Error getting Azure service information: {ex.Message}";
        }
    }
}
