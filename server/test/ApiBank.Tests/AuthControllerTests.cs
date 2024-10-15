using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiBank.Api.Controllers;
using ApiBank.Application.Interfaces;
using ApiBank.Application.Services;
using ApiBank.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace ApiBank.Tests;

public class AuthControllerTests
{
    [Fact]
    public void Authenticate_WithValidCredentials_ReturnsToken()
    {
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(a => a.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns("valid_token");
        var controller = new AuthController(authServiceMock.Object);
        var credentials = new AuthRequest { Username = "admin", Password = "admin" };

        var result = controller.Authenticate(credentials) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("valid_token", result.Value);
    }

    [Fact]
    public void Authenticate_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(a => a.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns((string)null);
        var controller = new AuthController(authServiceMock.Object);
        var credentials = new AuthRequest { Username = "wrong", Password = "wrong" };

        var result = controller.Authenticate(credentials) as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    
    // [Fact]
    // public void GenerateJwtToken_WithValidClaims_ShouldGenerateValidToken()
    // {
    //     // Arrange
    //     var configurationMock = new Mock<IConfiguration>();
    //     configurationMock.SetupGet(c => c["Jwt:Key"]).Returns("YourVerySecretKey");
    //     configurationMock.SetupGet(c => c["Jwt:Issuer"]).Returns("https://yourapp.com");
    //     configurationMock.SetupGet(c => c["Jwt:Audience"]).Returns("https://yourapp.com");
    //
    //     var authService = new AuthService(configurationMock.Object);
    //
    //     // Act
    //     var jwtToken = authService.Authenticate("admin", "admin");
    //
    //     // Assert
    //     Assert.NotNull(jwtToken);
    //     Assert.IsType<string>(jwtToken);
    // }

    
    [Fact]
    public void Authenticate_WithValidToken_ShouldValidateSuccessfully()
    {
        var secretKey = "CAD478256F9DA39BDBB3AB6F2EA4AAAA";
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "admin") }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out var validatedToken);

        Assert.NotNull(validatedToken);
        Assert.NotNull(principal);
        Assert.Equal("admin", principal.Identity.Name);
    }

}