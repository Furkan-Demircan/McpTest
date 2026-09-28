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

        builder.Services.AddAppManifest();

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