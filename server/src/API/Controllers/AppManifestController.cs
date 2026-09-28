using MCP.Server.Manifest;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Koddan üretilen uygulama manifest'i. İstemci route'ları, menüyü ve formları buradan kurar.
/// </summary>
[ApiController]
[Route("api/app-manifest")]
public class AppManifestController(AppManifestStore store) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AppManifest), StatusCodes.Status200OK)]
    public IActionResult Get() => Ok(store.Manifest);

    /// <summary>
    /// Tek bir formun şeması. Büyük uygulamada istemci özet manifest'i alıp
    /// form şemasını sayfa açılınca buradan yükleyebilir.
    /// </summary>
    [HttpGet("forms/{formId}")]
    [ProducesResponseType(typeof(FormDefinition), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetForm(string formId) =>
        store.FindForm(formId) is { } form
            ? Ok(form)
            : NotFound(new { message = $"Form bulunamadı: {formId}" });
}
