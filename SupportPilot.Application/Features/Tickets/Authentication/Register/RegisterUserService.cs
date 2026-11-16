
using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Application.Common.Results;
using SupportPilot.Core.Entities;

namespace SupportPilot.Application.Features.Authentication.Register;

public sealed class RegisterUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> ExecuteAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return Result<Guid>.Failure("First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            return Result<Guid>.Failure("Last name is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<Guid>.Failure("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result<Guid>.Failure("Password is required.");

        if (request.Password.Length < 8)
            return Result<Guid>.Failure(
                "Password must contain at least 8 characters.");

        if (System.Text.Encoding.UTF8.GetByteCount(request.Password) > 72)
            return Result<Guid>.Failure(
                "Password cannot exceed 72 UTF-8 bytes.");

        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _userRepository.ExistsByEmailAsync(
            email,
            cancellationToken);

        if (emailExists)
            return Result<Guid>.Failure(
                "An account with this email already exists.");

        var user = new User(
            request.FirstName,
            request.LastName,
            email);

        var passwordHash = _passwordHasher.Hash(request.Password);

        user.SetPasswordHash(passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}