using InventoryApi.Models;
using InventoryApi.Models.Common;

namespace InventoryApi.Data.Interfaces;

public interface IDbHelper
{
    Task BulkUpsertAsync(List<Inventory> items);
    Task<PagedResponse<Inventory>> GetInventoryAsync(int page, int size, InventoryFilter? filter);
    // Task BulkUpsertAsync(List<Models.Inventory> items);
}