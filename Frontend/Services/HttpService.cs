using System.Net;
using System.Net.Http.Json;
using Azure.Core;
using Frontend.Dtos;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
                var request = await _http.PostAsJsonAsync("sendotp", otpDto.Otp);
                var response = await request.Content.ReadFromJsonAsync<ApiResponse>();

                if(response is null)
                    throw new Exception("Something went wrong, please try again later");
                if(response.Success != true)
                    throw new Exception(response.Errors);
                else
                {
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