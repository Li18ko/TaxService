using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaxService.Data;
using TaxService.DTOs;
using TaxService.Models;

namespace TaxService.Services;

public class UserService {
    private readonly ApplicationDbContext _context;
    private readonly string _jwtKey;

    public UserService(string jwtKey, ApplicationDbContext context) {
        _context = context;
        _jwtKey = jwtKey;
    }
    
    public async Task<UserProfileDto> GetUserProfileAsync(int userId)
    {
        var user = await _context.Users
            .Where(u => u.Userid == userId)
            .Select(u => new UserProfileDto
            {
                UserId = u.Userid,
                Email = u.Email,
                FullName = u.Fullname
            })
            .FirstOrDefaultAsync();

        return user;
    }
    public async Task<IEnumerable<TaxPayerDto>> GetUserCompaniesAsync(int userId) {
        var companies = await _context.Taxpayers
            .Where(tp => tp.Userid == userId)
            .Select(tp => new TaxPayerDto
            {
                TaxPayerID = tp.Taxpayerid,
                Inn = tp.Inn,
                CompanyName = tp.Companyname,
                Address = tp.Address
            })
            .ToListAsync();

        return companies;
    }
    
    public ClaimsPrincipal ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception)
        {
            return null;  // Ошибка валидации токена
        }
    }
    
    public string GenerateJwtToken(string email, int userId, string fullName) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, fullName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    public async Task<User?> GetUserByEmailAsync(string email) {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}