namespace InventoryApi.Models.Common;

public class ResponseMeta
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public class BulkUploadResponse
{
    public int TotalProcessed { get; set; }
}