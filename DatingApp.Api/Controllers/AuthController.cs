using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.Api.Data;
using DatingApp.Api.Dtos;
using DatingApp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.Api.Controllers
{
    [Route("Api/[Controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository authRepository, IConfiguration config)
        {
            _config = config;
            repo = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            var userName = userForRegisterDto.userName.ToLower();
            if (await repo.UserExists(userName))
            {
                return BadRequest("User name already exists");
            }

            var userToCreate = new User
            {
                Username = userName
            };

            var createuser = await repo.Register(userToCreate, userForRegisterDto.password);
            return StatusCode(201);

        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var loginInfo = await repo.Login(userForLoginDto.userName.ToLower(), userForLoginDto.password);
            if (loginInfo == null)
            {
                return Unauthorized();
            }

            var claim = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, loginInfo.Id.ToString()),
                new Claim(ClaimTypes.Name, loginInfo.Username.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claim),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}