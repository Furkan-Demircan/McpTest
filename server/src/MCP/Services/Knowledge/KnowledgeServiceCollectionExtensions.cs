using MCP.Server.Catalog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MCP.Server.Knowledge;

public static class KnowledgeServiceCollectionExtensions
{
    /// <summary>
    /// Bilgi tabanını (Knowledge/*.md) yükler. Rehberlerdeki referanslar kataloğa
    /// karşı doğrulanır (sorunlar loglanır), ardından [@eleman] referansları
    /// indekslemeden önce katalog etiketlerine çevrilir. AddAppCatalog() ile
    /// kataloğun kayıtlı olması gerekir. Dosyalar build çıktısına kopyalanır (MCP.csproj).
    /// </summary>
    public static IServiceCollection AddAppKnowledge(this IServiceCollection services)
    {
        services.AddSingleton<IKnowledgeRetriever>(sp =>
        {
            var catalog = sp.GetRequiredService<AppCatalogStore>();
            var documents = MarkdownKnowledgeLoader
                .LoadFrom(Path.Combine(AppContext.BaseDirectory, "Knowledge"));

            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("AppKnowledge");
            foreach (var issue in KnowledgeValidator.Validate(documents, catalog))
            {
                logger?.LogError("Bilgi tabanı doğrulaması: {Issue}", issue);
            }

            return new KeywordKnowledgeRetriever(
                documents.Select(document => KnowledgeReferenceExpander.Expand(document, catalog)));
        });

        return services;
    }
}
