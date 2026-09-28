namespace MCP.Server.Knowledge;

/// <summary>
/// Bilgi tabanı arama soyutlaması. Şu an anahtar kelime tabanlı;
/// ileride embedding/pgvector tabanlı bir retriever ile değiştirilebilir.
/// </summary>
public interface IKnowledgeRetriever
{
    IReadOnlyList<KnowledgeMatch> Search(string query, int maxResults = 2);
}
