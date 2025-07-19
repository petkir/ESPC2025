using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using escp25.local.llm.Server.Data;
using escp25.local.llm.Server.Services;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;

namespace escp25.local.llm.Server.Test.Infrastructure;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public Mock<IChatService> MockChatService { get; } = new();
    public Mock<IChatCompletionService> MockChatCompletionService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the app's ChatDbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ChatDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove the app's ChatService registration
            var chatServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IChatService));
            if (chatServiceDescriptor != null)
            {
                services.Remove(chatServiceDescriptor);
            }

            // Remove the Kernel registration
            var kernelDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(Kernel));
            if (kernelDescriptor != null)
            {
                services.Remove(kernelDescriptor);
            }

            // Add InMemory database for testing
            services.AddDbContext<ChatDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Add mock services
            services.AddSingleton(MockChatService.Object);
            
            // Create a real kernel with mocked chat completion service
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.Services.AddSingleton(MockChatCompletionService.Object);
            var kernel = kernelBuilder.Build();
            services.AddSingleton(kernel);

            // Override authorization for testing
            services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
            services.AddSingleton<IAuthorizationHandler, FakeAuthorizationHandler>();

            // Replace authentication with test authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
        });

        builder.UseEnvironment("Test");
    }
}

public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "test-object-id")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class FakePolicyEvaluator : IPolicyEvaluator
{
    public virtual async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var principal = new ClaimsPrincipal();
        principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim("Permission", "CanViewPage"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "test-object-id")
        }, "Test"));

        return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, "Test")));
    }

    public virtual async Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy,
        AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        return await Task.FromResult(PolicyAuthorizationResult.Success());
    }
}

public class FakeAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.PendingRequirements.ToList())
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
