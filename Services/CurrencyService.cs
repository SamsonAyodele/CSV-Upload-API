using System.Net.Http.Json;
using InventoryApi.Models.Common;
using InventoryApi.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Serilog;

namespace InventoryApi.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _http;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(HttpClient http, ILogger<CurrencyService> logger)
    {
        _http = http;
        _logger = logger;
    }
    // This method fetches exchange rates for the specified currencies to EUR using the Frankfurter API
    // It returns a dictionary where the key is the currency code and the value is the exchange rate to EUR
    // IEnumerable<string> currencies: A collection of currency codes for which exchange rates to EUR are needed.
    // If a currency is "EUR", it directly assigns an exchange rate of 1. For other currencies, it makes an API call to fetch the exchange rate.
    // If the API call fails for any currency, it logs the error and continues processing the remaining currencies without throwing an exception.
    //
    public async Task<Dictionary<string, decimal>> GetRatesToEurAsync(string currency)
    {
        var result = new Dictionary<string, decimal>();

       // foreach (var currency in currencies.Distinct())
        {


            try
            {
                var url = $"https://api.frankfurter.app/latest?from={currency}&to=EUR";

                var response = await _http.GetFromJsonAsync<FrankfurterResponse>(url);

                if (response?.Rates != null && response.Rates.ContainsKey("EUR"))
                {
                    result[currency] = response.Rates["EUR"];
                }
            }
            catch (Exception ex)
            {
                // Log the error and continue with other currencies
                _logger.LogError(ex, $"Failed to get exchange rate for {currency}");
                ServiceUtil.WriteToFile("Failed to get exchange rate for " + currency);

            }
        }

        return result;
    }
}

public class FrankfurterResponse
{
    public Dictionary<string, decimal> Rates { get; set; } = new();
}