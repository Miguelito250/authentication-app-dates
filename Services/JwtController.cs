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
    public string CreateJWT(Login model)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var byteKey = Encoding.UTF8.GetBytes(_secretKey);
        var tokenDes = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[] {
                        new Claim(ClaimTypes.Name, model.User!),
                    }),
            Expires = DateTime.UtcNow.AddMonths(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(byteKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDes);

        return tokenHandler.WriteToken(token);
    }
}