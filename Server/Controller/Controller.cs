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
            try {
                if(!ModelState.IsValid)
                {
                    var errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).Where(x => !string.IsNullOrEmpty(x)).Distinct();
                    return  BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = string.Join(";", errorMessages)    
                    });
                }

                var userCheck = await _context.Users.FirstOrDefaultAsync(u => u.Username == data.Username);
                if(userCheck is null)
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = "Incorrect 'Username' or 'Password'"
                    });

                if(new PasswordHasher<Users>().VerifyHashedPassword(userCheck, userCheck.Password, data.Password) == PasswordVerificationResult.Failed)
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = "Incorrect 'Username' or 'Password'"
                    });

                var jwt = generateJwtToken(userCheck, null!, DateTime.UtcNow.AddDays(1));
                if(string.IsNullOrWhiteSpace(jwt))
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = "Something went wrong, try again later"
                    });
            
                return Ok(new ApiResponse {Success = true, Message = jwt});
            }
            catch (DbException ex) {
                return BadRequest(new ApiResponse {Success = false, Errors = ex.Message});
            }            
        }

        [HttpPost("signup")]
        public async Task<ActionResult<string>> SignUp(SignUpDto data)
        {
            try
            {
                if(!ModelState.IsValid) {
                    var errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).Where(x => !string.IsNullOrEmpty(x)).Distinct();
                    return  BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = string.Join(";", errorMessages)    
                    });
                }

                var userCheck = await _context.Users.FirstOrDefaultAsync(u => u.Username == data.Username);
                if(userCheck is not null)
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = "Username' is already taken, please try another one"
                    });

                var emailCheck = await _context.Users.FirstOrDefaultAsync(u => u.Email == data.Email);
                if(emailCheck is not null)
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = "Email Address' is already taken, please try another one"
                    });
                
                var newUser = new Users(); 
                newUser = new Users
                {
                    Username = data.Username,
                    Password = new PasswordHasher<Users>().HashPassword(newUser, data.Password),
                    Email = data.Email
                };

                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();
                
                var jwt = generateJwtToken(newUser, null!, DateTime.UtcNow.AddDays(1));

                return Ok(new ApiResponse {Success = true, Message = jwt});
            }
            catch (DbException Ex)
            {
                return BadRequest(new ApiResponse {Success = false, Errors = Ex.Message});
            }
        }

        [HttpPost("emailverify")]
        public async Task<ActionResult<string>> EmailVerify(OtpDto dto)
        {
            try {
                if(!ModelState.IsValid) {
                    var errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).Where(x => !string.IsNullOrEmpty(x)).Distinct();
                    return  BadRequest(new ApiResponse
                    {
                        Success = false,
                        Errors = string.Join(";", errorMessages)    
                    });
                }

                var userCheck = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if(userCheck is null)
                    return BadRequest(new ApiResponse {
                        Success = false,
                        Errors = "No account found associated with email, please try again"
                    });
                
                var otp = generateOTP();
                await SendEmailAsync(dto.Email, "demos@gmail.com", otp);

                var otpToken = generateJwtToken(userCheck, otp, DateTime.UtcNow.AddSeconds(200));

                userCheck.Otp = otpToken;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse {Success = true, Message = otpToken});

            } catch (Exception Ex) {
                return BadRequest(new ApiResponse {Success = false, Errors = Ex.Message});
            }
        }
        
        [HttpPost("otp")]
        public async Task<ActionResult<bool>> OtpHandler(OtpDto dto)
        {
           try
           {
                var getOtp = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if(getOtp is null)
                    return BadRequest(new ApiResponse {Success = false, Errors = "No account found associated with email, please create an new account"});

            // Implement a method to check or decode the otp token and then verify if the user sent otp matches

                var state = await DecodeToken(getOtp.Otp!);
                var user = state.User;

                if(user.HasClaim(c => c.Value == dto.Otp))
                    return Ok(new ApiResponse {Success = true, Message = generateJwtToken(getOtp, user.FindFirst(ClaimTypes.Authentication)?.Value!, DateTime.UtcNow.AddSeconds(180))});
                else 
                    return BadRequest(new ApiResponse {Success = false, Errors = "Invalid 'Otp', please try again"});
           }
           catch (Exception Ex)
           {
                return BadRequest(new ApiResponse {Success = false, Errors = Ex.Message.Contains("IDX10223") ? "OTP has expired, please request a new one":Ex.Message});
           }
        }

        public string generateOTP()
        {
            return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");;
        }

        public async Task SendEmailAsync(string recipientEmail, string senderEmail, string body)
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
            await client.AuthenticateAsync(_config.GetValue<string>("Mail:Username")!, _config.GetValue<string>("Mail:Password")!);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task<AuthenticationState> DecodeToken(string jwt)
        {
            try
            {    
                var JwtHandler = new JwtSecurityTokenHandler();
                var tokenValidation = JwtHandler.ValidateToken(
                jwt, 
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config.GetValue<string>("Settings:Token") ?? "")
                    ),
                    ValidateIssuer = true,
                    ValidIssuer = _config.GetValue<string>("Settings:Issuer"),
                    ValidateAudience = true,
                    ValidAudience = _config.GetValue<string>("Settings:Audience"),
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out _);

                var identity = new ClaimsIdentity(tokenValidation.Claims, "jwt");
                var otp = new ClaimsPrincipal(identity);

                return new AuthenticationState(otp);
            }
            catch (SecurityTokenExpiredException ex)
            {
                Console.WriteLine(ex.Expires);
                throw;
            }
        }


        public string generateJwtToken(Users user, string otp, DateTime expires)
        {

            var claims =  new List<Claim>();
            if (otp is not null)
            {
                claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Authentication, otp),
                    new Claim(ClaimTypes.Email, user.Email)
                };
            }
            else
            {
                claims = new List<Claim>{
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
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