using Application.MCP;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/mcp")]
public class McpController : ControllerBase
{
    private readonly IMcpClientService _mcpClientService;

    public McpController(IMcpClientService mcpClientService)
    {
        _mcpClientService = mcpClientService;
    }

    /// <summary>
    /// MCP Server üzerindeki tüm kullanılabilir araçları listeler.
    /// GET /api/mcp/tools
    /// </summary>
    [HttpGet("tools")]
    public async Task<IActionResult> GetTools(
        [FromQuery] bool includeParameters = false,
        CancellationToken cancellationToken = default)
    {
        var tools = await _mcpClientService.GetToolDefinitionsAsync(cancellationToken);

        if (includeParameters)
        {
            return Ok(tools);
        }

        var result = tools.Select(tool => new
        {
            name = tool.Name,
            description = tool.Description
        });

        return Ok(result);
    }
}
