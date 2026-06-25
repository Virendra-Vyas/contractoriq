using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ContractorIQ.API.Configuration;
using ContractorIQ.API.Data;
using ContractorIQ.API.DTOs.Auth;
using ContractorIQ.API.Entities;
using ContractorIQ.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ContractorIQ.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtSettings _jwt;

    public AuthService(AppDbContext db, IOptions<JwtSettings> jwt)
    {
        _db = db;
        _jwt = jwt.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check email already exists
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower());
        if (exists)
            throw new InvalidOperationException("An account with this email already exists.");

        // Create user
        var user = new User
        {
            Email = request.Email.ToLower().Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        // Create free subscription
        var subscription = new Subscription
        {
            UserId = user.Id,
            Tier = "free",
            Status = "active"
        };

        // Create empty profile
        var profile = new ContractorProfile
        {
            UserId = user.Id
        };

        _db.Users.Add(user);
        _db.Subscriptions.Add(subscription);
        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync();

        return GenerateAuthResponse(user, "free");
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var tier = user.Subscription?.Tier ?? "free";
        return GenerateAuthResponse(user, tier);
    }

    public async Task<AuthResponse?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        var tier = user.Subscription?.Tier ?? "free";
        return GenerateAuthResponse(user, tier);
    }

    private AuthResponse GenerateAuthResponse(User user, string tier)
    {
        var expiresAt = DateTime.UtcNow.AddHours(_jwt.ExpiryHours);
        var token = GenerateJwtToken(user, tier, expiresAt);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserId = user.Id,
            Tier = tier,
            ExpiresAt = expiresAt
        };
    }

    private string GenerateJwtToken(User user, string tier, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim("tier", tier),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}