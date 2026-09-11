using MCP.Server.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.server.Tools;

[McpServerToolType]
public static class ApplicationInfoTools
{
    private static string GetPageName(string? currentPage)
    {
        return currentPage switch
        {
            "/form" => "Kişisel bilgi formu",
            "/" => "Ana Sayfa",
            "/users" => "Kullanıcılar",
            _ => "Bilinmeyen Sayfa"
        };
    }



    [McpServerTool(UseStructuredContent = true)]
    [Description("Uygulamadaki mevcut sayfa hakkında bilgi verir.")]
    public static CurrentPageResult GetCurrentPage(
        string? currentPage = null
    )
    {
        return new CurrentPageResult
        {
            Page = currentPage ?? "/",
            PageName = GetPageName(currentPage)
        };
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Kullanıcıyı uygulamadaki başka bir sayfaya yönlendirmek için navigation action üretir. " +
    "Yalnızca izin verilen uygulama sayfaları kullanılabilir.")]
    public static NavigationResult NavigateToPage(string path)
    {
        var allowedPages = new Dictionary<string, string>
        {
            ["/"] = "Ana sayfa",
            ["/form"] = "Kişisel bilgi formu",
            ["/users"] = "Kullanıcılar"
        };

        if (!allowedPages.ContainsKey(path))
        {
            throw new ArgumentException(
                $"Invalid navigation path: '{path}'");
        }

        return new NavigationResult
        {
            Type = "navigation",
            Data = new NavigationData
            {
                Path = path
            }
        };
    }
}
