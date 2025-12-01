using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.ChatCompletion;
using espc25.local.llm.Server.Models;
using espc25.local.llm.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace espc25.local.llm.Server.Services;

public interface IChatService
{
    Task<ChatSession> CreateChatSessionAsync(string userId, string title);
    Task<IEnumerable<ChatSession>> GetChatSessionsAsync(string userId);
    Task<ChatSession?> GetChatSessionAsync(Guid sessionId, string userId);
    Task<ChatSession?> UpdateChatSessionAsync(Guid sessionId, string userId, string title);
    Task<bool> DeleteChatSessionAsync(Guid sessionId, string userId);
    Task<ChatMessage> SaveUserMessageAsync(Guid sessionId, string content, List<ChatAttachment> attachments);
    IAsyncEnumerable<string> StreamChatResponseAsync(Guid sessionId, string userMessage, CancellationToken cancellationToken, string? accessToken = null);
    Task<ChatMessage> SaveAssistantMessageAsync(Guid sessionId, string content);
    Task<string> ProcessFileAsync(IFormFile file, string uploadsPath);
}

public class ChatService : IChatService
{
    private readonly ChatDbContext _dbContext;
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ISemanticKernelToolService _toolService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        ChatDbContext dbContext, 
        Kernel kernel, 
        ISemanticKernelToolService toolService,
        ILogger<ChatService> logger)
    {
        _dbContext = dbContext;
        _kernel = kernel;
        _toolService = toolService;
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        _logger = logger;
    }

    public async Task<ChatSession> CreateChatSessionAsync(string userId, string title)
    {
        var session = new ChatSession
        {
            UserId = userId,
            Title = title
        };

        _dbContext.ChatSessions.Add(session);
        await _dbContext.SaveChangesAsync();

        return session;
    }

    public async Task<IEnumerable<ChatSession>> GetChatSessionsAsync(string userId)
    {
        return await _dbContext.ChatSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .Include(s => s.Messages)
            .ThenInclude(m => m.Attachments)
            .ToListAsync();
    }

    public async Task<ChatSession?> GetChatSessionAsync(Guid sessionId, string userId)
    {
        return await _dbContext.ChatSessions
            .Include(s => s.Messages)
            .ThenInclude(m => m.Attachments)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
    }

    public async Task<ChatSession?> UpdateChatSessionAsync(Guid sessionId, string userId, string title)
    {
        var session = await _dbContext.ChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null)
        {
            return null;
        }

        session.Title = title;
        session.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return session;
    }

    public async Task<bool> DeleteChatSessionAsync(Guid sessionId, string userId)
    {
        var session = await _dbContext.ChatSessions
            .Include(s => s.Messages)
            .ThenInclude(m => m.Attachments)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null)
        {
            return false;
        }

        // Remove all attachments first
        foreach (var message in session.Messages)
        {
            if (message.Attachments != null && message.Attachments.Any())
            {
                _dbContext.ChatAttachments.RemoveRange(message.Attachments);
            }
        }

        // Remove all messages
        if (session.Messages != null && session.Messages.Any())
        {
            _dbContext.ChatMessages.RemoveRange(session.Messages);
        }

        // Remove the session
        _dbContext.ChatSessions.Remove(session);
        
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<ChatMessage> SaveUserMessageAsync(Guid sessionId, string content, List<ChatAttachment> attachments)
    {
        var message = new ChatMessage
        {
            ChatSessionId = sessionId,
            Role = "user",
            Content = content,
            Attachments = attachments
        };

        _dbContext.ChatMessages.Add(message);
        
        // Update session timestamp
        var session = await _dbContext.ChatSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return message;
    }

    public async Task<ChatMessage> SaveAssistantMessageAsync(Guid sessionId, string content)
    {
        var message = new ChatMessage
        {
            ChatSessionId = sessionId,
            Role = "assistant",
            Content = content
        };

        _dbContext.ChatMessages.Add(message);
        
        // Update session timestamp
        var session = await _dbContext.ChatSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return message;
    }

    public async IAsyncEnumerable<string> StreamChatResponseAsync(Guid sessionId, string userMessage, [EnumeratorCancellation] CancellationToken cancellationToken, string? accessToken = null)
    {
        // Get chat history
        var session = await _dbContext.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session == null)
        {
            yield return "Error: Chat session not found";
            yield break;
        }

        // Configure tools if access token is provided
        Kernel workingKernel = _kernel;
        if (!string.IsNullOrEmpty(accessToken))
        {
            try
            {
                // Clone the kernel to avoid modifying the original
                workingKernel = _kernel.Clone();
                await _toolService.InitializeToolsAsync(workingKernel, accessToken);
                _logger.LogInformation("Tools configured for authenticated session");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to configure tools with access token, proceeding without authenticated tools");
                workingKernel = _kernel;
            }
        }

        // Build chat history
        var chatHistory = new ChatHistory();
        
        // Add system message with tool information
        var systemMessage = @"You are a helpful AI assistant. You can help with various tasks including:
- Analyzing uploaded files and images
- Accessing Microsoft Learn documentation and tutorials (via MCP server)
- Making authenticated API calls to Microsoft Graph when user is signed in (via OpenAPI tool)
- Getting weather forecasts and conditions for any location worldwide (via Open-Meteo API)

Available weather capabilities:
- Current weather conditions
- 7-day weather forecasts
- Hourly weather data
- Historical weather data
- Marine weather for coastal areas
- Weather for cities worldwide (just ask for weather in 'City Name')

When using tools, explain what you're doing and provide helpful context about the information you find.";
        
        chatHistory.AddSystemMessage(systemMessage);

        // Add previous messages
        foreach (var msg in session.Messages.OrderBy(m => m.CreatedAt))
        {
            if (msg.Role == "user")
            {
                chatHistory.AddUserMessage(msg.Content);
            }
            else if (msg.Role == "assistant")
            {
                chatHistory.AddAssistantMessage(msg.Content);
            }
        }

        // Add current user message
        chatHistory.AddUserMessage(userMessage);

        // Stream the response with tool support
        var responseBuilder = new System.Text.StringBuilder();
        var hasError = false;
        var errorMessage = string.Empty;

        IAsyncEnumerable<StreamingChatMessageContent>? streamingResponse = null;
        
        try
        {
            // Get the chat completion service from the working kernel (which may have tools configured)
            var chatService = workingKernel.GetRequiredService<IChatCompletionService>();
            
            // Configure execution settings to enable tool calling
            var executionSettings = new PromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            streamingResponse = chatService.GetStreamingChatMessageContentsAsync(
                chatHistory,
                executionSettings,
                workingKernel,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting chat stream for session {SessionId}", sessionId);
            hasError = true;
            errorMessage = ex.Message;
        }

        if (hasError)
        {
            yield return $"Error: {errorMessage}";
            yield break;
        }

        if (streamingResponse != null)
        {
            await foreach (var chunk in streamingResponse.WithCancellation(cancellationToken))
            {
                if (chunk.Content != null)
                {
                    responseBuilder.Append(chunk.Content);
                    yield return chunk.Content;
                }
            }

            // Save the complete response
            var completeResponse = responseBuilder.ToString();
            if (!string.IsNullOrEmpty(completeResponse))
            {
                try
                {
                    await SaveAssistantMessageAsync(sessionId, completeResponse);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving assistant message for session {SessionId}", sessionId);
                }
            }
        }
    }

    public async Task<string> ProcessFileAsync(IFormFile file, string uploadsPath)
    {
        try
        {
            // Create uploads directory if it doesn't exist
            Directory.CreateDirectory(uploadsPath);

            // Generate unique file name
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FileName}", file.FileName);
            throw;
        }
    }
}
