using System.Text;
using LibraryApi.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LibraryApi.Authentication;

public record AccessToken(string Token, DateTime ExpiresAt);

public class TokenService(IOptions<JwtOptions> options)
{

    private readonly JsonWebTokenHandler _handler = new();

    public AccessToken CreateAccessToken(Member member)
    {
        var jwt = options.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(jwt.AccessTokenMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Expires = expiresAt,
            Claims = new Dictionary<string, object>
            {
                [AuthClaims.Sub] = member.Id.ToString(),
                [AuthClaims.Email] = member.Email,
                [AuthClaims.Name] = member.FullName,
                [AuthClaims.Role] = member.Role.ToString(),
                [AuthClaims.Jti] = Guid.NewGuid().ToString(),
            },
            SigningCredentials = new SigningCredentials(CreateSigningKey(jwt), SecurityAlgorithms.HmacSha256),

        };
        return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
    }

    public static SymmetricSecurityKey CreateSigningKey(JwtOptions jwt) =>
            new(Encoding.UTF8.GetBytes(jwt.SigningKey));
}
