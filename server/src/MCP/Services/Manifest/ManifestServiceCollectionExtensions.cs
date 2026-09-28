using Microsoft.Extensions.DependencyInjection;

namespace MCP.Server.Manifest;

public static class ManifestServiceCollectionExtensions
{
    /// <summary>
    /// Uygulama manifest'ini (Manifest/app-manifest.json) yükler.
    /// Dosya `npm run manifest` ile üretilir ve build çıktısına kopyalanır (MCP.csproj).
    /// </summary>
    public static IServiceCollection AddAppManifest(this IServiceCollection services)
    {
        services.AddSingleton(_ =>
            AppManifestStore.LoadFrom(
                Path.Combine(AppContext.BaseDirectory, "Manifest", "app-manifest.json")));

        return services;
    }
}
