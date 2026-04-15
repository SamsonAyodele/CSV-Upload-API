namespace InventoryApi.Services.Interfaces;

public interface ICurrencyService
{
    Task<Dictionary<string, decimal>> GetRatesToEurAsync(string currencies);
}