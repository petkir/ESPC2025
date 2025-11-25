using System.Text.Json;
using ESPC25.MCP.KnowledgeServer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESPC25.MCP.KnowledgeServer.Services;

public class McpServer : IMcpServer
{
    private readonly ILogger<McpServer> _logger;
    private readonly McpServerSettings _settings;
    private readonly IKnowledgeService _knowledgeService;
    private readonly IVectorService _vectorService;
    private readonly IM365Service _m365Service;

    public McpServer(
        ILogger<McpServer> logger,
        IOptions<McpServerSettings> settings,
        IKnowledgeService knowledgeService,
        IVectorService vectorService,
        IM365Service m365Service)
    {
        _logger = logger;
        _settings = settings.Value;
        _knowledgeService = knowledgeService;
        _vectorService = vectorService;
        _m365Service = m365Service;
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("MCP Server {Name} v{Version} starting...", _settings.Name, _settings.Version);

        // Start listening for MCP requests via stdin/stdout
        await ListenForRequestsAsync();
    }

    private async Task ListenForRequestsAsync()
    {
        try
        {
            using var reader = new StreamReader(Console.OpenStandardInput());
            using var writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                try
                {
                    var request = JsonSerializer.Deserialize<McpRequest>(line);
                    if (request != null)
                    {
                        var response = await HandleRequestAsync(request);
                        var responseJson = JsonSerializer.Serialize(response);
                        await writer.WriteLineAsync(responseJson);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing request: {Line}", line);
                    
                    var errorResponse = new McpResponse
                    {
                        Error = new { code = -32603, message = "Internal error" },
                        Id = null
                    };
                    var errorJson = JsonSerializer.Serialize(errorResponse);
                    await writer.WriteLineAsync(errorJson);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MCP server main loop");
        }
    }

    public async Task<McpResponse> HandleRequestAsync(McpRequest request)
    {
        try
        {
            return request.Method switch
            {
                "initialize" => HandleInitialize(request),
                "tools/list" => HandleToolsList(request),
                "tools/call" => await HandleToolCallAsync(request),
                _ => new McpResponse
                {
                    Error = new { code = -32601, message = "Method not found" },
                    Id = request.Id
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request: {Method}", request.Method);
            return new McpResponse
            {
                Error = new { code = -32603, message = "Internal error" },
                Id = request.Id
            };
        }
    }

    private McpResponse HandleInitialize(McpRequest request)
    {
        return new McpResponse
        {
            Result = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    tools = new { }
                },
                serverInfo = new
                {
                    name = _settings.Name,
                    version = _settings.Version
                }
            },
            Id = request.Id
        };
    }

    private McpResponse HandleToolsList(McpRequest request)
    {
        var tools = new[]
        {
            new McpTool
            {
                Name = "search_knowledge",
                Description = "Search through the local knowledge base for relevant information",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        query = new { type = "string", description = "Search query" },
                        limit = new { type = "integer", description = "Maximum number of results", @default = 10 }
                    },
                    required = new[] { "query" }
                }
            },
            new McpTool
            {
                Name = "semantic_search",
                Description = "Perform semantic search using vector embeddings",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        query = new { type = "string", description = "Search query" },
                        limit = new { type = "integer", description = "Maximum number of results", @default = 10 }
                    },
                    required = new[] { "query" }
                }
            },
            new McpTool
            {
                Name = "get_calendar_events",
                Description = "Retrieve calendar events from Microsoft 365",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        startDate = new { type = "string", format = "date-time", description = "Start date for events" },
                        endDate = new { type = "string", format = "date-time", description = "End date for events" }
                    }
                }
            },
            new McpTool
            {
                Name = "get_documents",
                Description = "Get all available documents in the knowledge base",
                InputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new McpTool
            {
                Name = "get_user_profile",
                Description = "Get the current user's profile information from Microsoft 365",
                InputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            }
        };

        return new McpResponse
        {
            Result = new { tools },
            Id = request.Id
        };
    }

    private async Task<McpResponse> HandleToolCallAsync(McpRequest request)
    {
        var toolCall = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(request.Params));
        var name = toolCall.GetProperty("name").GetString();
        var arguments = toolCall.TryGetProperty("arguments", out var args) ? args : new JsonElement();

        var result = name switch
        {
            "search_knowledge" => await HandleSearchKnowledge(arguments),
            "semantic_search" => await HandleSemanticSearch(arguments),
            "get_calendar_events" => await HandleGetCalendarEvents(arguments),
            "get_documents" => await HandleGetDocuments(),
            "get_user_profile" => await HandleGetUserProfile(),
            _ => new { error = "Unknown tool" }
        };

        return new McpResponse
        {
            Result = new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })
                    }
                }
            },
            Id = request.Id
        };
    }

    private async Task<object> HandleSearchKnowledge(JsonElement arguments)
    {
        var query = arguments.TryGetProperty("query", out var q) ? q.GetString() : "";
        var limit = arguments.TryGetProperty("limit", out var l) ? l.GetInt32() : 10;

        if (string.IsNullOrEmpty(query))
            return new { error = "Query is required" };

        var results = await _knowledgeService.SearchAsync(query, limit);
        return new { results };
    }

    private async Task<object> HandleSemanticSearch(JsonElement arguments)
    {
        var query = arguments.TryGetProperty("query", out var q) ? q.GetString() : "";
        var limit = arguments.TryGetProperty("limit", out var l) ? l.GetInt32() : 10;

        if (string.IsNullOrEmpty(query))
            return new { error = "Query is required" };

        var results = await _vectorService.SemanticSearchAsync(query, limit);
        return new { results };
    }

    private async Task<object> HandleGetCalendarEvents(JsonElement arguments)
    {
        DateTime? startDate = null;
        DateTime? endDate = null;

        if (arguments.TryGetProperty("startDate", out var start) && 
            DateTime.TryParse(start.GetString(), out var startParsed))
            startDate = startParsed;

        if (arguments.TryGetProperty("endDate", out var end) && 
            DateTime.TryParse(end.GetString(), out var endParsed))
            endDate = endParsed;

        var events = await _m365Service.GetCalendarEventsAsync(startDate, endDate);
        return new { events };
    }

    private async Task<object> HandleGetDocuments()
    {
        var documents = await _knowledgeService.GetDocumentsAsync();
        return new { documents };
    }

    private async Task<object> HandleGetUserProfile()
    {
        var profile = await _m365Service.GetUserProfileAsync();
        return new { profile };
    }
}
