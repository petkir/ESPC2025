namespace ESPC25.MCP.KnowledgeServer.Models;

public class McpServerSettings
{
    public string Name { get; set; } = "ESPC25 Knowledge Server";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "MCP server providing localhost knowledge access";
}

public class OllamaSettings
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string ModelId { get; set; } = "llama3.2";
}

public class QdrantSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6333;
    public string CollectionName { get; set; } = "documents";
}

public class McpRequest
{
    public string? Jsonrpc { get; set; }
    public string? Method { get; set; }
    public object? Params { get; set; }
    public string? Id { get; set; }
}

public class McpResponse
{
    public string Jsonrpc { get; set; } = "2.0";
    public object? Result { get; set; }
    public object? Error { get; set; }
    public string? Id { get; set; }
}

public class McpTool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object InputSchema { get; set; } = new object();
}

public class SearchResult
{
    public string Content { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class KnowledgeDocument
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
