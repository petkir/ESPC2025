using ESPC25.MCP.KnowledgeServer.Models;
using Microsoft.Extensions.Logging;

namespace ESPC25.MCP.KnowledgeServer.Services;

public class KnowledgeService : IKnowledgeService
{
    private readonly ILogger<KnowledgeService> _logger;
    private readonly List<KnowledgeDocument> _documents;

    public KnowledgeService(ILogger<KnowledgeService> logger)
    {
        _logger = logger;
        _documents = new List<KnowledgeDocument>();
        
        // Initialize with some sample documents
        InitializeSampleDocuments();
    }

    public async Task<IEnumerable<SearchResult>> SearchAsync(string query, int limit = 10)
    {
        _logger.LogInformation("Searching knowledge base for: {Query}", query);

        var results = _documents
            .Where(doc => 
                doc.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                doc.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Select(doc => new SearchResult
            {
                Content = doc.Content.Length > 500 ? doc.Content[..500] + "..." : doc.Content,
                Source = doc.Source,
                Score = CalculateRelevanceScore(doc, query),
                Metadata = new Dictionary<string, object>
                {
                    ["id"] = doc.Id,
                    ["title"] = doc.Title,
                    ["createdAt"] = doc.CreatedAt
                }
            })
            .OrderByDescending(r => r.Score)
            .Take(limit);

        return await Task.FromResult(results);
    }

    public async Task<IEnumerable<KnowledgeDocument>> GetDocumentsAsync()
    {
        _logger.LogInformation("Retrieving all documents from knowledge base");
        return await Task.FromResult(_documents);
    }

    public async Task<KnowledgeDocument?> GetDocumentByIdAsync(string id)
    {
        _logger.LogInformation("Retrieving document with ID: {Id}", id);
        var document = _documents.FirstOrDefault(d => d.Id == id);
        return await Task.FromResult(document);
    }

    private void InitializeSampleDocuments()
    {
        _documents.AddRange(new[]
        {
            new KnowledgeDocument
            {
                Id = "doc-1",
                Title = "ESPC 2025 Conference Overview",
                Content = "The European SharePoint, Office 365 & Azure Conference (ESPC) 2025 is a premier event for Microsoft technology professionals. This year's focus includes AI integration, local LLM deployment, and Microsoft 365 extensibility. Sessions cover practical implementations of semantic kernel, vector databases, and MCP servers for enterprise scenarios.",
                Source = "Conference Documentation",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Metadata = new Dictionary<string, object>
                {
                    ["category"] = "Conference",
                    ["type"] = "Overview"
                }
            },
            new KnowledgeDocument
            {
                Id = "doc-2",
                Title = "Local LLM Integration Guide",
                Content = "This guide demonstrates how to integrate local Large Language Models using Ollama with .NET applications. Key topics include: Setting up Ollama locally, Configuring Semantic Kernel for local models, Implementing chat interfaces, Vector database integration with Qdrant, and Authentication with Microsoft 365 services.",
                Source = "Technical Documentation",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                Metadata = new Dictionary<string, object>
                {
                    ["category"] = "Technical",
                    ["type"] = "Guide",
                    ["technologies"] = new[] { "Ollama", "Semantic Kernel", "Qdrant" }
                }
            },
            new KnowledgeDocument
            {
                Id = "doc-3",
                Title = "Microsoft 365 API Integration",
                Content = "Learn how to integrate Microsoft 365 APIs including Graph API for calendar access, SharePoint content retrieval, and user authentication using Entra ID. This includes examples of accessing user calendars, document libraries, and implementing proper OAuth flows for secure access to organizational data.",
                Source = "API Documentation",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                Metadata = new Dictionary<string, object>
                {
                    ["category"] = "API",
                    ["type"] = "Integration",
                    ["services"] = new[] { "Graph API", "SharePoint", "Entra ID" }
                }
            },
            new KnowledgeDocument
            {
                Id = "doc-4",
                Title = "Vector Database and Semantic Search",
                Content = "Implementing semantic search capabilities using vector databases. This covers document chunking strategies, embedding generation, similarity search algorithms, and maintaining context across conversations. Includes practical examples with Qdrant vector database and integration patterns for enterprise knowledge management.",
                Source = "Technical Guide",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Metadata = new Dictionary<string, object>
                {
                    ["category"] = "Database",
                    ["type"] = "Implementation",
                    ["concepts"] = new[] { "Vector Search", "Embeddings", "Semantic Search" }
                }
            },
            new KnowledgeDocument
            {
                Id = "doc-5",
                Title = "MCP Server Development",
                Content = "Model Context Protocol (MCP) servers enable AI assistants to access external data sources and tools. This document covers building MCP servers in .NET, implementing the MCP specification, creating custom tools for knowledge retrieval, and integrating with various AI clients including Claude Desktop and VS Code.",
                Source = "Development Guide",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Metadata = new Dictionary<string, object>
                {
                    ["category"] = "Development",
                    ["type"] = "Implementation",
                    ["protocols"] = new[] { "MCP", "JSON-RPC" }
                }
            }
        });

        _logger.LogInformation("Initialized knowledge base with {Count} documents", _documents.Count);
    }

    private static double CalculateRelevanceScore(KnowledgeDocument document, string query)
    {
        var queryLower = query.ToLowerInvariant();
        var titleLower = document.Title.ToLowerInvariant();
        var contentLower = document.Content.ToLowerInvariant();

        double score = 0;

        // Title matches get higher score
        if (titleLower.Contains(queryLower))
            score += 10;

        // Count word matches in content
        var queryWords = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in queryWords)
        {
            if (titleLower.Contains(word))
                score += 3;
            if (contentLower.Contains(word))
                score += 1;
        }

        // Boost recent documents
        var daysSinceCreated = (DateTime.UtcNow - document.CreatedAt).Days;
        if (daysSinceCreated < 7)
            score += 2;
        else if (daysSinceCreated < 30)
            score += 1;

        return score;
    }
}
