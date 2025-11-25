using ESPC25.MCP.KnowledgeServer.Models;

namespace ESPC25.MCP.KnowledgeServer.Services;

public interface IMcpServer
{
    Task StartAsync();
    Task<McpResponse> HandleRequestAsync(McpRequest request);
}

public interface IKnowledgeService
{
    Task<IEnumerable<SearchResult>> SearchAsync(string query, int limit = 10);
    Task<IEnumerable<KnowledgeDocument>> GetDocumentsAsync();
    Task<KnowledgeDocument?> GetDocumentByIdAsync(string id);
}

public interface IVectorService
{
    Task<IEnumerable<SearchResult>> SemanticSearchAsync(string query, int limit = 10);
    Task IndexDocumentAsync(KnowledgeDocument document);
}

public interface IM365Service
{
    Task<IEnumerable<object>> GetCalendarEventsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object?> GetUserProfileAsync();
}
