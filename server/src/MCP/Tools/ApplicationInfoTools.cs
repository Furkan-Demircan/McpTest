using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.server.Tools;

[McpServerToolType]
public static class ApplicationInfoTools
{
    [McpServerTool]
    [Description("Uygulamadaki mevcut sayfa hakkinda bilgi verir.")]
    public static string GetCurrentPage()
    {
        return """
            "page": "/form",
            "pageName": "Kisisel bilgi formu"
        """;
    }
}
