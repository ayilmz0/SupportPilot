namespace SupportPilot.Application.Common.Interfaces;

public interface IRefreshTokenGenerator
{
    string Generate();

    string Hash(string token);

    DateTime GetExpirationDateUtc();
}