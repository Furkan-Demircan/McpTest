using MCP.Server.Knowledge;
using MCP.Server.Catalog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MCP;

public static class McpServerHost
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);

        var app = builder.Build();
        ConfigureApplication(app);
        app.Run();
    }

    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("McpBrowserClient", policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Mcp-Session-Id");
            });
        });

        // Ayrı çalışan MCP host'unda Swagger olmadığı için katalog boştur;
        // gerçek kataloğu API host'u sayfa kataloğu + Swagger'dan üretir.
        builder.Services.AddAppCatalog(_ => new AppCatalog());
        builder.Services.AddAppKnowledge();

        builder.Services
            .AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly();
    }

    public static void ConfigureApplication(WebApplication app)
    {
        app.UseCors("McpBrowserClient");
        app.MapMcp("/mcp");
    }
}