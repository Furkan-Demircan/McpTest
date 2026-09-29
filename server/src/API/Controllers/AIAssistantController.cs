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

    /// <summary>
    /// İstemci, awaiting_client cevabındaki aksiyonları uyguladıktan sonra gerçek sonuçları
    /// ve güncel ekran özetini gönderir; bekleyen tur bu sonuçlarla sürer.
    /// </summary>
    [HttpPost("chat/continue")]
    public async Task<IActionResult> Continue(
        AiContinueRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _assistantService.ContinueAsync(
                request,
                cancellationToken);

        return response is null
            ? StatusCode(StatusCodes.Status410Gone, new
            {
                message = "Asistan oturumunun süresi doldu. Lütfen mesajınızı tekrar gönderin."
            })
            : Ok(response);
    }
}