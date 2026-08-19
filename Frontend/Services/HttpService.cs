using System.Net;
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
                var response = await request.Content.ReadFromJsonAsync<ApiResponse>();
                    Console.WriteLine(response.Message);

                if (response?.Success != true)
                {
                    throw new Exception(response.Message);
                }
                else
                {
                    return response.Message!;
                }
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
                {
                    switch(request.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                        {
                            throw new Exception("'Username' already taken");
                        }
                        case HttpStatusCode.Conflict:
                        {
                            throw new Exception("'Email' aleady taken");
                        }
                    }
                }

                var result = await request.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message);
            }
        }

        public async Task<string> EmailAsync(OtpDto dto)
        {
            try
            {
                var request = await _http.PostAsJsonAsync("emailVerify", dto);
                if(!request.IsSuccessStatusCode)
                {
                    switch(request.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                        {
                            throw new Exception("Incorrect 'Email Address'");
                        }
                    }
                }

                var result = await request.Content.ReadAsStringAsync();
                return result;
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