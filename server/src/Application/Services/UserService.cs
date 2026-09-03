using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        // 1. TC No benzersizlik kontrolü
        if (await _userRepository.ExistsByTcNoAsync(dto.TcNo, cancellationToken))
        {
            throw new DomainException($"'{dto.TcNo}' TC Kimlik Numarası ile kayıtlı bir kullanıcı zaten mevcut.");
        }

        // 2. E-posta benzersizlik kontrolü
        if (await _userRepository.ExistsByEmailAsync(dto.Email, cancellationToken))
        {
            throw new DomainException($"'{dto.Email}' e-posta adresi ile kayıtlı bir kullanıcı zaten mevcut.");
        }

        // 3. Domain entity oluşturulması (Entity içindeki doğrulamalar çalışır)
        var user = new User(
            firstName: dto.FirstName,
            lastName: dto.LastName,
            tcNo: dto.TcNo,
            email: dto.Email,
            motherName: dto.MotherName,
            fatherName: dto.FatherName,
            birthDate: dto.BirthDate
        );

        // 4. Veritabanına ekleme ve kaydetme
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 5. Response DTO dönülmesi
        return UserResponseDto.FromEntity(user);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : UserResponseDto.FromEntity(user);
    }

    public async Task<UserResponseDto?> GetUserByTcNoAsync(string tcNo, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByTcNoAsync(tcNo, cancellationToken);
        return user is null ? null : UserResponseDto.FromEntity(user);
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(UserResponseDto.FromEntity).ToList();
    }
}
