using System.Security.Claims;
using AuthenticationService.Models;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration){
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Login([FromBody] Login model)
        {
            var isValidUser = AuthenticateUser(model.User, model.Password);

            if (!isValidUser)
            {
                return Unauthorized(new {message = "Incorrect credentials"});
            }

            var _jwt = new Jwt(_configuration);
            
            var token = _jwt.CreateJWT(model);
            
            return Ok(new {token});
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
