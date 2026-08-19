namespace Frontend.Dtos;

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}