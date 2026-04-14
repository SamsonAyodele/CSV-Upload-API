namespace InventoryApi.Services.Interfaces;

public interface ICurrencyService
{
    Task<Dictionary<string, decimal>> GetRatesToUsdAsync(IEnumerable<string> currencies);
}