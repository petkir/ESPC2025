using System.Text.Json;
using ESPC25.MCP.KnowledgeServer.Models;
using ESPC25.MCP.KnowledgeServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ESPC25.MCP.KnowledgeServer;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<McpServerSettings>(context.Configuration.GetSection("McpServer"));
                services.Configure<OllamaSettings>(context.Configuration.GetSection("Ollama"));
                services.Configure<QdrantSettings>(context.Configuration.GetSection("Qdrant"));
                
                services.AddSingleton<IKnowledgeService, KnowledgeService>();
                services.AddSingleton<IVectorService, VectorService>();
                services.AddSingleton<IM365Service, M365Service>();
                services.AddSingleton<IMcpServer, McpServer>();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        var mcpServer = host.Services.GetRequiredService<IMcpServer>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting ESPC25 MCP Knowledge Server...");

        try
        {
            await mcpServer.StartAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting MCP server");
            throw;
        }
    }
}
