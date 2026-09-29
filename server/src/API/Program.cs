using API.Catalog;
using API.Middlewares;
using Application;
using Application.AI;
using Application.MCP;
using Application.MCP.Tools;
using Infrastructure;
using Infrastructure.MCP;
using Infrastructure.Persistence;
using MCP.Server.Knowledge;
using MCP.Server.Catalog;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

AddDotEnvConfiguration(builder);

// Katman Bağımlılıkları (Clean Architecture DI)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
// MCP sunucusunu API ile aynı işlemde yayınla.
// Asistan kataloğu: sayfa kataloğu (client/src/app/aiPages.ts) + Swagger şemaları (API/Forms)
builder.Services.AddAppCatalogWithSwagger();
builder.Services.AddAppKnowledge();
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(FormTools).Assembly);


// MCP Client Yapılandırması
var mcpServerUrl = builder.Configuration["Mcp:ServerUrl"]
    ?? throw new InvalidOperationException("MCP Server URL yapılandırılmamış.");

builder.Services.AddSingleton<McpClient>(sp =>
{
    var transport = new HttpClientTransport(
        new HttpClientTransportOptions
        {
            Endpoint = new Uri(mcpServerUrl),
            TransportMode = HttpTransportMode.StreamableHttp
        });

    return McpClient
        .CreateAsync(transport)
        .GetAwaiter()
        .GetResult();
});
builder.Services.AddSingleton<IToolContextRegistry, ToolContextRegistry>();
builder.Services.AddSingleton<IToolContextResolver, ToolContextResolver>();
builder.Services.AddScoped<IMcpClientService, McpClientService>();
builder.Services.AddScoped<McpClientService>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc("v1", new()
    {
        Title = "User Management Clean Architecture API",
        Version = "v1",
        Description = "Clean Architecture prensipleriyle geliştirilmiş kullanıcı yönetim servisi"
    });
});

// CORS Yapılandırması (Frontend ile tam uyum)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Mcp-Session-Id");
    });
});

var app = builder.Build();

// Veritabanını otomatik olarak oluşturma (Docker veya ilk başlangıçta otomatik ayağa kalkması için)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Veritabanı bağlantısı kontrol ediliyor ve şema hazırlanıyor...");
        DatabaseInitializer.EnsureSchema(context, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Veritabanı oluşturulurken veya bağlanırken bir hata oluştu.");
    }
}

// Global Exception Handler
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
    c.RoutePrefix = string.Empty; // Kök dizinde Swagger açılsın
});

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();
app.MapMcp("/mcp");

// Uygulama kataloğunu ve bilgi tabanını açılışta yükle: doğrulama hataları hemen loglansın.
app.Services.GetRequiredService<AppCatalogStore>();
app.Services.GetRequiredService<IKnowledgeRetriever>();

app.Run();

static void AddDotEnvConfiguration(WebApplicationBuilder builder)
{
    var directory = new DirectoryInfo(builder.Environment.ContentRootPath);

    while (directory is not null)
    {
        var dotEnvPath = Path.Combine(directory.FullName, ".env");
        if (File.Exists(dotEnvPath))
        {
            var values = new Dictionary<string, string?>();

            foreach (var rawLine in File.ReadLines(dotEnvPath))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith('#'))
                {
                    continue;
                }

                if (line.StartsWith("export ", StringComparison.Ordinal))
                {
                    line = line[7..].TrimStart();
                }

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim();

                if (value.Length >= 2 &&
                    ((value[0] == '"' && value[^1] == '"') ||
                     (value[0] == '\'' && value[^1] == '\'')))
                {
                    value = value[1..^1];
                }

                // İşletim sistemi ortam değişkenleri .env değerlerinden önceliklidir.
                if (Environment.GetEnvironmentVariable(key) is null)
                {
                    values[key] = value;
                }
            }

            builder.Configuration.AddInMemoryCollection(values);
            return;
        }

        directory = directory.Parent;
    }
}