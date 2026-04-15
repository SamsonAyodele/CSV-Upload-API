using CsvHelper;
using System.Globalization;
using InventoryApi.Models;
using InventoryApi.Data.Interfaces;
using InventoryApi.Services.Interfaces;
using InventoryApi.Models.Common;
using Microsoft.AspNetCore.Http.Features;
using System.Data;


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
                    StockQuantity = stock,
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
        // Created a HashSet to store unique currency codes from the inventory items. 
        // This ensures that we only fetch exchange rates for distinct currencies, avoiding duplicate API calls.
        // optimizing the number of API calls.
        //var currencies = new HashSet<string>();
        var pagedRows = await _db.GetInventoryAsync(page, size, filter);
        var rows = pagedRows.Items;
        // foreach (var row in rows)
        // {
        //     currencies.Add(row.Currency ?? "USD");
        // }
        var currency = "USD";

        var rates = await _currencyService.GetRatesToEurAsync(currency);

        foreach (var row in rows)
        {
            // row.Currency ??= "USD";
            // currencies.Add(row.Currency);
            if (string.IsNullOrEmpty(row.Currency) || !rates.TryGetValue(row.Currency, out var rate))
            {
                continue;
            }
            row.Price = Math.Round(row.Price * rate, 2);
            row.Currency = "EUR";
        }

        return pagedRows;
    }

    public async Task<Inventory> GetSingleInventoryAsync(int id)
    {
        return await _db.GetSingleInventoryAsync(id);
    }
}