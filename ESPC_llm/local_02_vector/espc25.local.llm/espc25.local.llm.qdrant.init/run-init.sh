#!/bin/bash

# Simple script to run the Qdrant document initialization
# Can be run independently or called from setup scripts

echo "🔧 Qdrant Document Initialization"
echo "================================="

# Check if we're in the right directory
if [ ! -f "espc25.local.llm.qdrant.init.csproj" ]; then
    echo "Error: Please run this script from the espc25.local.llm.qdrant.init directory"
    exit 1
fi

# Check if Qdrant is running
if ! curl -s http://localhost:6333/collections >/dev/null 2>&1; then
    echo "⚠️  Qdrant server is not running on localhost:6333"
    echo "Please start Qdrant first:"
    echo "  docker run -d --name qdrant-espc25 -p 6333:6333 -p 6334:6334 qdrant/qdrant"
    exit 1
fi

# Check if Ollama is running
if ! curl -s http://localhost:11434/api/version >/dev/null 2>&1; then
    echo "⚠️  Ollama server is not running on localhost:11434"
    echo "Please start Ollama first with the all-minilm model:"
    echo "  ollama serve"
    echo "  ollama pull all-minilm:latest"
    exit 1
fi

echo "✅ Prerequisites check passed"
echo ""

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

# Run the application
echo "🚀 Running document initialization..."
dotnet run

echo ""
echo "✅ Document initialization completed!"
echo "Your Qdrant vector database is now ready with sample documents."
