using ForumApi.Services;
using ForumAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ForumApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenService _jwtTokenService;
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(UserService userService, IConfiguration configuration,JwtTokenService jwtTokenService)
        {
            _userService = userService;
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var existingUser = await _userService.GetByUsernameAsync(request.Username);
            if (existingUser != null)
                return BadRequest("Username already exists.");

            existingUser = await _userService.GetByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest("Email already registered.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new ForumAPI.Models.User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hashedPassword
            };

            await _userService.CreateAsync(user);
            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _userService.GetByUsernameAsync(request.Username);
            if (user == null)
                return Unauthorized("Invalid username or password.");

            bool validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!validPassword)
                return Unauthorized("Invalid username or password.");

            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new { Token = token });

        }

        [Authorize]
        [HttpPost("testAuthorization")]
        public IActionResult TestAuthorization()
        {
            return Ok("Authorization successful. You are authenticated.");
        }

        [Authorize(Roles = "admin")]
        [HttpPost("adminTestAuthorization")]
        public IActionResult AdminTestAuthorization()
        {
            return Ok("Authorization successful. You are authenticated as admin.");
        }
        
    }
}


