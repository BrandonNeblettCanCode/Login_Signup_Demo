using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Frontend.Services
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _annonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private readonly JwtTokenService _jwtTokenService;

        public JwtAuthStateProvider(JwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _jwtTokenService.GetTokenAsync("AT");
                if(token is null)
                    return new AuthenticationState(_annonymous);

                var tokenHandler = new JwtSecurityTokenHandler();
                var data = tokenHandler.ReadJwtToken(token);

                var identity = new ClaimsIdentity(data.Claims, "jwt");
                if(!identity.IsAuthenticated)
                    return new AuthenticationState(_annonymous);

                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message);
            }
        }

        public async Task MarkUserAsAuthenticatedAsync(string token)
        {
            await _jwtTokenService.SetTokenAsync("AT", token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Logout()
        {
            await _jwtTokenService.RemoveToken("AT");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

    }
}