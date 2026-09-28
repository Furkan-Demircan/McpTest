using Microsoft.Extensions.DependencyInjection;

namespace MCP.Server.Catalog;

public static class CatalogServiceCollectionExtensions
{
    /// <summary>
    /// Uygulama kataloğunu kaydeder. Kataloğu üreten taraf host'tur:
    /// API host onu controller'lardan türetir (AddAppCatalogFromControllers);
    /// ayrı çalışan MCP host'u katalogsuz (boş) çalışır.
    /// Fabrika ilk kullanımda bir kez çalışır.
    /// </summary>
    public static IServiceCollection AddAppCatalog(
        this IServiceCollection services,
        Func<IServiceProvider, AppCatalog> factory)
    {
        services.AddSingleton(sp => new AppCatalogStore(factory(sp)));

        return services;
    }
}
