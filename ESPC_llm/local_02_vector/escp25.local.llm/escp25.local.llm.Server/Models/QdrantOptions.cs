namespace escp25.local.llm.Server.Models;

public class QdrantOptions
{
    public const string SectionName = "Qdrant";
    
    public string Endpoint { get; set; } = "http://localhost:6333";
    public string CollectionName { get; set; } = "knowledge_base";
    public int VectorSize { get; set; } = 384; // Default for all-MiniLM-L6-v2
}
