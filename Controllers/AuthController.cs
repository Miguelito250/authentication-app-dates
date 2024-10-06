using System.Security.Claims;
using AuthenticationService.Models;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AuthenticationContext _context;
        private readonly Jwt _jwtService;

        public AuthController(
            AuthenticationContext context,
            IConfiguration configuration,
            Jwt jwtService
            )
        {
            _configuration = configuration;
            _jwtService = jwtService;
            _context = context;
        }

        [Route("signup")]
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] User user)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dayExpireRefreshToken = 10;
            var dayExpireAccessToken = 30;

            var necessaryClaims = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, user.Username!),
                    new Claim("UUID", user.UUID.ToString())
                ]
            );

            var accessToken = _jwtService.CreateJWT(necessaryClaims, dayExpireAccessToken);
            var refreshToken = _jwtService.CreateJWT(necessaryClaims, dayExpireRefreshToken);

            var refToken = new RefreshToken
            {
                Token = refreshToken,
                ExpirationDate = DateTime.UtcNow.AddSeconds(dayExpireRefreshToken),
                UserId = user.UUID
            };

            user.RefreshToken = refToken;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Succeed created user",
                accesToken = accessToken
            });
        }

        [HttpPost]
        public IActionResult Login([FromBody] Login model)
        {
            var isValidUser = AuthenticateUser(model.User, model.Password);

            if (!isValidUser)
            {
                return Unauthorized(new { message = "Incorrect credentials" });
            }

            var _jwt = new Jwt(_configuration);

            // var token = _jwt.CreateJWT(model);

            return Ok(new { });
        }

        public bool AuthenticateUser(string user, string password)
        {
            if (user == "miguel" && password == "123")
            {
                return true;
            }

            return false;
        }
    }

}
