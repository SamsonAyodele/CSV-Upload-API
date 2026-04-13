namespace InventoryApi.Models.Common;

public class ApiResponse<T>
{
    public string Message { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public int StatusCode { get; set; }
    public List<string>? Errors { get; set; } // for error details
    public T? Data { get; set; } // actual payload
}