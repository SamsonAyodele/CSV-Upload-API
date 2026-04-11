using Microsoft.AspNetCore.Mvc;

public class ResultDto
{
    public int TotalProcessed { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class InventoryFilter
{
    [FromQuery(Name = "name")]
    public string? Name { get; set; }
    [FromQuery(Name = "category")]
    public string? Category { get; set; }
    [FromQuery(Name = "minPrice")]
    public decimal? MinPrice { get; set; }
    [FromQuery(Name = "maxPrice")]
    public decimal? MaxPrice { get; set; }
}