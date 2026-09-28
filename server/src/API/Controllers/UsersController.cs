using API.Forms;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Yeni bir kullanıcı kaydı oluşturur.
    /// </summary>
    [HttpPost]
    [AppForm("studentForm", PageId = "student-create", Path = "/form",
        Aliases = ["/ogrenci", "/ogrenci-ekle", "/student", "/student-form", "/forma"],
        Title = "Öğrenci Ekleme Formu", Description = "Yeni öğrenci kaydı oluşturulur.",
        Module = AppPages.Module, NavLabel = "Öğrenci Ekle", SubmitLabel = "Öğrenciyi Kaydet")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Yeni kullanıcı ekleme isteği alındı: {TcNo}", dto.TcNo);
        var createdUser = await _userService.CreateUserAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
    }

    /// <summary>
    /// Yeni bir öğretmen kaydı oluşturur. Öğretmen, kullanıcı tablosunda tutulur.
    /// </summary>
    [HttpPost("teachers")]
    [AppForm("teacherForm", PageId = "teacher-create", Path = "/teacher",
        Aliases = ["/teacher-form", "/ogretmen", "/ogretmen-ekle"],
        Title = "Öğretmen Ekleme Formu", Description = "Yeni öğretmen kaydı oluşturulur.",
        Module = AppPages.Module, NavLabel = "Öğretmen Ekle", SubmitLabel = "Öğretmeni Kaydet")]
    [ProducesResponseType(typeof(TeacherResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Yeni öğretmen ekleme isteği alındı: {TcNo}", dto.TcNo);
        var createdUser = await _userService.CreateUserAsync(dto, cancellationToken);

        // Bilinen kısıt: branş kalıcı değil, sadece cevapta geri döner.
        return CreatedAtAction(nameof(GetById), new { id = createdUser.Id },
            TeacherResponseDto.From(createdUser, dto.Branch));
    }

    /// <summary>
    /// Kayıtlı tüm kullanıcıları listeler.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip kullanıcıyı getirir.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = $"ID: {id} olan kullanıcı bulunamadı." });
        }
        return Ok(user);
    }

    /// <summary>
    /// TC Kimlik Numarasına göre kullanıcı arar.
    /// </summary>
    [HttpGet("by-tc/{tcNo}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByTcNo(string tcNo, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByTcNoAsync(tcNo, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = $"TC No: {tcNo} olan kullanıcı bulunamadı." });
        }
        return Ok(user);
    }
}
