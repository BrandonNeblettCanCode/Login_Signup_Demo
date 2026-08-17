using Microsoft.JSInterop;

namespace Frontend.Services
{
    public class JwtTokenService
    {
        private readonly IJSRuntime _js;

        public JwtTokenService(IJSRuntime js)
        {
            _js = js;
        }

        public async ValueTask<string> GetTokenAsync(string tokenName) => await _js.InvokeAsync<string>("sessionStorage.getItem", tokenName);
        public async ValueTask SetTokenAsync(string tokenName, string token) => await _js.InvokeVoidAsync("sessionStorage.setItem", tokenName, token);
        public async ValueTask RemoveToken(string tokenName) => await _js.InvokeVoidAsync("sessionStorage.removeItem", tokenName);
    }
}