using Application.AI;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/assistant")]
public class AiAssistantController : ControllerBase
{
    private readonly IAiAssistantService _assistantService;

    public AiAssistantController(
        IAiAssistantService assistantService)
    {
        _assistantService = assistantService;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        AiChatRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _assistantService.ChatAsync(
                request,
                cancellationToken);

        return Ok(response);
    }
}