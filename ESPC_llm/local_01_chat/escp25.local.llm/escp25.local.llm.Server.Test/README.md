# escp25.local.llm.Server.Test

This is a comprehensive test project for the escp25.local.llm.Server API endpoints. It includes unit tests for all controllers, services, models, DTOs, and data access layer components.

## Test Structure

### Controllers
- **ChatControllerTests**: Tests all API endpoints in the Chat controller including sessions management, message sending, and error handling
- **WeatherForecastControllerTests**: Tests the sample WeatherForecast controller

### Services  
- **ChatServiceTests**: Unit tests for the ChatService with mocked dependencies (database and LLM services)

### Models
- **ChatModelsTests**: Tests for all domain models (ChatSession, ChatMessage, ChatAttachment)

### DTOs
- **ChatDTOsTests**: Tests for all Data Transfer Objects used in API communication

### Data
- **ChatDbContextTests**: Tests for Entity Framework DbContext including relationships and cascade deletes

### Infrastructure
- **CustomWebApplicationFactory**: Test factory that sets up in-memory database and mocked services for integration testing

## Key Features

### Mocked Dependencies
- **Database**: Uses Entity Framework In-Memory database for testing
- **LLM Services**: All Semantic Kernel and chat completion services are mocked
- **Authentication**: Uses fake authentication handlers for testing authorized endpoints
- **File Operations**: File upload operations are mocked

### Test Coverage
- ✅ All API endpoints
- ✅ Success scenarios  
- ✅ Error handling
- ✅ Authentication and authorization
- ✅ Database operations and relationships
- ✅ Service layer business logic
- ✅ Model validation and properties
- ✅ DTO serialization/deserialization

### Testing Tools Used
- **xUnit**: Test framework
- **FluentAssertions**: Fluent assertion library for readable tests
- **Moq**: Mocking framework for dependencies
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing framework
- **Entity Framework In-Memory**: In-memory database for testing

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ChatControllerTests"

# Run tests in verbose mode
dotnet test --verbosity normal
```

## Test Configuration

The test project uses:
- In-memory SQLite database for isolated tests
- Fake authentication that bypasses Microsoft Identity Web
- Mocked chat completion services
- Custom web application factory for integration tests

## Adding New Tests

When adding new API endpoints or services:

1. Add controller tests in `Controllers/` folder
2. Add service tests in `Services/` folder  
3. Add model tests in `Models/` folder
4. Update the `CustomWebApplicationFactory` if new dependencies need mocking

## Best Practices

- Each test class inherits from `IClassFixture<CustomWebApplicationFactory<Program>>` for integration tests
- Service tests use fresh in-memory database instances
- All external dependencies (LLM, file system) are mocked
- Tests are isolated and can run in any order
- Descriptive test names following Given-When-Then pattern
