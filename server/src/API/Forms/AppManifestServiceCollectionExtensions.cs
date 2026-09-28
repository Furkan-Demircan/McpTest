using MCP.Server.Manifest;

namespace API.Forms;

public static class AppManifestServiceCollectionExtensions
{
    /// <summary>
    /// Manifest'i [AppForm] işaretli controller action'larından ve verilen özel
    /// sayfalardan üretir. Üretim ilk kullanımda bir kez yapılır; tutarsızlıklar loglanır.
    /// </summary>
    public static IServiceCollection AddAppManifestFromControllers(
        this IServiceCollection services,
        Action<AppPageRegistry> configurePages)
    {
        var registry = new AppPageRegistry();
        configurePages(registry);

        services.AddSingleton(registry);
        services.AddSingleton<AppManifestBuilder>();
        services.AddAppManifest(sp =>
        {
            var (manifest, issues) = sp.GetRequiredService<AppManifestBuilder>().Build();

            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("AppManifest");
            foreach (var issue in issues)
            {
                logger.LogError("Manifest doğrulaması: {Issue}", issue);
            }

            logger.LogInformation(
                "Manifest üretildi: {PageCount} sayfa, {FormCount} form",
                manifest.Pages.Count, manifest.Forms.Count);

            return manifest;
        });

        return services;
    }
}
