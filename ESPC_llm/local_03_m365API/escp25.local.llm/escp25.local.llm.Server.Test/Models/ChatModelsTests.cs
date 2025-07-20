using FluentAssertions;
using escp25.local.llm.Server.Models;

namespace escp25.local.llm.Server.Test.Models;

public class ChatModelsTests
{
    [Fact]
    public void ChatSession_DefaultValues_AreSetCorrectly()
    {
        // Act
        var session = new ChatSession();

        // Assert
        session.Id.Should().NotBeEmpty();
        session.UserId.Should().BeEmpty();
        session.Title.Should().BeEmpty();
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        session.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        session.Messages.Should().NotBeNull();
        session.Messages.Should().BeEmpty();
    }

    [Fact]
    public void ChatSession_CanSetProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = "test-user-id";
        var title = "Test Session";
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var session = new ChatSession
        {
            Id = id,
            UserId = userId,
            Title = title,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        session.Id.Should().Be(id);
        session.UserId.Should().Be(userId);
        session.Title.Should().Be(title);
        session.CreatedAt.Should().Be(createdAt);
        session.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ChatMessage_DefaultValues_AreSetCorrectly()
    {
        // Act
        var message = new ChatMessage();

        // Assert
        message.Id.Should().NotBeEmpty();
        message.ChatSessionId.Should().BeEmpty();
        message.Role.Should().BeEmpty();
        message.Content.Should().BeEmpty();
        message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        message.Attachments.Should().NotBeNull();
        message.Attachments.Should().BeEmpty();
    }

    [Fact]
    public void ChatMessage_CanSetProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var role = "user";
        var content = "Hello, world!";
        var createdAt = DateTime.UtcNow;

        // Act
        var message = new ChatMessage
        {
            Id = id,
            ChatSessionId = sessionId,
            Role = role,
            Content = content,
            CreatedAt = createdAt
        };

        // Assert
        message.Id.Should().Be(id);
        message.ChatSessionId.Should().Be(sessionId);
        message.Role.Should().Be(role);
        message.Content.Should().Be(content);
        message.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ChatAttachment_DefaultValues_AreSetCorrectly()
    {
        // Act
        var attachment = new ChatAttachment();

        // Assert
        attachment.Id.Should().NotBeEmpty();
        attachment.ChatMessageId.Should().BeEmpty();
        attachment.FileName.Should().BeEmpty();
        attachment.ContentType.Should().BeEmpty();
        attachment.FilePath.Should().BeEmpty();
        attachment.FileSize.Should().Be(0);
        attachment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ChatAttachment_CanSetProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var fileName = "test.txt";
        var contentType = "text/plain";
        var filePath = "/uploads/test.txt";
        var fileSize = 1024L;
        var createdAt = DateTime.UtcNow;

        // Act
        var attachment = new ChatAttachment
        {
            Id = id,
            ChatMessageId = messageId,
            FileName = fileName,
            ContentType = contentType,
            FilePath = filePath,
            FileSize = fileSize,
            CreatedAt = createdAt
        };

        // Assert
        attachment.Id.Should().Be(id);
        attachment.ChatMessageId.Should().Be(messageId);
        attachment.FileName.Should().Be(fileName);
        attachment.ContentType.Should().Be(contentType);
        attachment.FilePath.Should().Be(filePath);
        attachment.FileSize.Should().Be(fileSize);
        attachment.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ChatSession_WithMessages_HasCorrectRelationship()
    {
        // Arrange
        var session = new ChatSession
        {
            UserId = "test-user",
            Title = "Test Session"
        };

        var message = new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = "user",
            Content = "Hello"
        };

        // Act
        session.Messages.Add(message);

        // Assert
        session.Messages.Should().HaveCount(1);
        session.Messages[0].Should().Be(message);
        session.Messages[0].ChatSessionId.Should().Be(session.Id);
    }

    [Fact]
    public void ChatMessage_WithAttachments_HasCorrectRelationship()
    {
        // Arrange
        var message = new ChatMessage
        {
            Role = "user",
            Content = "Hello with attachment"
        };

        var attachment = new ChatAttachment
        {
            ChatMessageId = message.Id,
            FileName = "test.txt",
            ContentType = "text/plain",
            FilePath = "/uploads/test.txt",
            FileSize = 1024
        };

        // Act
        message.Attachments.Add(attachment);

        // Assert
        message.Attachments.Should().HaveCount(1);
        message.Attachments[0].Should().Be(attachment);
        message.Attachments[0].ChatMessageId.Should().Be(message.Id);
    }
}
