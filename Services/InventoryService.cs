using CsvHelper;
using System.Globalization;
using InventoryApi.Models;
using InventoryApi.Data.Interfaces;
using InventoryApi.Services.Interfaces;
using InventoryApi.Models.Common;
using Microsoft.AspNetCore.Http.Features;


namespace InventoryApi.Services;

public class InventoryService : IInventoryService
{
    private readonly IDbHelper _db;
    private readonly ILogger<InventoryService> _logger;
    private readonly ICurrencyService _currencyService;

    public InventoryService(IDbHelper db, ILogger<InventoryService> logger, ICurrencyService currencyService)
    {
        _db = db;
        _logger = logger;
        _currencyService = currencyService;
    }

    public async Task<(int success, List<string> errors)> UploadCsvAsync(Stream stream)
    {
        int success = 0;
        var errors = new List<string>();

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var rows = csv.GetRecords<dynamic>();

        var inventoryList = new List<Inventory>();
        var currencies = new HashSet<string>();

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
                string currency = row.Currency?.ToString() ?? "USD";

                currencies.Add(currency);

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
                    StockQuantity = stock,
                    Currency = currency
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

        var rates = await _currencyService.GetRatesToUsdAsync(currencies);
        foreach (var item in inventoryList)
        {
            if (item.Currency == "USD")
            {
                continue;
            }
            if (string.IsNullOrEmpty(item.Currency) || !rates.TryGetValue(item.Currency, out var rate))
            {
                errors.Add($"Unsupported currency: {item.Currency}");
                continue;
            }

            item.Price = Math.Round(item.Price * rate, 2);
            item.Currency = null; // Clear currency since it's now in USD
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