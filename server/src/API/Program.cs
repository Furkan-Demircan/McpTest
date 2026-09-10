using API.Middlewares;
using Application;
using Application.AI;
using Application.MCP;
using Infrastructure;
using Infrastructure.MCP;
using Infrastructure.Persistence;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

// Katman Bağımlılıkları (Clean Architecture DI)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


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
              .AllowAnyHeader();
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
        logger.LogInformation("Veritabanı bağlantısı kontrol ediliyor ve tablolar oluşturuluyor...");
        context.Database.EnsureCreated();
        logger.LogInformation("Veritabanı ve tablolar başarıyla hazırlandı.");
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

app.Run();
