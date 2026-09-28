using MCP.Server.Manifest;

namespace API.Forms;

public static class AppManifestServiceCollectionExtensions
{
    /// <summary>
    /// Asistan manifest'ini sayfa kataloğu (Manifest/app-pages.json) + Swagger'dan üretir.
    /// Üretim ilk kullanımda bir kez yapılır; tutarsızlıklar loglanır.
    /// </summary>
    public static IServiceCollection AddAppManifestFromCatalog(this IServiceCollection services)
    {
        services.AddSingleton<SwaggerFormSchemaProvider>();
        services.AddSingleton<AppManifestBuilder>();
        services.AddAppManifest(sp =>
        {
            var catalogPath = Path.Combine(AppContext.BaseDirectory, "Manifest", "app-pages.json");
            var (manifest, issues) = sp.GetRequiredService<AppManifestBuilder>().Build(catalogPath);

            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("AppManifest");
            foreach (var issue in issues)
            {
                logger.LogError("Manifest doğrulaması: {Issue}", issue);
            }

            logger.LogInformation(
                "Manifest üretildi: {PageCount} sayfa, {FormCount} form (alanlar Swagger'dan)",
                manifest.Pages.Count, manifest.Forms.Count);

            return manifest;
        });

        return services;
    }
}
