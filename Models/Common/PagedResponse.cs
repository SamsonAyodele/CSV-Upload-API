namespace InventoryApi.Models.Common;

public class PagedResponse<T>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / Size);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
    public List<T> Items { get; set; } = new();
}