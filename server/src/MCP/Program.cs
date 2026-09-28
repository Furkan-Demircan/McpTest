using MCP.Server.Knowledge;
using MCP.Server.Manifest;
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

        // Ayrı çalışan MCP host'unda controller olmadığı için manifest boştur;
        // gerçek manifest'i API host'u controller'lardan türetir.
        builder.Services.AddAppManifest(_ => new AppManifest());
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