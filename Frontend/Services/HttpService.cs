using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Azure.Core;
using Frontend.Dtos;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;

namespace Frontend.Services
{
    public class HttpService
    {
        private readonly HttpClient _http;
        private readonly JwtAuthStateProvider _jwtAuthStateProvider;

        private readonly JwtTokenService _jwtTokenService;

        public HttpService(HttpClient http, JwtAuthStateProvider jwtAuthStateProvider, JwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
            _http = http;
            _jwtAuthStateProvider = jwtAuthStateProvider;
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            try
            {
                var request = await _http.PostAsJsonAsync("login", dto);
                var response = await request.Content.ReadFromJsonAsync<ApiResponse>();

                if (response is null)
                    throw new Exception("Something went wrong, please try again later");
                else if (response is not null && response?.Success != true)
                    throw new Exception(response.Errors);
                else
                {
                    return response!.Message!;
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
                var response = await request.Content.ReadFromJsonAsync<ApiResponse>();

                if (response is null)
                    throw new Exception("Something went wrong, please try again later");
                else if (response is not null && response?.Success != true)
                    throw new Exception(response.Errors);
                else
                {
                    return response!.Message!;
                }
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message);
            }
        }

        public async Task<TokenCheckResponse> CheckForToken(string tokenName)
        {
            try
            {
                var token = await _jwtTokenService.GetTokenAsync(tokenName);
                if(token is null || string.IsNullOrWhiteSpace(token))
                {
                    return new TokenCheckResponse()
                    {
                        Username = "",
                        Status = false
                    };
                }
                
                var decodedData = await _jwtTokenService.DecodeTokenAsync(token);
                if(decodedData is null)
                    throw new Exception("Something went wrong");

                var data = decodedData.User;

                return new TokenCheckResponse
                {
                    Id = Guid.Parse(data.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                    Username = data.FindFirst(ClaimTypes.Name)!.Value,
                    Email = data.FindFirst(ClaimTypes.Email)!.Value,
                    Status = true
                };
            }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new TokenCheckResponse { Id = Guid.Empty, Username = null!, Email = null!, Status = false };
        }
        }

        public async Task<ApiResponse> EmailAsync(OtpDto dto)
        {
            try
            {
                var request = await _http.PostAsJsonAsync("emailVerify", dto);
                var response = await request.Content.ReadFromJsonAsync<ApiResponse>();

                if(response is null)
                    throw new Exception("Something went wrong, please try again later");

                if(response.Success != true)
                    throw new Exception(response.Errors);

                await _jwtTokenService.SetTokenAsync("OTP", response.Message!);
                return response;
            }
            catch (Exception Ex)
            {   
                throw new Exception(Ex.Message);
            }
        }

        public async Task<ApiResponse> SendOtpAsync(OtpDto otpDto)
        {
            try
            {
                var request = await _http.PostAsJsonAsync("otp", otpDto.Otp);
                var response = await request.Content.ReadFromJsonAsync<ApiResponse>();

                if(response is null)
                    throw new Exception("Something went wrong, please try again later");
                if(response.Success != true)
                    throw new Exception(response.Errors);
                else
                {
                    _jwtTokenService.RemoveToken("OTP");
                    _jwtTokenService.SetTokenAsync("OTP_Verified", response.Message);
                    return response;
                }
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