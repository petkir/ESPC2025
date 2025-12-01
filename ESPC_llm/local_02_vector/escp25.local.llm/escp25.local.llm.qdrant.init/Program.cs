using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Memory;
using Qdrant.Client;
using escp25.local.llm.qdrant.init.Services;
using escp25.local.llm.qdrant.init.Models;

#pragma warning disable SKEXP0001

namespace escp25.local.llm.qdrant.init;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Setup dependency injection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection, configuration);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Get logger
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("🚀 Starting Qdrant Document Initialization");
            logger.LogInformation("=========================================");

            // Get services
            var qdrantService = serviceProvider.GetRequiredService<IQdrantInitService>();
            var documentService = serviceProvider.GetRequiredService<IDocumentProcessingService>();

            // Check if user wants to upload documents
            Console.WriteLine();
            Console.Write("Do you want to upload sample documents to Qdrant? (y/N): ");
            var response = Console.ReadLine();
            
            if (response?.ToLowerInvariant() != "y" && response?.ToLowerInvariant() != "yes")
            {
                logger.LogInformation("Skipping document upload as requested by user");
                return;
            }

            // Initialize Qdrant collection
            logger.LogInformation("Initializing Qdrant collection...");
            await qdrantService.InitializeAsync();

            // Process sample documents from JSON
            logger.LogInformation("Loading sample documents from JSON...");
            var sampleDocuments = await documentService.LoadSampleDocumentsAsync("sample-documents.json");
            
            foreach (var doc in sampleDocuments)
            {
                await qdrantService.AddDocumentAsync(doc.Content, doc.FileName, doc.Category);
            }

            // Process PDF files from sample_files folder
            var sampleFilesPath = configuration["SampleFilesPath"] ?? Path.Combine("..", "escp25.local.llm", "sample_files");
            if (Directory.Exists(sampleFilesPath))
            {
                logger.LogInformation("Processing PDF files from sample_files folder...");
                var pdfFiles = Directory.GetFiles(sampleFilesPath, "*.pdf");
                
                foreach (var pdfFile in pdfFiles)
                {
                    try
                    {
                        var content = await documentService.ExtractTextFromPdfAsync(pdfFile);
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            var fileName = Path.GetFileName(pdfFile);
                            await qdrantService.AddDocumentAsync(content, fileName, "PDF Document");
                        }
                        else
                        {
                            logger.LogWarning("PDF file appears to be empty or unreadable: {FileName}", Path.GetFileName(pdfFile));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to process PDF file: {FileName}", Path.GetFileName(pdfFile));
                    }
                }
            }
            else
            {
                logger.LogWarning("Sample files directory not found: {Path}", sampleFilesPath);
            }

            logger.LogInformation("✅ Document initialization completed successfully!");
            Console.WriteLine();
            Console.WriteLine("🎉 All documents have been uploaded to Qdrant!");
            Console.WriteLine("You can now start the main application to use the knowledge base.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to initialize documents");
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
        });

        // Configure Qdrant client
        services.AddSingleton<QdrantClient>(serviceProvider =>
        {
            var endpoint = configuration["Qdrant:Endpoint"] ?? "http://localhost:6333";
            return new QdrantClient(endpoint);
        });
        
        // Configure Semantic Memory with Ollama embeddings and Qdrant
        #pragma warning disable SKEXP0001, SKEXP0020, SKEXP0070 // Type is for evaluation purposes only
        services.AddSingleton<ISemanticTextMemory>(serviceProvider =>
        {
            var qdrantEndpoint = configuration["Qdrant:Endpoint"] ?? "http://localhost:6333";
            var ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
            var embeddingModel = configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text:latest";

            var textEmbeddingService = new Microsoft.SemanticKernel.Connectors.Ollama.OllamaTextEmbeddingGenerationService(
                embeddingModel,
                new Uri(ollamaEndpoint));

            return new MemoryBuilder()
                .WithQdrantMemoryStore(qdrantEndpoint, 768)
                .WithTextEmbeddingGeneration(textEmbeddingService)
                .Build();
        });
            #pragma warning restore SKEXP0001, SKEXP0020, SKEXP0070

        // Add services
        services.AddScoped<IQdrantInitService, QdrantInitService>();
        services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
    }
}
