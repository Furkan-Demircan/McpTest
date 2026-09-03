using Domain.Exceptions;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string TcNo { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string MotherName { get; private set; } = default!;
    public string FatherName { get; private set; } = default!;
    public DateOnly BirthDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core için parametresiz constructor
    protected User() { }

    public User(
        string firstName,
        string lastName,
        string tcNo,
        string email,
        string motherName,
        string fatherName,
        DateOnly birthDate)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;

        SetFirstName(firstName);
        SetLastName(lastName);
        SetTcNo(tcNo);
        SetEmail(email);
        SetMotherName(motherName);
        SetFatherName(fatherName);
        SetBirthDate(birthDate);
    }

    public void SetFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("Ad alanı boş bırakılamaz.");
        FirstName = firstName.Trim();
    }

    public void SetLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Soyad alanı boş bırakılamaz.");
        LastName = lastName.Trim();
    }

    public void SetTcNo(string tcNo)
    {
        if (string.IsNullOrWhiteSpace(tcNo))
            throw new DomainException("TC Kimlik Numarası boş bırakılamaz.");
        
        tcNo = tcNo.Trim();
        if (tcNo.Length != 11 || !tcNo.All(char.IsDigit))
            throw new DomainException("TC Kimlik Numarası 11 haneli rakamlardan oluşmalıdır.");

        TcNo = tcNo;
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("E-posta adresi boş bırakılamaz.");
        
        email = email.Trim().ToLowerInvariant();
        if (!email.Contains('@') || !email.Contains('.'))
            throw new DomainException("Geçerli bir e-posta adresi giriniz.");

        Email = email;
    }

    public void SetMotherName(string motherName)
    {
        if (string.IsNullOrWhiteSpace(motherName))
            throw new DomainException("Anne adı boş bırakılamaz.");
        MotherName = motherName.Trim();
    }

    public void SetFatherName(string fatherName)
    {
        if (string.IsNullOrWhiteSpace(fatherName))
            throw new DomainException("Baba adı boş bırakılamaz.");
        FatherName = fatherName.Trim();
    }

    public void SetBirthDate(DateOnly birthDate)
    {
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("Doğum tarihi gelecekte bir tarih olamaz.");
        BirthDate = birthDate;
    }

    public void Update(
        string firstName,
        string lastName,
        string email,
        string motherName,
        string fatherName,
        DateOnly birthDate)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetEmail(email);
        SetMotherName(motherName);
        SetFatherName(fatherName);
        SetBirthDate(birthDate);
        UpdatedAt = DateTime.UtcNow;
    }
}
