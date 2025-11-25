# ESPC25 MCP Knowledge Server

A Model Context Protocol (MCP) server that provides localhost knowledge access to your organizational data including:

- Local vector database (Qdrant) with document chunks
- Microsoft 365 integration (Calendar, SharePoint, etc.)
- Chat history and conversation context
- Semantic search capabilities
- Local LLM integration with Ollama

## Features

- **Knowledge Retrieval**: Search through local documents and vector embeddings
- **M365 Integration**: Access calendar events, SharePoint content, and user data
- **Chat Context**: Maintain conversation history and context
- **Semantic Search**: Use embeddings for intelligent document retrieval
- **Authentication**: Support for Entra ID authentication

## Quick Start

1. Install dependencies:
```bash
dotnet restore
```

2. Configure your settings in `appsettings.json`

3. Run the MCP server:
```bash
dotnet run
```

4. The server will be available for MCP clients to connect to.

## MCP Tools Available

- `search_knowledge`: Search through local knowledge base
- `get_calendar_events`: Retrieve M365 calendar events
- `get_chat_history`: Access previous conversations
- `semantic_search`: Perform semantic search on documents
- `get_user_context`: Retrieve user information and context

## Configuration

See `appsettings.example.json` for configuration options.
