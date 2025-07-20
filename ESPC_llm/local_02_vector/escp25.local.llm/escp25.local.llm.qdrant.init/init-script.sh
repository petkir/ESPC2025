#!/bin/bash

# Qdrant Initialization Script for Mac/Linux
# This script sets up Qdrant and loads sample data

echo "🚀 Initializing Qdrant Vector Database..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi

# Check if Qdrant container is already running
if docker ps | grep -q qdrant; then
    echo "✅ Qdrant is already running"
else
    echo "🐳 Starting Qdrant container..."
    docker run -d -p 6333:6333 -p 6334:6334 --name qdrant-vector-db qdrant/qdrant
    
    # Wait for Qdrant to be ready
    echo "⏳ Waiting for Qdrant to be ready..."
    sleep 10
    
    # Check if Qdrant is responding
    max_attempts=30
    attempt=1
    while [ $attempt -le $max_attempts ]; do
        if curl -s http://localhost:6333/collections > /dev/null; then
            echo "✅ Qdrant is ready!"
            break
        fi
        
        if [ $attempt -eq $max_attempts ]; then
            echo "❌ Qdrant failed to start within timeout"
            exit 1
        fi
        
        echo "⏳ Attempt $attempt/$max_attempts - waiting for Qdrant..."
        sleep 2
        ((attempt++))
    done
fi

# Check if Ollama is running
if curl -s http://localhost:11434/api/tags > /dev/null; then
    echo "✅ Ollama is running"
else
    echo "❌ Ollama is not running. Please start Ollama first:"
    echo "   ollama serve"
    exit 1
fi

# Check if embedding model is available
if ollama list | grep -q "all-minilm"; then
    echo "✅ Embedding model (all-minilm) is available"
else
    echo "📥 Pulling embedding model..."
    ollama pull all-minilm:latest
fi

echo ""
echo "🎉 Qdrant initialization complete!"
echo ""
echo "📊 Qdrant Dashboard: http://localhost:6333/dashboard"
echo "🔍 REST API: http://localhost:6333"
echo "🚀 gRPC API: http://localhost:6334"
echo ""
echo "🔧 To load sample data, start the .NET application:"
echo "   cd ../escp25.local.llm/escp25.local.llm.Server"
echo "   dotnet run"
echo ""
echo "📝 To manually load sample data via API:"
echo "   curl -X POST http://localhost:5227/api/knowledgebase/documents \\"
echo "     -H \"Content-Type: application/json\" \\"
echo "     -H \"Authorization: Bearer <your-token>\" \\"
echo "     -d '{\"content\":\"Sample document\",\"category\":\"test\"}'"
