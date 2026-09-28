using Microsoft.Extensions.DependencyInjection;

namespace MCP.Server.Manifest;

public static class ManifestServiceCollectionExtensions
{
    /// <summary>
    /// Uygulama manifest'ini kaydeder. Manifest'i üreten taraf host'tur:
    /// API host onu controller'lardan türetir (AddAppManifestFromControllers);
    /// ayrı çalışan MCP host'u manifest'siz (boş) çalışır.
    /// Fabrika ilk kullanımda bir kez çalışır.
    /// </summary>
    public static IServiceCollection AddAppManifest(
        this IServiceCollection services,
        Func<IServiceProvider, AppManifest> factory)
    {
        services.AddSingleton(sp => new AppManifestStore(factory(sp)));

        return services;
    }
}
