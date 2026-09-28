using MCP.Server.Manifest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MCP.Server.Knowledge;

public static class KnowledgeServiceCollectionExtensions
{
    /// <summary>
    /// Bilgi tabanını (Knowledge/*.md) yükler. Rehberlerdeki referanslar manifest'e
    /// karşı doğrulanır (sorunlar loglanır), ardından [@eleman] referansları
    /// indekslemeden önce manifest etiketlerine çevrilir. AddAppManifest() ile
    /// manifest'in kayıtlı olması gerekir. Dosyalar build çıktısına kopyalanır (MCP.csproj).
    /// </summary>
    public static IServiceCollection AddAppKnowledge(this IServiceCollection services)
    {
        services.AddSingleton<IKnowledgeRetriever>(sp =>
        {
            var manifest = sp.GetRequiredService<AppManifestStore>();
            var documents = MarkdownKnowledgeLoader
                .LoadFrom(Path.Combine(AppContext.BaseDirectory, "Knowledge"));

            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("AppKnowledge");
            foreach (var issue in KnowledgeValidator.Validate(documents, manifest))
            {
                logger?.LogError("Bilgi tabanı doğrulaması: {Issue}", issue);
            }

            return new KeywordKnowledgeRetriever(
                documents.Select(document => KnowledgeReferenceExpander.Expand(document, manifest)));
        });

        return services;
    }
}
