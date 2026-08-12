using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Context;
using Server.Dto;
using Server.Model;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

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

            if(new PasswordHasher<Users>().VerifyHashedPassword(userCheck, userCheck.Password, data.Password) == PasswordVerificationResult.Failed)
                return BadRequest("Incorrect 'Username' or 'Password'");

            var jwt = generateJwtToken(userCheck, null!, new DateTime().AddDays(1));
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
                var jwt = generateJwtToken(newUser, null!, new DateTime().AddDays(1));

                return Ok(jwt);
            }
            catch (DbException ex)
            {
                return BadRequest("Something went wrong, please try again later");
            }
        }

        [HttpPost("emailverify")]
        public async Task<ActionResult<string>> EmailVerify(OtpDto dto)
        {
            if(!ModelState.IsValid)
                return BadRequest("Invalid 'Email Address'");

            var userCheck = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if(userCheck is null)
                return NotFound("No account found associated with email, please try again");
            
            var otp = generateOTP();
            SendEmail(dto.Email, otp);
            userCheck.Otp = generateJwtToken(userCheck, dto.Otp!, new DateTime().AddMinutes(3));

            return Ok("Email verified");
        }
        
        [HttpPost("otp")]
        public async Task<ActionResult<bool>> OtpHandler(OtpDto dto)
        {
            var getOtp = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if(getOtp is null)
                return NotFound("No account found associated with email, please create an new account");

           // iMPLEMENT A method to check or decode the otp token and then verify if the user sent otp matches


            //     return Ok(true);
            // else 
            //     return BadRequest("Invalid 'Otp', please try again");
        }

        public string generateOTP()
        {

            string otp = ""; 
            for (int i = 0; i < 5; i++)
            {
                otp += RandomNumberGenerator.GetInt32(10000, 100000).ToString();
            }
            return otp;
        }

        public async void SendEmail(string recipientEmail, string senderEmail, string body)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Demos", senderEmail));
            message.To.Add(new MailboxAddress("Recipient", recipientEmail));
            message.Subject = "Demos OTP Code";

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using var client = new SmtpClient();

            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config.GetValue<string>("Mail: Username")!, _config.GetValue<string>("Mail: Password")!);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task<AuthenticationState> DecodeToken(string jwt)
        {
            var JwtHandler = new JwtSecurityTokenHandler();
            var tokenData =  JwtHandler.ReadJwtToken(jwt);

            var identity = new ClaimsIdentity(tokenData.Claims, "jwt");
            var otp = new ClaimsPrincipal(identity);

            return new AuthenticationState(otp);
        }


        public string generateJwtToken(Users user, string otp, DateTime expires)
        {
            var claims =  new List<Claim>();
            if (otp is not null)
            {
                claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, otp)
                };
            }
            else
            {
                claims = new List<Claim>{
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                };
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("Settings:Token") ?? ""));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenConstructor = new JwtSecurityToken(
                issuer: _config.GetValue<string>("Settings:Issuer"),
                audience: _config.GetValue<string>("Settings:Audience"),
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenConstructor);
        }
    }
}