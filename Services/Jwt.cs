using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using AuthenticationService.Models;

namespace AuthenticationService.Services;
public class Jwt
{
    private readonly string _secretKey;
    public Jwt(IConfiguration configuration){
        _secretKey = configuration["JwtSettings:SecretKey"]!;
    }

    // Se debe de tener en cuenta de Cambiar el contador de Segundos de expiracion del token a dias
    public string CreateJWT(ClaimsIdentity arrayClaims, int daysExpire)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var byteKey = Encoding.UTF8.GetBytes(_secretKey);
        var tokenDes = new SecurityTokenDescriptor
        {
            Subject = arrayClaims,
            Expires = DateTime.UtcNow.AddSeconds(daysExpire),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(byteKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDes);

        return tokenHandler.WriteToken(token);
    }


}