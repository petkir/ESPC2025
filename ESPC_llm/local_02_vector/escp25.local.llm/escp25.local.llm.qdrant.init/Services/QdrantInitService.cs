using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using Qdrant.Client.Grpc;

#pragma warning disable SKEXP0001

namespace escp25.local.llm.qdrant.init.Services;

public interface IQdrantInitService
{
    Task InitializeAsync();
    Task<string> AddDocumentAsync(string content, string? fileName = null, string? category = null);
    Task<bool> IsQdrantHealthyAsync();
    Task<bool> CollectionExistsAsync(string collectionName);
}

public class QdrantInitService : IQdrantInitService
{
    private readonly QdrantClient _qdrantClient;
    private readonly ILogger<QdrantInitService> _logger;
    private readonly ISemanticTextMemory _memory;
    private readonly string _collectionName = "knowledge_base";

    public QdrantInitService(
        QdrantClient qdrantClient,
        ILogger<QdrantInitService> logger,
        ISemanticTextMemory memory)
    {
        _qdrantClient = qdrantClient;
        _logger = logger;
        _memory = memory;
    }

    public async Task<bool> IsQdrantHealthyAsync()
    {
        try
        {
            var health = await _qdrantClient.HealthAsync();
            _logger.LogInformation("Qdrant health check: {Status}", health);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Qdrant health check failed");
            return false;
        }
    }

    public async Task<bool> CollectionExistsAsync(string collectionName)
    {
        try
        {
            var collections = await _qdrantClient.ListCollectionsAsync();
            return collections.Any(c => c == collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if collection exists: {CollectionName}", collectionName);
            return false;
        }
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Checking Qdrant health...");
            if (!await IsQdrantHealthyAsync())
            {
                throw new InvalidOperationException("Qdrant is not healthy. Please ensure Qdrant server is running.");
            }

            _logger.LogInformation("Initializing Qdrant collection: {CollectionName}", _collectionName);
            
            // Check if collection already exists
            if (await CollectionExistsAsync(_collectionName))
            {
                _logger.LogInformation("Collection {CollectionName} already exists", _collectionName);
                return;
            }

            // Create collection with proper configuration
            await _qdrantClient.CreateCollectionAsync(_collectionName, new VectorParams
            {
                Size = 768, // nomic-embed-text embedding size
                Distance = Distance.Cosine
            });

            _logger.LogInformation("Successfully created collection: {CollectionName}", _collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Qdrant collection");
            throw;
        }
    }

    public async Task<string> AddDocumentAsync(string content, string? fileName = null, string? category = null)
    {
        try
        {
            var documentId = Guid.NewGuid().ToString();

            // Save as a single memory entry (simple path)
            var description = !string.IsNullOrWhiteSpace(fileName)
                ? fileName
                : (!string.IsNullOrWhiteSpace(category) ? category : "Document");

            var savedId = await _memory.SaveInformationAsync(
                collection: _collectionName,
                text: content,
                id: documentId,
                description: description);

            _logger.LogInformation(
                "Added document to Qdrant: {DocumentId} (File: {FileName}, Category: {Category})",
                documentId, fileName ?? "Unknown", category ?? "General");

            return savedId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add document: {FileName}", fileName);
            throw;
        }
    }
}
