using System.Text.Json;

namespace Server.Dto
{    
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Errors { get; set; }
    }
}