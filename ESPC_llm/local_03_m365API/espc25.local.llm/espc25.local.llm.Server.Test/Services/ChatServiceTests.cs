using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using FluentAssertions;
using espc25.local.llm.Server.Data;
using espc25.local.llm.Server.Models;
using espc25.local.llm.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace espc25.local.llm.Server.Test.Services;

public class ChatServiceTests : IDisposable
{
    private readonly ChatDbContext _context;
    private readonly Mock<IChatCompletionService> _mockChatCompletionService;
    private readonly Mock<ILogger<ChatService>> _mockLogger;
    private readonly ChatService _chatService;

    public ChatServiceTests()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ChatDbContext(options);
        _mockChatCompletionService = new Mock<IChatCompletionService>();
        _mockLogger = new Mock<ILogger<ChatService>>();

        // Create a simple kernel with service collection
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(_mockChatCompletionService.Object);
        var kernel = kernelBuilder.Build();

        _chatService = new ChatService(_context, kernel, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateChatSessionAsync_CreatesNewSession()
    {
        // Arrange
        var userId = "test-user-id";
        var title = "Test Session";

        // Act
        var result = await _chatService.CreateChatSessionAsync(userId, title);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Title.Should().Be(title);
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify it was saved to database
        var sessionInDb = await _context.ChatSessions.FindAsync(result.Id);
        sessionInDb.Should().NotBeNull();
        sessionInDb!.Title.Should().Be(title);
    }

    [Fact]
    public async Task GetChatSessionsAsync_ReturnsUserSessions()
    {
        // Arrange
        var userId = "test-user-id";
        var otherUserId = "other-user-id";

        var userSession1 = new ChatSession { UserId = userId, Title = "User Session 1" };
        var userSession2 = new ChatSession { UserId = userId, Title = "User Session 2" };
        var otherUserSession = new ChatSession { UserId = otherUserId, Title = "Other User Session" };

        _context.ChatSessions.AddRange(userSession1, userSession2, otherUserSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _chatService.GetChatSessionsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(s => s.UserId == userId);
        result.Select(s => s.Title).Should().Contain("User Session 1", "User Session 2");
    }

    [Fact]
    public async Task GetChatSessionAsync_WithValidIdAndUser_ReturnsSession()
    {
        // Arrange
        var userId = "test-user-id";
        var session = new ChatSession { UserId = userId, Title = "Test Session" };
        
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _chatService.GetChatSessionAsync(session.Id, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
        result.Title.Should().Be("Test Session");
    }

    [Fact]
    public async Task GetChatSessionAsync_WithValidIdButWrongUser_ReturnsNull()
    {
        // Arrange
        var userId = "test-user-id";
        var wrongUserId = "wrong-user-id";
        var session = new ChatSession { UserId = userId, Title = "Test Session" };
        
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _chatService.GetChatSessionAsync(session.Id, wrongUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetChatSessionAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var userId = "test-user-id";
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _chatService.GetChatSessionAsync(invalidId, userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveUserMessageAsync_SavesMessageWithAttachments()
    {
        // Arrange
        var userId = "test-user-id";
        var session = new ChatSession { UserId = userId, Title = "Test Session" };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        var content = "Hello, world!";
        var attachments = new List<ChatAttachment>
        {
            new ChatAttachment
            {
                FileName = "test.txt",
                ContentType = "text/plain",
                FilePath = "/uploads/test.txt",
                FileSize = 1024
            }
        };

        // Act
        var result = await _chatService.SaveUserMessageAsync(session.Id, content, attachments);

        // Assert
        result.Should().NotBeNull();
        result.ChatSessionId.Should().Be(session.Id);
        result.Role.Should().Be("user");
        result.Content.Should().Be(content);
        result.Attachments.Should().HaveCount(1);
        result.Attachments[0].FileName.Should().Be("test.txt");

        // Verify it was saved to database
        var messageInDb = await _context.ChatMessages
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == result.Id);
        messageInDb.Should().NotBeNull();
        messageInDb!.Attachments.Should().HaveCount(1);
    }

    [Fact]
    public async Task SaveAssistantMessageAsync_SavesMessage()
    {
        // Arrange
        var userId = "test-user-id";
        var session = new ChatSession { UserId = userId, Title = "Test Session" };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        var content = "Hello from assistant!";

        // Act
        var result = await _chatService.SaveAssistantMessageAsync(session.Id, content);

        // Assert
        result.Should().NotBeNull();
        result.ChatSessionId.Should().Be(session.Id);
        result.Role.Should().Be("assistant");
        result.Content.Should().Be(content);
        result.Attachments.Should().BeEmpty();

        // Verify it was saved to database
        var messageInDb = await _context.ChatMessages.FindAsync(result.Id);
        messageInDb.Should().NotBeNull();
        messageInDb!.Role.Should().Be("assistant");
    }

    [Fact]
    public async Task ProcessFileAsync_CreatesDirectoryAndSavesFile()
    {
        // Arrange
        var uploadsPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var fileName = "test.txt";
        var fileContent = "Test file content";
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(fileContent.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)));
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, CancellationToken>((stream, token) =>
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(fileContent);
                stream.Write(bytes, 0, bytes.Length);
            })
            .Returns(Task.CompletedTask);

        try
        {
            // Act
            var result = await _chatService.ProcessFileAsync(mockFile.Object, uploadsPath);

            // Assert
            result.Should().NotBeNullOrEmpty();
            Directory.Exists(uploadsPath).Should().BeTrue();
            
            var expectedFilePath = Path.Combine(uploadsPath, fileName);
            result.Should().Be(expectedFilePath);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(uploadsPath))
            {
                Directory.Delete(uploadsPath, true);
            }
        }
    }

    [Fact]
    public async Task StreamChatResponseAsync_CallsChatCompletionService()
    {
        // Arrange
        var userId = "test-user-id";
        var session = new ChatSession { UserId = userId, Title = "Test Session" };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        var userMessage = "Hello";
        var responseChunks = new[] { "Hello", " there", "!" };

        _mockChatCompletionService
            .Setup(c => c.GetStreamingChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerable(responseChunks));

        // Act
        var result = new List<string>();
        await foreach (var chunk in _chatService.StreamChatResponseAsync(session.Id, userMessage, CancellationToken.None))
        {
            result.Add(chunk);
        }

        // Assert
        result.Should().BeEquivalentTo(responseChunks);
        _mockChatCompletionService.Verify(
            c => c.GetStreamingChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static async IAsyncEnumerable<StreamingChatMessageContent> CreateAsyncEnumerable(string[] chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, chunk);
            await Task.Delay(1); // Small delay to simulate async behavior
        }
    }
}
