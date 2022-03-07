using FreedomBot;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FreedomBot.Coinbase;

public interface IProducts
{
  Task<IReadOnlyList<string>> GetCoinTypesTradableForUsd();
}

public class Products : IProducts
{
  private readonly IHttpClientSingleton _httpClientSingleton;
  private List<string>? _coinTypesTradableForUsd;
  
  public Products(IHttpClientSingleton httpClientSingleton)
  {
    _httpClientSingleton = httpClientSingleton;
  }
  
  public async Task<IReadOnlyList<string>> GetCoinTypesTradableForUsd()
  {
    if (_coinTypesTradableForUsd != null) return _coinTypesTradableForUsd;

    var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.exchange.coinbase.com/products");
    request.Headers.Add("Accept", "application/json");
    request.Headers.Add("User-Agent", HttpClientSingleton.UserAgent);
    await _httpClientSingleton.UseAsync("fetching coin types tradable for USD", async http => 
    {
      var response = await http.SendAsync(request);
      string responseBody = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"coinbase pro api for all known trading pairs returned {response.StatusCode}: {responseBody}");
      }

      var jArray = JArray.Parse(responseBody);

      // look for these
      //  "base_currency": "BTC",
      //  "quote_currency": "USD",

      var results = new HashSet<string>();
      foreach (JObject o in jArray) {
        if (o["quote_currency"]?.Value<string>() == "USD") {
          results.Add(o["base_currency"]?.Value<string>() ?? throw new Exception("unexpected null base_currency"));
        }
      }
      _coinTypesTradableForUsd = results.OrderBy(x => x).ToList();
    });
    
    return _coinTypesTradableForUsd!;
  }
}