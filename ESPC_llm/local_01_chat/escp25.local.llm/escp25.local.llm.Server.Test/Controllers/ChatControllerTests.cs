using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Moq;
using escp25.local.llm.Server.Controllers;
using escp25.local.llm.Server.DTOs;
using escp25.local.llm.Server.Models;
using escp25.local.llm.Server.Services;
using escp25.local.llm.Server.Test.Infrastructure;

namespace escp25.local.llm.Server.Test.Controllers;

public class ChatControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Mock<IChatService> _mockChatService;

    public ChatControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _mockChatService = _factory.MockChatService;
    }

    [Fact]
    public async Task GetChatSessions_ReturnsOkWithSessions()
    {
        // Arrange
        var userId = "test-user-id";
        var sessions = new List<ChatSession>
        {
            new ChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Test Session 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Messages = new List<ChatMessage>()
            },
            new ChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Test Session 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Messages = new List<ChatMessage>()
            }
        };

        _mockChatService.Setup(x => x.GetChatSessionsAsync(It.IsAny<string>()))
            .ReturnsAsync(sessions);

        // Act
        var response = await _client.GetAsync("/api/chat/sessions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var sessionDtos = JsonSerializer.Deserialize<List<ChatSessionDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        sessionDtos.Should().NotBeNull();
        sessionDtos.Should().HaveCount(2);
        sessionDtos![0].Title.Should().Be("Test Session 1");
        sessionDtos[1].Title.Should().Be("Test Session 2");
    }

    [Fact]
    public async Task GetChatSessions_WhenServiceThrows_ReturnsInternalServerError()
    {
        // Arrange
        _mockChatService.Setup(x => x.GetChatSessionsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var response = await _client.GetAsync("/api/chat/sessions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreateChatSession_ReturnsOkWithCreatedSession()
    {
        // Arrange
        var request = new CreateChatSessionRequest { Title = "New Test Session" };
        var createdSession = new ChatSession
        {
            Id = Guid.NewGuid(),
            UserId = "test-user-id",
            Title = request.Title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages = new List<ChatMessage>()
        };

        _mockChatService.Setup(x => x.CreateChatSessionAsync(It.IsAny<string>(), request.Title))
            .ReturnsAsync(createdSession);

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/chat/sessions", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var sessionDto = JsonSerializer.Deserialize<ChatSessionDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        sessionDto.Should().NotBeNull();
        sessionDto!.Title.Should().Be(request.Title);
        sessionDto.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateChatSession_WhenServiceThrows_ReturnsInternalServerError()
    {
        // Arrange
        var request = new CreateChatSessionRequest { Title = "New Test Session" };
        
        _mockChatService.Setup(x => x.CreateChatSessionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/chat/sessions", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetChatSession_WithValidId_ReturnsOkWithSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = "test-user-id";
        var session = new ChatSession
        {
            Id = sessionId,
            UserId = userId,
            Title = "Test Session",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    ChatSessionId = sessionId,
                    Role = "user",
                    Content = "Hello",
                    CreatedAt = DateTime.UtcNow,
                    Attachments = new List<ChatAttachment>()
                }
            }
        };

        _mockChatService.Setup(x => x.GetChatSessionAsync(sessionId, It.IsAny<string>()))
            .ReturnsAsync(session);

        // Act
        var response = await _client.GetAsync($"/api/chat/sessions/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var sessionDto = JsonSerializer.Deserialize<ChatSessionDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        sessionDto.Should().NotBeNull();
        sessionDto!.Id.Should().Be(sessionId);
        sessionDto.Title.Should().Be("Test Session");
        sessionDto.Messages.Should().HaveCount(1);
        sessionDto.Messages[0].Content.Should().Be("Hello");
    }

    [Fact]
    public async Task GetChatSession_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        
        _mockChatService.Setup(x => x.GetChatSessionAsync(sessionId, It.IsAny<string>()))
            .ReturnsAsync((ChatSession?)null);

        // Act
        var response = await _client.GetAsync($"/api/chat/sessions/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetChatSession_WhenServiceThrows_ReturnsInternalServerError()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        
        _mockChatService.Setup(x => x.GetChatSessionAsync(sessionId, It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var response = await _client.GetAsync($"/api/chat/sessions/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetAttachment_ReturnsNotFound()
    {
        // Arrange
        var attachmentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/chat/attachments/{attachmentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendMessage_ValidatesSessionExists()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new SendMessageRequest { Content = "Hello" };
        
        _mockChatService.Setup(x => x.GetChatSessionAsync(sessionId, It.IsAny<string>()))
            .ReturnsAsync((ChatSession?)null);

        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(request.Content), "Content");

        // Act
        var response = await _client.PostAsync($"/api/chat/sessions/{sessionId}/messages", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
