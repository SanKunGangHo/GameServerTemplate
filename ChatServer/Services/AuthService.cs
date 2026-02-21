using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using ChatServer.Models;
using ChatServer.Repositories;

namespace ChatServer.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    
    public AuthService(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }

    public async Task<User> RegisterAsync(string username, string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return await _userRepository.CreateAsync(username, hash);
    }
    
    public async Task<string?> LoginAsync (string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user is null) return null;
        
        if(!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;
        
        return GenerateToken(user); 
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var token = new JwtSecurityToken(issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            },
            expires: DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpireHours"]!)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}