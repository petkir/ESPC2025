using FluentAssertions;
using escp25.local.llm.Server.DTOs;

namespace escp25.local.llm.Server.Test.DTOs;

public class ChatDTOsTests
{
    [Fact]
    public void ChatSessionDto_DefaultValues_AreSetCorrectly()
    {
        // Act
        var dto = new ChatSessionDto();

        // Assert
        dto.Id.Should().BeEmpty();
        dto.Title.Should().BeEmpty();
        dto.CreatedAt.Should().Be(default);
        dto.UpdatedAt.Should().Be(default);
        dto.Messages.Should().NotBeNull();
        dto.Messages.Should().BeEmpty();
    }

    [Fact]
    public void ChatSessionDto_CanSetProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Session";
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var dto = new ChatSessionDto
        {
            Id = id,
            Title = title,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.Title.Should().Be(title);
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ChatMessageDto_DefaultValues_AreSetCorrectly()
    {
        // Act
        var dto = new ChatMessageDto();

        // Assert
        dto.Id.Should().BeEmpty();
        dto.Role.Should().BeEmpty();
        dto.Content.Should().BeEmpty();
        dto.CreatedAt.Should().Be(default);
        dto.Attachments.Should().NotBeNull();
        dto.Attachments.Should().BeEmpty();
    }

    [Fact]
    public void ChatMessageDto_CanSetProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var role = "user";
        var content = "Hello, world!";
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new ChatMessageDto
        {
            Id = id,
            Role = role,
            Content = content,
            CreatedAt = createdAt
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.Role.Should().Be(role);
        dto.Content.Should().Be(content);
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ChatAttachmentDto_DefaultValues_AreSetCorrectly()
    {
        // Act
        var dto = new ChatAttachmentDto();

        // Assert
        dto.Id.Should().BeEmpty();
        dto.FileName.Should().BeEmpty();
        dto.ContentType.Should().BeEmpty();
        dto.FileSize.Should().Be(0);
        dto.CreatedAt.Should().Be(default);
    }

    [Fact]
    public void ChatAttachmentDto_CanSetProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fileName = "test.txt";
        var contentType = "text/plain";
        var fileSize = 1024L;
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new ChatAttachmentDto
        {
            Id = id,
            FileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            CreatedAt = createdAt
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.FileName.Should().Be(fileName);
        dto.ContentType.Should().Be(contentType);
        dto.FileSize.Should().Be(fileSize);
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void CreateChatSessionRequest_DefaultValues_AreSetCorrectly()
    {
        // Act
        var request = new CreateChatSessionRequest();

        // Assert
        request.Title.Should().BeEmpty();
    }

    [Fact]
    public void CreateChatSessionRequest_CanSetTitle()
    {
        // Arrange
        var title = "New Chat Session";

        // Act
        var request = new CreateChatSessionRequest { Title = title };

        // Assert
        request.Title.Should().Be(title);
    }

    [Fact]
    public void SendMessageRequest_DefaultValues_AreSetCorrectly()
    {
        // Act
        var request = new SendMessageRequest();

        // Assert
        request.Content.Should().BeEmpty();
        request.Files.Should().NotBeNull();
        request.Files.Should().BeEmpty();
    }

    [Fact]
    public void SendMessageRequest_CanSetProperties()
    {
        // Arrange
        var content = "Hello, world!";

        // Act
        var request = new SendMessageRequest { Content = content };

        // Assert
        request.Content.Should().Be(content);
        request.Files.Should().NotBeNull();
    }

    [Fact]
    public void ChatStreamResponse_DefaultValues_AreSetCorrectly()
    {
        // Act
        var response = new ChatStreamResponse();

        // Assert
        response.Type.Should().BeEmpty();
        response.Content.Should().BeEmpty();
        response.MessageId.Should().BeNull();
        response.Error.Should().BeNull();
    }

    [Fact]
    public void ChatStreamResponse_CanSetProperties()
    {
        // Arrange
        var type = "chunk";
        var content = "Hello";
        var messageId = Guid.NewGuid();
        var error = "Error message";

        // Act
        var response = new ChatStreamResponse
        {
            Type = type,
            Content = content,
            MessageId = messageId,
            Error = error
        };

        // Assert
        response.Type.Should().Be(type);
        response.Content.Should().Be(content);
        response.MessageId.Should().Be(messageId);
        response.Error.Should().Be(error);
    }

    [Fact]
    public void ChatSessionDto_WithMessages_HasCorrectStructure()
    {
        // Arrange
        var sessionDto = new ChatSessionDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Session"
        };

        var messageDto = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            Role = "user",
            Content = "Hello"
        };

        var attachmentDto = new ChatAttachmentDto
        {
            Id = Guid.NewGuid(),
            FileName = "test.txt",
            ContentType = "text/plain",
            FileSize = 1024
        };

        // Act
        messageDto.Attachments.Add(attachmentDto);
        sessionDto.Messages.Add(messageDto);

        // Assert
        sessionDto.Messages.Should().HaveCount(1);
        sessionDto.Messages[0].Should().Be(messageDto);
        sessionDto.Messages[0].Attachments.Should().HaveCount(1);
        sessionDto.Messages[0].Attachments[0].Should().Be(attachmentDto);
    }
}
