using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Generates JWT token for the given username.
/// </summary>
public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(string username)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Convert.FromBase64String(jwtSettings["Key"]!);

        var claims = new[] { new Claim(ClaimTypes.Name, username) };
        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = creds
        };

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));
    }
}
