using InventoryApi.Models;
using InventoryApi.Models.Common;

namespace InventoryApi.Services.Interfaces;

public interface IInventoryService
{
    Task<(int success, List<string> errors)> UploadCsvAsync(Stream stream);
    Task<PagedResponse<Inventory>> GetInventoryAsync(int page, int size, InventoryFilter? filter);
    Task<Inventory> GetSingleInventoryAsync(int id);
}