using ApiBank.Application.Interfaces;
using ApiBank.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ApiBank.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Test()
    {
        return Ok("token");
    }
    
    [HttpPost]
    public IActionResult Authenticate([FromBody] AuthRequest credentials)
    {
        var token = _authService.Authenticate(credentials.Username, credentials.Password);
        if (token == null)
        {
            return Unauthorized();
        }
        return Ok(token);
    }
    
    
}