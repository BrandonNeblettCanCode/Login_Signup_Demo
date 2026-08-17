using System.Net.Http.Json;
using Frontend.Dtos;

namespace Frontend.Services
{
    public class HttpService
    {
        private readonly HttpClient _http;
        private readonly JwtAuthStateProvider _jwtAuthStateProvider;

        public HttpService(HttpClient http, JwtAuthStateProvider jwtAuthStateProvider)
        {
            _http = http;
            _jwtAuthStateProvider = jwtAuthStateProvider;
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            try
            {
                var request = await _http.PostAsJsonAsync("login", dto);
                if(!request.IsSuccessStatusCode)
                    throw new Exception(await request.Content.ReadAsStringAsync());

                var result = await request.Content.ReadAsStringAsync();
                
                await _jwtAuthStateProvider.MarkUserAsAuthenticatedAsync(result);
                return "Login Successfull";
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message);
            }
        }

        public async Task<string> SignUpAsync(SignUpDto dto)
        {
            try
            {
                var request = await _http.PostAsJsonAsync("signup", dto);
                if(!request.IsSuccessStatusCode)
                    throw new Exception(await request.Content.ReadAsStringAsync());

                var result = await request.Content.ReadAsStringAsync();
                
                await _jwtAuthStateProvider.MarkUserAsAuthenticatedAsync(result);
                return "Sign Up Successfull";
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message);
            }
        }

        public async void Logout()
        {
            await _jwtAuthStateProvider.Logout();
        }
    }
}