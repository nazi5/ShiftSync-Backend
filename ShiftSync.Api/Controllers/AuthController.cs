using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ShiftSync.Api.Data;
using ShiftSync.Api.Models; 
using ShiftSync.Api.DTOs;

namespace ShiftSync.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.PasswordHash == request.Password);
            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }
            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.UserName) 
                }),
                Expires = DateTime.UtcNow.AddHours(8), // Token valid for one full shift
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtString = tokenHandler.WriteToken(token);
            return Ok(new { Token = jwtString, Username = user.UserName, UserId = user.Id });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Email already in use");
            }
            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = request.Password // In production, hash the password
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "User registered successfully", UserId = user.Id });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Select(u => new UserResponse { Id = u.Id, UserName = u.UserName, Email = u.Email }).ToListAsync();
            return Ok(users);
        }
    }
}
