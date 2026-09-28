using MCP.Server.Catalog;

namespace API.Catalog;

public static class AppCatalogServiceCollectionExtensions
{
    /// <summary>
    /// Asistan kataloğunu sayfa kataloğu (Catalog/app-pages.json) + Swagger'dan üretir.
    /// Üretim ilk kullanımda bir kez yapılır; tutarsızlıklar loglanır.
    /// </summary>
    public static IServiceCollection AddAppCatalogWithSwagger(this IServiceCollection services)
    {
        services.AddSingleton<SwaggerFormSchemaProvider>();
        services.AddSingleton<AppCatalogBuilder>();
        services.AddAppCatalog(sp =>
        {
            var catalogPath = Path.Combine(AppContext.BaseDirectory, "Catalog", "app-pages.json");
            var (catalog, issues) = sp.GetRequiredService<AppCatalogBuilder>().Build(catalogPath);

            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("AppCatalog");
            foreach (var issue in issues)
            {
                logger.LogError("Katalog doğrulaması: {Issue}", issue);
            }

            logger.LogInformation(
                "Uygulama kataloğu üretildi: {PageCount} sayfa, {FormCount} form (alanlar Swagger'dan)",
                catalog.Pages.Count, catalog.Forms.Count);

            return catalog;
        });

        return services;
    }
}
