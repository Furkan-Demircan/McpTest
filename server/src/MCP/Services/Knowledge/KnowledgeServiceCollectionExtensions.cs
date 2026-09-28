using MCP.Server.Manifest;
using Microsoft.Extensions.DependencyInjection;

namespace MCP.Server.Knowledge;

public static class KnowledgeServiceCollectionExtensions
{
    /// <summary>
    /// Bilgi tabanını (Knowledge/*.md) yükler. Rehberlerdeki [@eleman] referansları
    /// indekslemeden önce manifest etiketlerine çevrilir; böylece aramada da güncel
    /// etiketler kullanılır. AddAppManifest() ile manifest'in kayıtlı olması gerekir.
    /// Dosyalar build çıktısına kopyalanır (MCP.csproj).
    /// </summary>
    public static IServiceCollection AddAppKnowledge(this IServiceCollection services)
    {
        services.AddSingleton<IKnowledgeRetriever>(sp =>
        {
            var manifest = sp.GetRequiredService<AppManifestStore>();

            return new KeywordKnowledgeRetriever(
                MarkdownKnowledgeLoader
                    .LoadFrom(Path.Combine(AppContext.BaseDirectory, "Knowledge"))
                    .Select(document => KnowledgeReferenceExpander.Expand(document, manifest)));
        });

        return services;
    }
}
