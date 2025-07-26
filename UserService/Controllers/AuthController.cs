using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTO;
using UserService.Models;
using UserService.Services;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly JwtService _jwtService;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, JwtService jwtService, IConfiguration config, ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _secret = config["Jwt:Key"];
        _issuer = config["Jwt:Issuer"];
        _audience = config["Jwt:Audience"];
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto dto)
    {
        try
        {
            var existingUser = await _userService.GetByUsernameAsync(dto.Username);
            if (existingUser != null)
                return BadRequest("Username already exists");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "User"
            };

            await _userService.CreateAsync(user);
            _logger.LogInformation("Đăng ký user thành công");
            return Ok("User registered");
        }
        catch(Exception ex)
        {
            _logger.LogError("AuthController - Register: {0}", ex.ToString());
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto dto)
    {
        try
        {
            var user = await _userService.GetByUsernameAsync(dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = _jwtService.GenerateToken(user);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            _logger.LogError("AuthController - Login: {0}", ex.ToString());
            return StatusCode(500, "Internal Server Error");
        }
    }
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
            var email = payload.Email;
            var name = payload.Name;

            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    Username = email,
                    Email = email,
                    Role = "User"
                };
                await _userService.CreateAsync(user);
            }

            var token = _jwtService.GenerateToken(user);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Xác thực Google thất bại", error = ex.Message });
        }
    }


}
