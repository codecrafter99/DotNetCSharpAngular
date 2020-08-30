using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DotNetWeb.API.Data;
using DotNetWeb.API.Dtos;
using DotNetWeb.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DotNetWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAuthRepository _repo;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto usrDto)
        {
            // validate request

            usrDto.Username = usrDto.Username.ToLower();

            if(await _repo.UserExists(usrDto.Username))
                return BadRequest("User already exists");
            
            var userToCreate = new User 
            {
                Username = usrDto.Username
            };

            var createdUser = await _repo.Register(userToCreate, usrDto.Password);
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto usrDto)
        {
            var userFromRepo = await _repo.Login(
            usrDto.Username.ToLower(), usrDto.Password);
        
            if (userFromRepo == null)
                return Unauthorized();
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            var key =  new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config.GetSection(
                        "AppSettings:Token").Value
            ));

            var creds = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha512Signature
            );

            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(
                tokenDescriptor
            );

            return Ok( new {
                token = tokenHandler.WriteToken(token)
            });
            
        }        
    }
}