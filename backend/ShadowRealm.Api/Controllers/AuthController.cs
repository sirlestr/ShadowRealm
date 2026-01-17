using Microsoft.AspNetCore.Mvc;
using ShadowRealm.Api.Models.Auth;
using ShadowRealm.Api.Models.Responses;
using ShadowRealm.Api.Services;

namespace ShadowRealm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;

    public AuthController(
        ILogger<AuthController> logger,
        IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for registration");
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(request.Username, request.Password);
        
        if (!result.Success)
        {
            _logger.LogWarning("Registration failed for username {Username}: {Error}", 
                request.Username, result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        _logger.LogInformation("User {Username} registered successfully", request.Username);
        return Ok(new { message = "Registration successful" });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for login");
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(request.Username, request.Password);
        
        if (!result.Success)
        {
            _logger.LogWarning("Login failed for username {Username}", request.Username);
            return Unauthorized(result.ErrorMessage);
        }

        _logger.LogInformation("User {Username} logged in successfully", request.Username);
        
        return Ok(new LoginResponse { Token = result.Token! });
    }
}
