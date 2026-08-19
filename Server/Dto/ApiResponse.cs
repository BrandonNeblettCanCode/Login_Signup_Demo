namespace Server.Dto
{    
    public class ApiResponse
    {
        public bool Success { get; set; }
        public dynamic? Message { get; set; }
        public string? Errors { get; set; }
    }
}