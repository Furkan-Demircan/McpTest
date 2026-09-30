using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Application.DTOs;
using Application.Interfaces;
using Application.MCP.Tools;
using Domain.Exceptions;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace API.Tools;

/// <summary>
/// Öğrenci verisine erişen tool'lar. Ekran tool'larının aksine istemciye aksiyon göndermez,
/// doğrudan uygulama servislerini çağırır. Bu yüzden veritabanını bilen API host'unda durur.
/// </summary>
[McpServerToolType]
public static class StudentTools
{
    private const int MaxSearchResults = 10;
    private const int MaxPageSize = 50;

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Kayıtlı bir öğrenciyi veritabanında arar: TC Kimlik Numarasına göre (tam eşleşme) veya ada/soyada göre. " +
    "Kullanıcı kayıtlı bir öğrencinin bilgisini sorduğunda, bir öğrencinin kayıtlı olup olmadığını merak ettiğinde kullanılır. " +
    "TC 11 haneliyse tcNo, değilse name ver. name'deki her kelime adda veya soyadda geçmelidir (örn. 'ali kaya'). " +
    "İsim aramasında en fazla 10 sonuç döner.")]
    public static async Task<StudentSearchResult> FindStudent(
        IUserService userService,
        [Description("11 haneli TC Kimlik Numarası.")] string? tcNo = null,
        [Description("Ad, soyad veya ikisi birden, örn. 'Ali Kaya'.")] string? name = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<UserResponseDto> students;

        if (!string.IsNullOrWhiteSpace(tcNo))
        {
            var student = await userService.GetUserByTcNoAsync(tcNo.Trim(), cancellationToken);
            students = student is null ? [] : [student];
        }
        else if (!string.IsNullOrWhiteSpace(name))
        {
            students = await userService.SearchUsersByNameAsync(name, MaxSearchResults, cancellationToken);
        }
        else
        {
            throw new McpException("tcNo veya name verilmelidir.");
        }

        return new StudentSearchResult
        {
            Found = students.Count > 0,
            Students = students.Select(StudentInfo.From).ToList(),
            Hint = students.Count > 0
                ? null
                : "Bu bilgiyle kayıtlı öğrenci bulunamadı. Kullanıcıya açıkça söyle; yazımı kontrol etmesini veya TC ile aramayı öner."
        };
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Kayıtlı öğrencileri en yeni kayıttan başlayarak sayfa sayfa listeler; toplam kayıt sayısını da döner. " +
    "Kullanıcı öğrenci listesini, kaç öğrenci olduğunu veya son eklenenleri sorduğunda kullanılır. " +
    "Belirli bir öğrenci aranıyorsa find_student kullanılır.")]
    public static async Task<StudentListResult> ListStudents(
        IUserService userService,
        [Description("Sayfa numarası, 1'den başlar.")] int page = 1,
        [Description("Sayfa başına kayıt (en fazla 50).")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await userService.GetUsersPageAsync(page, Math.Clamp(pageSize, 1, MaxPageSize), cancellationToken);

        return new StudentListResult
        {
            Total = result.Total,
            Page = result.Page,
            PageSize = result.PageSize,
            HasMore = result.Page * result.PageSize < result.Total,
            Students = result.Items.Select(StudentInfo.From).ToList()
        };
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Sohbetten yeni öğrenci kaydı oluşturur (veritabanına yazar). İki adımlıdır: " +
    "1) confirmed=false ile çağır: bilgiler doğrulanır, kaydedilmez; eksik/hatalı alanlar 'invalid', " +
    "sorun yoksa 'needs_confirmation' ve önizleme döner. Önizlemeyi kullanıcıya göster ve onay iste. " +
    "2) Kullanıcı SONRAKİ mesajında açıkça onaylarsa AYNI değerlerle confirmed=true çağır. " +
    "Önizlenmemiş veya değiştirilmiş değerlerle confirmed=true kaydetmez, yeniden önizleme döner. " +
    "Kullanıcı sadece formu doldurmanı istiyorsa bunu değil fill_fields'ı kullan.")]
    public static async Task<SaveStudentResult> SaveStudent(
        IUserService userService,
        PendingConfirmationStore confirmations,
        [Description("Ad.")] string firstName,
        [Description("Soyad.")] string lastName,
        [Description("11 haneli TC Kimlik Numarası.")] string tcNo,
        [Description("E-posta adresi.")] string email,
        [Description("Anne adı.")] string motherName,
        [Description("Baba adı.")] string fatherName,
        [Description("Doğum tarihi, YYYY-MM-DD.")] string birthDate,
        [Description("Kullanıcı önizlemeyi gördükten sonra açıkça onayladıysa true.")] bool confirmed = false,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        var normalizedDate = FormTools.NormalizeDate(birthDate?.Trim());
        if (!DateOnly.TryParseExact(normalizedDate, "yyyy-MM-dd", out var parsedBirthDate))
        {
            errors.Add($"Doğum tarihi anlaşılamadı: '{birthDate}'.");
        }

        var dto = new CreateUserDto
        {
            FirstName = firstName?.Trim() ?? string.Empty,
            LastName = lastName?.Trim() ?? string.Empty,
            TcNo = tcNo?.Trim() ?? string.Empty,
            Email = email?.Trim() ?? string.Empty,
            MotherName = motherName?.Trim() ?? string.Empty,
            FatherName = fatherName?.Trim() ?? string.Empty,
            BirthDate = parsedBirthDate
        };

        // Sözleşmedeki kurallar (API'nin model doğrulamasıyla aynı)
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, validateAllProperties: true);
        errors.AddRange(validationResults.Select(result => result.ErrorMessage ?? "Geçersiz değer."));

        if (errors.Count == 0)
        {
            if (await userService.GetUserByTcNoAsync(dto.TcNo, cancellationToken) is not null)
            {
                errors.Add($"'{dto.TcNo}' TC Kimlik Numarası ile kayıtlı bir öğrenci zaten var.");
            }

            if (await userService.GetUserByEmailAsync(dto.Email, cancellationToken) is not null)
            {
                errors.Add($"'{dto.Email}' e-posta adresi ile kayıtlı bir öğrenci zaten var.");
            }
        }

        var preview = new StudentPreview
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            TcNo = dto.TcNo,
            Email = dto.Email,
            MotherName = dto.MotherName,
            FatherName = dto.FatherName,
            BirthDate = normalizedDate ?? birthDate ?? string.Empty
        };

        if (errors.Count > 0)
        {
            return new SaveStudentResult
            {
                Status = SaveStudentStatus.Invalid,
                Preview = preview,
                Errors = errors,
                Hint = "Kaydedilmedi. Hataları kullanıcıya söyle ve doğru bilgiyi iste; bilgi uydurma."
            };
        }

        var key = $"save_student:{dto.FirstName}|{dto.LastName}|{dto.TcNo}|{dto.Email.ToLowerInvariant()}|{dto.MotherName}|{dto.FatherName}|{dto.BirthDate:yyyy-MM-dd}";

        // Onay ancak aynı değerler daha önce önizlendiyse geçerlidir
        if (!confirmed || !confirmations.TryTake(key))
        {
            confirmations.Add(key);

            return new SaveStudentResult
            {
                Status = SaveStudentStatus.NeedsConfirmation,
                Preview = preview,
                Hint = confirmed
                    ? "Bu değerler daha önce önizlenmemiş (veya değişmiş); kaydedilmedi. Önizlemeyi kullanıcıya göster ve yeniden onay iste."
                    : "Henüz kaydedilmedi. Önizlemeyi kullanıcıya madde madde göster ve 'Kaydedeyim mi?' diye sor. " +
                      "Onay verirse aynı değerlerle confirmed=true çağır."
            };
        }

        try
        {
            var created = await userService.CreateUserAsync(dto, cancellationToken);

            return new SaveStudentResult
            {
                Status = SaveStudentStatus.Saved,
                Student = StudentInfo.From(created),
                Hint = "Öğrenci kaydedildi. Ekranda aynı bilgilerle dolu bir form varsa kullanıcıya formu " +
                       "ayrıca kaydetmemesini söyle (aynı TC ile ikinci kayıt reddedilir)."
            };
        }
        catch (DomainException ex)
        {
            return new SaveStudentResult
            {
                Status = SaveStudentStatus.Failed,
                Preview = preview,
                Errors = [ex.Message],
                Hint = "Kaydedilemedi; nedeni kullanıcıya söyle."
            };
        }
    }
}

public static class SaveStudentStatus
{
    public const string Invalid = "invalid";
    public const string NeedsConfirmation = "needs_confirmation";
    public const string Saved = "saved";
    public const string Failed = "failed";
}

public class StudentInfo
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string TcNo { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string MotherName { get; init; } = default!;
    public string FatherName { get; init; } = default!;
    public string BirthDate { get; init; } = default!;
    public DateTime CreatedAt { get; init; }

    public static StudentInfo From(UserResponseDto user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        TcNo = user.TcNo,
        Email = user.Email,
        MotherName = user.MotherName,
        FatherName = user.FatherName,
        BirthDate = user.BirthDate.ToString("yyyy-MM-dd"),
        CreatedAt = user.CreatedAt
    };
}

public class StudentSearchResult
{
    public bool Found { get; init; }
    public List<StudentInfo> Students { get; init; } = [];
    public string? Hint { get; init; }
}

public class StudentListResult
{
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public bool HasMore { get; init; }
    public List<StudentInfo> Students { get; init; } = [];
}

public class StudentPreview
{
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string TcNo { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string MotherName { get; init; } = default!;
    public string FatherName { get; init; } = default!;
    public string BirthDate { get; init; } = default!;
}

public class SaveStudentResult
{
    public string Status { get; init; } = default!;
    public StudentPreview? Preview { get; init; }
    public StudentInfo? Student { get; init; }
    public List<string>? Errors { get; init; }
    public string? Hint { get; init; }
}
