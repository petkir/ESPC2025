#!/bin/bash

# ESPC25 MCP Knowledge Server Setup Script
echo "🚀 Setting up ESPC25 MCP Knowledge Server..."

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET is not installed. Please install .NET 9.0 or later."
    echo "   Visit: https://dotnet.microsoft.com/download"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "✅ Found .NET version: $DOTNET_VERSION"

# Restore packages
echo "📦 Restoring NuGet packages..."
dotnet restore

# Build the project
echo "🔨 Building the project..."
dotnet build

# Check if Qdrant is running (optional)
if command -v curl &> /dev/null; then
    if curl -s http://localhost:6333/collections > /dev/null 2>&1; then
        echo "✅ Qdrant is running on localhost:6333"
    else
        echo "⚠️  Qdrant is not running. Vector search will use fallback implementation."
        echo "   To start Qdrant: docker run -p 6333:6333 qdrant/qdrant"
    fi
fi

# Check if Ollama is running (optional)
if command -v curl &> /dev/null; then
    if curl -s http://localhost:11434/api/tags > /dev/null 2>&1; then
        echo "✅ Ollama is running on localhost:11434"
    else
        echo "⚠️  Ollama is not running. Local LLM features will not be available."
        echo "   To start Ollama: ollama serve"
    fi
fi

echo ""
echo "🎉 Setup complete!"
echo ""
echo "To run the MCP server:"
echo "   dotnet run"
echo ""
echo "To configure for Claude Desktop, add this to your claude_desktop_config.json:"
echo "{"
echo "  \"mcpServers\": {"
echo "    \"espc25-knowledge\": {"
echo "      \"command\": \"dotnet\","
echo "      \"args\": [\"run\", \"--project\", \"$(pwd)\"]"
echo "    }"
echo "  }"
echo "}"
