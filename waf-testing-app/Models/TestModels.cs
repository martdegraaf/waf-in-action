namespace WafTestingApp.Models;

public class TestRequest
{
    public string? UserId { get; set; }
    public string? Comment { get; set; }
    public string? FilePath { get; set; }
    public string? Data { get; set; }
    public Dictionary<string, string>? CustomHeaders { get; set; }
}

public class TestResponse
{
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
