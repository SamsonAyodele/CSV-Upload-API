using CsvHelper;
using System.Globalization;
using InventoryApi.Models;
using InventoryApi.Data.Interfaces;
using InventoryApi.Services.Interfaces;
using InventoryApi.Models.Common;


namespace InventoryApi.Services;

public class InventoryService : IInventoryService
{
    private readonly IDbHelper _db;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IDbHelper db, ILogger<InventoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(int success, List<string> errors)> UploadCsvAsync(Stream stream)
    {
        int success = 0;
        var errors = new List<string>();

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var rows = csv.GetRecords<dynamic>();

        var inventoryList = new List<Inventory>();
        foreach (var row in rows)
        {
            try
            {
                if (!int.TryParse(row.Id?.ToString(), out int id))
                {
                    errors.Add($"Row {row?.Id ?? "Unknown"}: Invalid numbers");
                    continue;
                }

                if (!decimal.TryParse(row.Price?.ToString(), out decimal price))
                {
                    errors.Add($"Row {row?.Id ?? "Unknown"}: Invalid price");
                    continue;
                }

                if (!int.TryParse(row.StockQuantity?.ToString(), out int stock))
                {
                    errors.Add($"Row {row?.Id ?? "Unknown"}: Invalid stock quantity");
                    continue;
                }

                if (price <= 0 || stock <= 0)
                {
                    errors.Add($"Row {row?.Id ?? "Unknown"}: Invalid values");
                    continue;
                }

                string name = row.Name?.ToString()?.Trim() ?? "";
                string category = row.Category?.ToString()?.Trim() ?? "";

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(category))
                {
                    errors.Add($"Row {row?.Id ?? "Unknown"}: Missing fields");
                    continue;
                }

                inventoryList.Add(new Inventory
                {
                    Id = id,
                    Name = name,
                    Category = category,
                    Price = price,
                    StockQuantity = stock
                });

                success++;
            }
            catch (Exception ex)
            {
                string rowId = row?.Id?.ToString() ?? "Unknown";
                _logger.LogError(ex, "An error occurred while processing row {RowId}", rowId);
                errors.Add($"Row {rowId}: {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            return (0, errors);
        }

        await _db.BulkUpsertAsync(inventoryList);
        // ServiceUtil.WriteToFile("Bulk upsert completed");

        return (success, errors);
    }

    public async Task<PagedResponse<Inventory>> GetInventoryAsync(int page, int size, InventoryFilter? filter)
    {
        return await _db.GetInventoryAsync(page, size, filter);
    }

    public async Task<Inventory> GetSingleInventoryAsync(int id)
    {
        return await _db.GetSingleInventoryAsync(id);
    }
}