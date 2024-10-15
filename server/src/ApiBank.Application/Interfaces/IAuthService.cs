namespace ApiBank.Application.Interfaces;

public interface IAuthService
{
    string Authenticate(string username, string password);
    string GenerateJwtToken(string username);
}