using AuthenticationService.Services;
using Microsoft.EntityFrameworkCore;
using AuthenticationService.Models;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using AuthenticationService.Utilities;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AuthenticationContext _context;
        private readonly Jwt _jwtService;
        private readonly int _dayExpireRefreshToken = 2;
        private readonly int _dayExpireAccessToken = 1;

        public AuthController(
            AuthenticationContext context,
            Jwt jwtService
            )
        {
            _jwtService = jwtService;
            _context = context;
        }

        /**
         * ValidateToken()
         * Validates the provided token by invoking the middleware token validation mechanism.
         * @return IActionResult indicating the validation result.
        */
        [Route("validate-token")]
        [HttpPost]
        [Authorize]

        public IActionResult ValidateToken()
        {
            return Ok(new Response(true, "The token is valid"));
        }

        /**
         * RequestNewTokens([FromBody] string refreshToken)
         * Updates the refresh token in the database and returns a new access token and refresh token.
         * @param refreshToken The refresh token to update.
         * @return A JSON object containing the new access token and refresh token.
        */
        [Route("request-new-tokens")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestNewTokens()
        {
            Response refreshToken = UtilitiesFunctions.GetElementHeader(HttpContext.Request.Headers, "Authorization");

            RefreshToken? token = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken.Data!.ToString());

            if (token is null || token.User is null) return BadRequest(new { message = "The token does not exist or does not have any user assigned" });

            string newAccessToken = _jwtService.CreateJWT(token.User, _dayExpireAccessToken);
            string newRefreshToken = _jwtService.CreateJWT(token.User, _dayExpireAccessToken);

            token.Token = newRefreshToken;

            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();

            return Ok(new Response(true, "Tokens refreshed", new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            }));
        }

        /**
         * Signin function
         * Handles user authentication, returning an access token and refresh token upon successful login.
         * @param [FromBody] Login loginModel
         * @return Task<IActionResult> with access token and refresh token.
        */
        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> Signin([FromBody] Login loginModel)
        {
            if (!ModelState.IsValid) return BadRequest(new Response(false, "Invalid model", new { ModelState }));

            User? existUser = await AuthenticateUser(loginModel.Email, loginModel.Password);

            if (existUser is null) return Unauthorized(new Response(false, "Incorrect credentials"));

            string accesToken = _jwtService.CreateJWT(existUser, _dayExpireAccessToken);
            string refreshToken = _jwtService.CreateJWT(existUser, _dayExpireRefreshToken);

            RefreshToken refreshTokenEntity = existUser.RefreshToken!;
            refreshTokenEntity.Token = refreshToken;
            refreshTokenEntity.ExpirationDate = UtilitiesFunctions.ExpirationToken(_dayExpireRefreshToken);

            _context.RefreshTokens.Update(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new Response(true, "Successful login", new { accesToken, refreshToken }));
        }

        /**
         * SignUp function
         * Handles user registration, assigning a refresh token to the user in the database.
         * @param [FromBody] User user
         * @return Task<IActionResult> with a success message, access token and refresh token.
         */
        [Route("signup")]
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] User user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var refreshToken = _jwtService.CreateJWT(user, _dayExpireRefreshToken);
            var accessToken = _jwtService.CreateJWT(user, _dayExpireAccessToken);

            var refToken = new RefreshToken
            {
                Token = refreshToken,
                ExpirationDate = UtilitiesFunctions.ExpirationToken(_dayExpireRefreshToken),
                UserId = user.UUID
            };

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.RefreshToken = refToken;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok(new Response
            (
                true,
                "Succeed created user",
                new { accessToken, refreshToken }
            ));
        }

        /**
         * CallAuthenticationUser function
         * Invokes the AuthenticateUser method to revalidate user credentials for security-sensitive actions such as password changes or account deletion.
         * @param string loginModel.User
         */
        [Route("authenticate-user")]
        [HttpPost]
        [Authorize]
        public async Task<User?> CallAuthenticationUser([FromBody] Login loginModel)
        {
            return await AuthenticateUser(loginModel.Email, loginModel.Password);
        }

        /**
        * AuthenticateUser function
        * Responsible for querying the database to verify user credentials
        * @param username The username to authenticate
        * @param password The password to verify
        * @return null if the credentials are invalid
        */
        private async Task<User?> AuthenticateUser(string email, string password)
        {
            User? user = await _context.Users
            .Include(u => u.RefreshToken)
            .SingleOrDefaultAsync(u => u.Email == email);

            if (user is null) return null;

            var passwordIsValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return passwordIsValid ? user : null;
        }
    }

}
