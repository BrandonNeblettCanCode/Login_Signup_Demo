using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;

namespace Frontend.Services
{
    public class JwtTokenService
    {
        private readonly IJSRuntime _js;
        private readonly IConfiguration _config;

        public JwtTokenService(IJSRuntime js, IConfiguration config)
        {
            _js = js;
            _config = config;
        }

        public async ValueTask<string> GetTokenAsync(string tokenName) => await _js.InvokeAsync<string>("sessionStorage.getItem", tokenName);
        public async ValueTask SetTokenAsync(string tokenName, string token) => await _js.InvokeVoidAsync("sessionStorage.setItem", tokenName, token);
        public async ValueTask RemoveToken(string tokenName) => await _js.InvokeVoidAsync("sessionStorage.removeItem", tokenName);

        public async Task<AuthenticationState> DecodeTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var data =  tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config.GetValue<string>("Settings:Token") ?? "")
                    ),
                    ValidateAudience = true,
                    ValidAudience = _config.GetValue<string>("Settings:Audience"),
                    ValidateIssuer = true,
                    ValidIssuer = _config.GetValue<string>("Settings:Issuer"),
                    RequireAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero

                },
            out _);
            var identity = new ClaimsIdentity(data.Claims);
            var decodedData = new ClaimsPrincipal(identity);

            return new AuthenticationState(decodedData);

        }
    }
}