# Semantic Kernel Tools Integration

This document describes the integration of two powerful tools with the Semantic Kernel in the ESCP25 Local LLM Chat API:

## Tools Overview

### 1. Microsoft Graph API Tool (OpenAPI with OBO Authentication)
This tool enables the AI assistant to make authenticated calls to Microsoft Graph API using the user's access token through On-Behalf-Of (OBO) authentication flow.

**Capabilities:**
- Get user profile information
- Retrieve user's groups and memberships  
- Access recent emails
- Get upcoming calendar events
- Search files in OneDrive

### 2. Microsoft Learn Documentation Tool (MCP Server)
This tool provides access to Microsoft Learn documentation, tutorials, and code samples.

**Capabilities:**
- Search Microsoft Learn documentation
- Get learning paths for specific topics
- Retrieve code samples for technologies
- Get Azure service information and documentation

## Configuration

### appsettings.json
```json
{
  "Tools": {
    "OpenApi": {
      "ApiUrl": "https://graph.microsoft.com/v1.0",
      "SpecUrl": "https://raw.githubusercontent.com/microsoftgraph/msgraph-metadata/master/openapi/v1.0/openapi.yaml",
      "Description": "Microsoft Graph API with OBO authentication"
    },
    "MCP": {
      "ServerEndpoint": "https://mcp.learn.microsoft.com",
      "Description": "Microsoft Learn documentation and resources via MCP server"
    }
  }
}
```

## How It Works

### Authentication Flow
1. User authenticates with Entra ID and receives an access token
2. The access token is passed to the chat service via the Authorization header
3. When tools are needed, the access token is used for OBO authentication to Microsoft Graph
4. The AI can then make authenticated API calls on behalf of the user

### Tool Registration
Tools are automatically registered when the application starts:
- MCP server tool is always available (no authentication required)
- Microsoft Graph tool is only available when a valid access token is provided

### Example Usage
Users can ask questions like:
- "What meetings do I have today?" (uses Calendar API)
- "Show me my recent emails" (uses Mail API)
- "Search for PowerPoint files in my OneDrive" (uses Files API)
- "How do I create an Azure Function?" (uses Microsoft Learn tool)
- "Find learning paths for Power Platform" (uses Microsoft Learn tool)

## Security Considerations

- Access tokens are never stored; they're used only for the duration of the chat session
- All API calls use the user's permissions - the app never has elevated privileges
- Tools are only loaded when a valid access token is available
- Failed tool initialization doesn't prevent the app from functioning

## Implementation Details

### Key Files
- `Services/ISemanticKernelToolService.cs` - Interface for tool management
- `Services/SemanticKernelToolService.cs` - Main tool configuration service
- `Services/GraphApiPlugin.cs` - Microsoft Graph API plugin implementation
- `Services/MicrosoftLearnPlugin.cs` - Microsoft Learn documentation plugin
- `Controllers/ChatController.cs` - Updated to extract and pass access tokens
- `Services/ChatService.cs` - Updated to use tools during chat streaming

### Tool Architecture
- Tools are implemented as Semantic Kernel plugins
- Each tool is a class with methods decorated with `[KernelFunction]`
- Tools can be dynamically loaded based on available authentication
- Error handling ensures the app continues to function even if tools fail

## Future Enhancements

1. **Additional Microsoft Graph APIs**: Teams, SharePoint, Planner
2. **More MCP Servers**: Azure documentation, Power Platform docs
3. **Caching**: Cache frequently accessed data to improve performance
4. **Tool Configuration**: Allow users to enable/disable specific tools
5. **Advanced Authentication**: Support for different token scopes and permissions
