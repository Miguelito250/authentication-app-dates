using AuthenticationService.Services;
using AuthenticationService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AuthenticationContext _context;
        private readonly Jwt _jwtService;
        private readonly int _dayExpireRefreshToken = 10;
        private readonly int _dayExpireAccessToken = 30;

        public AuthController(
            AuthenticationContext context,
            Jwt jwtService
            )
        {
            _jwtService = jwtService;
            _context = context;
        }

        /**
         * RequestRefreshedToken([FromBody] string refreshToken)
         * Updates the refresh token in the database and returns a new access token and refresh token.
         * @param refreshToken The refresh token to update.
         * @return A JSON object containing the new access token and refresh token.
        */
        [Route("request-refreshed-token")]
        [HttpPost]
        public async Task<IActionResult> RequestRefreshedToken([FromBody] string refreshToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);


            var token = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token is null || token.User is null) return BadRequest(new { message = "The token does not exist or does not have any user assigned" });

            if (DateTime.UtcNow < token.ExpirationDate) return Ok(new { message = "The token has not expired yet" });

            var newAccessToken = _jwtService.CreateJWT(token.User, _dayExpireAccessToken);
            var newRefreshToken = _jwtService.CreateJWT(token.User, _dayExpireAccessToken);

            token.ExpirationDate = DateTime.UtcNow.AddSeconds(_dayExpireRefreshToken);

            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
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
            if (!ModelState.IsValid) return BadRequest(new
            {
                message = "Invalid model",
                ModelState
            });

            var isValidUser = await AuthenticateUser(loginModel.User, loginModel.Password);

            if (isValidUser is null) return Unauthorized(new { message = "Incorrect credentials" });

            var accesToken = _jwtService.CreateJWT(isValidUser, _dayExpireAccessToken);
            var refreshToken = _jwtService.CreateJWT(isValidUser, _dayExpireRefreshToken);

            return Ok(new { accesToken, refreshToken });
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
                ExpirationDate = DateTime.UtcNow.AddSeconds(_dayExpireRefreshToken),
                UserId = user.UUID
            };

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.RefreshToken = refToken;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Succeed created user",
                accessToken,
                refreshToken
            });
        }

        /**
         * CallAuthenticationUser function
         * Invokes the AuthenticateUser method to revalidate user credentials for security-sensitive actions such as password changes or account deletion.
         * @param string loginModel.User
         */
        [Route("authenticate-user")]
        [HttpPost]
        public async Task<User?> CallAuthenticationUser([FromBody] Login loginModel)
        {
            return await AuthenticateUser(loginModel.User, loginModel.Password);
        }

        /**
        * AuthenticateUser function
        * Responsible for querying the database to verify user credentials
        * @param username The username to authenticate
        * @param password The password to verify
        * @return null if the credentials are invalid
        */
        private async Task<User?> AuthenticateUser(string username, string password)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);

            if (user is null) return null;

            var passwordIsValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return passwordIsValid ? user : null;
        }
    }

}
