using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using escp25.local.llm.Server.Data;
using escp25.local.llm.Server.Models;

namespace escp25.local.llm.Server.Test.Data;

public class ChatDbContextTests : IDisposable
{
    private readonly ChatDbContext _context;

    public ChatDbContextTests()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ChatDbContext(options);
    }

    [Fact]
    public void ChatDbContext_HasCorrectDbSets()
    {
        // Assert
        _context.ChatSessions.Should().NotBeNull();
        _context.ChatMessages.Should().NotBeNull();
        _context.ChatAttachments.Should().NotBeNull();
    }

    [Fact]
    public async Task ChatSession_CanBeAddedAndRetrieved()
    {
        // Arrange
        var session = new ChatSession
        {
            UserId = "test-user-id",
            Title = "Test Session"
        };

        // Act
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        var retrievedSession = await _context.ChatSessions.FindAsync(session.Id);

        // Assert
        retrievedSession.Should().NotBeNull();
        retrievedSession!.UserId.Should().Be("test-user-id");
        retrievedSession.Title.Should().Be("Test Session");
    }

    [Fact]
    public async Task ChatMessage_CanBeAddedAndRetrieved()
    {
        // Arrange
        var session = new ChatSession
        {
            UserId = "test-user-id",
            Title = "Test Session"
        };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        var message = new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = "user",
            Content = "Hello, world!"
        };

        // Act
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        var retrievedMessage = await _context.ChatMessages.FindAsync(message.Id);

        // Assert
        retrievedMessage.Should().NotBeNull();
        retrievedMessage!.ChatSessionId.Should().Be(session.Id);
        retrievedMessage.Role.Should().Be("user");
        retrievedMessage.Content.Should().Be("Hello, world!");
    }

    [Fact]
    public async Task ChatAttachment_CanBeAddedAndRetrieved()
    {
        // Arrange
        var session = new ChatSession
        {
            UserId = "test-user-id",
            Title = "Test Session"
        };
        _context.ChatSessions.Add(session);

        var message = new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = "user",
            Content = "Hello with attachment"
        };
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        var attachment = new ChatAttachment
        {
            ChatMessageId = message.Id,
            FileName = "test.txt",
            ContentType = "text/plain",
            FilePath = "/uploads/test.txt",
            FileSize = 1024
        };

        // Act
        _context.ChatAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        var retrievedAttachment = await _context.ChatAttachments.FindAsync(attachment.Id);

        // Assert
        retrievedAttachment.Should().NotBeNull();
        retrievedAttachment!.ChatMessageId.Should().Be(message.Id);
        retrievedAttachment.FileName.Should().Be("test.txt");
        retrievedAttachment.ContentType.Should().Be("text/plain");
        retrievedAttachment.FilePath.Should().Be("/uploads/test.txt");
        retrievedAttachment.FileSize.Should().Be(1024);
    }

    [Fact]
    public async Task ChatSession_WithMessages_HasCorrectRelationship()
    {
        // Arrange
        var session = new ChatSession
        {
            UserId = "test-user-id",
            Title = "Test Session"
        };
        _context.ChatSessions.Add(session);

        var message1 = new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = "user",
            Content = "First message"
        };

        var message2 = new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = "assistant",
            Content = "Second message"
        };

        _context.ChatMessages.AddRange(message1, message2);
        await _context.SaveChangesAsync();

        // Act
        var retrievedSession = await _context.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == session.Id);

        // Assert
        retrievedSession.Should().NotBeNull();
        retrievedSession!.Messages.Should().HaveCount(2);
        retrievedSession.Messages.Should().Contain(m => m.Content == "First message");
        retrievedSession.Messages.Should().Contain(m => m.Content == "Second message");
    }

    [Fact]
    public async Task ChatMessage_WithAttachments_HasCorrectRelationship()
    {
        // Arrange
        var session = new ChatSession
        {
            UserId = "test-user-id",
            Title = "Test Session"
        };
        _context.ChatSessions.Add(session);

        var message = new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = "user",
            Content = "Message with attachments"
        };
        _context.ChatMessages.Add(message);

        var attachment1 = new ChatAttachment
        {
            ChatMessageId = message.Id,
            FileName = "file1.txt",
            ContentType = "text/plain",
            FilePath = "/uploads/file1.txt",
            FileSize = 1024
        };

        var attachment2 = new ChatAttachment
        {
            ChatMessageId = message.Id,
            FileName = "file2.jpg",
            ContentType = "image/jpeg",
            FilePath = "/uploads/file2.jpg",
            FileSize = 2048
        };

        _context.ChatAttachments.AddRange(attachment1, attachment2);
        await _context.SaveChangesAsync();

        // Act
        var retrievedMessage = await _context.ChatMessages
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        // Assert
        retrievedMessage.Should().NotBeNull();
        retrievedMessage!.Attachments.Should().HaveCount(2);
        retrievedMessage.Attachments.Should().Contain(a => a.FileName == "file1.txt");
        retrievedMessage.Attachments.Should().Contain(a => a.FileName == "file2.jpg");
    }

    [Fact]
    public async Task DeleteSession_CascadesDeleteToMessages()
    {
        // Arrange
        var session = new ChatSession
        {
            UserId = "test-user-id",
            Title = "Test Session"
        };
        _context.ChatSessions.Add(session);

        var message = new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = "user",
            Content = "Test message"
        };
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        // Act
        _context.ChatSessions.Remove(session);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedSession = await _context.ChatSessions.FindAsync(session.Id);
        var retrievedMessage = await _context.ChatMessages.FindAsync(message.Id);

        retrievedSession.Should().BeNull();
        retrievedMessage.Should().BeNull();
    }

    [Fact]
    public async Task DeleteMessage_CascadesDeleteToAttachments()
    {
        // Arrange
        var session = new ChatSession
        {
            UserId = "test-user-id",
            Title = "Test Session"
        };
        _context.ChatSessions.Add(session);

        var message = new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = "user",
            Content = "Test message"
        };
        _context.ChatMessages.Add(message);

        var attachment = new ChatAttachment
        {
            ChatMessageId = message.Id,
            FileName = "test.txt",
            ContentType = "text/plain",
            FilePath = "/uploads/test.txt",
            FileSize = 1024
        };
        _context.ChatAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        // Act
        _context.ChatMessages.Remove(message);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedMessage = await _context.ChatMessages.FindAsync(message.Id);
        var retrievedAttachment = await _context.ChatAttachments.FindAsync(attachment.Id);

        retrievedMessage.Should().BeNull();
        retrievedAttachment.Should().BeNull();
    }

    [Fact]
    public async Task ChatSession_UserIdIndex_AllowsEfficientQueries()
    {
        // Arrange
        var userId = "test-user-id";
        var sessions = new List<ChatSession>();

        for (int i = 0; i < 5; i++)
        {
            sessions.Add(new ChatSession
            {
                UserId = userId,
                Title = $"Session {i}"
            });
        }

        _context.ChatSessions.AddRange(sessions);
        await _context.SaveChangesAsync();

        // Act
        var userSessions = await _context.ChatSessions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        // Assert
        userSessions.Should().HaveCount(5);
        userSessions.Should().OnlyContain(s => s.UserId == userId);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
