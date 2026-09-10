using MCP.Server.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.server.Tools;

[McpServerToolType]
public static class ApplicationInfoTools
{
    [McpServerTool(UseStructuredContent = true)]
    [Description("Uygulamadaki mevcut sayfa hakkında bilgi verir.")]
    public static CurrentPageResult GetCurrentPage()
    {
        return new CurrentPageResult
        {
            Page = "/form",
            PageName = "Kişisel bilgi formu"
        };
    }
}
