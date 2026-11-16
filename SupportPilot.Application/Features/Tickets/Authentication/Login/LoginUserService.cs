
using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Application.Common.Results;
using SupportPilot.Core.Entities;

namespace SupportPilot.Application.Features.Authentication.Login;

public sealed class LoginUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginUserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result<LoginUserResponse>> ExecuteAsync(
        LoginUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<LoginUserResponse>.Failure(
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<LoginUserResponse>.Failure(
                "Password is required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Result<LoginUserResponse>.Failure(
                "Invalid email or password.");
        }

        var isPasswordValid = _passwordHasher.Verify(
            request.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            return Result<LoginUserResponse>.Failure(
                "Invalid email or password.");
        }

        // Access Token oluştur.
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            user.Role.ToString());

        // Refresh Token oluştur.
        var refreshTokenValue = _refreshTokenGenerator.Generate();

        // Refresh Token'ın hash'ini oluştur.
        var refreshTokenHash = _refreshTokenGenerator.Hash(
            refreshTokenValue);

        // Veritabanına kaydedilecek Refresh Token entity'sini oluştur.
        var refreshToken = new RefreshToken(
            user.Id,
            refreshTokenHash,
            _refreshTokenGenerator.GetExpirationDateUtc());

        // Hash edilmiş Refresh Token'ı veritabanına kaydet.
        await _refreshTokenRepository.AddAsync(
            refreshToken,
            cancellationToken);

        var response = new LoginUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            accessToken,
            refreshTokenValue);

        return Result<LoginUserResponse>.Success(response);
    }
}