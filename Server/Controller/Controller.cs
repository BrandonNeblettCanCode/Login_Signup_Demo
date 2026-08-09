using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Context;
using Server.Dto;
using Server.Model;

namespace Server.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServerController : ControllerBase
    {
        private readonly ApiContext _context;
        public readonly IConfiguration _config;

        public ServerController(ApiContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto data)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var userCheck = await _context.Users.FirstOrDefaultAsync(u => u.Username == data.Username);
            if(userCheck is null)
                return BadRequest("Incorrect 'Username' or 'Password'");

            if(new PasswordHasher<Users>().VerifyHashedPassword(userCheck, data.Password, userCheck.Password) == PasswordVerificationResult.Failed)
                return BadRequest("Incorrect 'Username' or 'Password'");

            var jwt = generateJwtToken(userCheck);
            if(string.IsNullOrWhiteSpace(jwt))
                return BadRequest("Something went wrong");
            
            return Ok(jwt);
        }

        [HttpPost("signup")]
        public async Task<ActionResult<string>> SignUp(SignUpDto data)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var userCheck = await _context.Users.FirstOrDefaultAsync(u => u.Username == data.Username);
            if(userCheck is not null)
                return BadRequest("'Username' is already taken");
            
            var newUser = new Users(); 
            newUser = new Users
            {
                Username = data.Username,
                Password = new PasswordHasher<Users>().HashPassword(newUser, data.Password)
            };

            await _context.Users.AddAsync(newUser);
            try
            {
                await _context.SaveChangesAsync();
                var jwt = generateJwtToken(newUser);

                return Ok(jwt);
            }
            catch (DbException ex)
            {
                return BadRequest("Something went wrong, please try again later");
            }
        }
        
        public string generateJwtToken(Users user)
        {
            var claims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("Settings:Token") ?? ""));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenConstructor = new JwtSecurityToken(
                issuer: _config.GetValue<string>("Settings:Issuer"),
                audience: _config.GetValue<string>("Settings:Audience"),
                claims: claims,
                expires: new DateTime().AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenConstructor);
        }
    }
}