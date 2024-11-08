using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

using AuthenticationService.Utilities;

namespace AuthenticationService.Services;
public class Jwt
{
    private readonly string _secretKey;
    public Jwt(IConfiguration configuration)
    {
        _secretKey = configuration["JwtSettings:SecretKey"]!;
    }

    // Se debe de tener en cuenta de Cambiar el contador de Segundos de expiracion del token a dias
    public string CreateJWT(User user, int daysExpire)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var byteKey = Encoding.UTF8.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Name, user.Username!),
                        new Claim("UUID", user.UUID.ToString())
                    ]
                ),
                Expires = UtilitiesFunctions.ExpirationToken(daysExpire),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(byteKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            throw new Exception("Error creating JWT token", ex);
        }
    }
}