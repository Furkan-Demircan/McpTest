using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Domain.Entities;

public partial class Course
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public int Credits { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core için parametresiz constructor
    protected Course() { }

    public Course(string name, string code, int credits, string? description)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;

        SetName(name);
        SetCode(code);
        SetCredits(credits);
        SetDescription(description);
    }

    [GeneratedRegex(@"^[A-Z]{3}\d{3}$")]
    private static partial Regex CodePattern();

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Ders adı boş bırakılamaz.");
        if (name.Trim().Length > 100)
            throw new DomainException("Ders adı en fazla 100 karakter olabilir.");
        Name = name.Trim();
    }

    public void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Ders kodu boş bırakılamaz.");

        code = code.Trim().ToUpperInvariant();
        if (!CodePattern().IsMatch(code))
            throw new DomainException("Ders kodu 3 harf ve 3 rakamdan oluşmalıdır (örn. MAT101).");

        Code = code;
    }

    public void SetCredits(int credits)
    {
        if (credits is < 1 or > 10)
            throw new DomainException("Kredi 1 ile 10 arasında olmalıdır.");
        Credits = credits;
    }

    public void SetDescription(string? description)
    {
        description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (description?.Length > 500)
            throw new DomainException("Açıklama en fazla 500 karakter olabilir.");
        Description = description;
    }
}
