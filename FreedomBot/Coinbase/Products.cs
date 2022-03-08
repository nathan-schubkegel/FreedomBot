using FreedomBot;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FreedomBot.Coinbase;

public interface IProducts
{
  Task<List<string>> GetCoinTypesTradableForUsd();
}

public class Products : IProducts
{
  private readonly IHttpClientSingleton _httpClientSingleton;
  private readonly IApiKeyDataManager _apiKeyManager;

  public Products(IHttpClientSingleton httpClientSingleton, IApiKeyDataManager apiKeyManager)
  {
    _httpClientSingleton = httpClientSingleton;
    _apiKeyManager = apiKeyManager;
  }

  public async Task<List<string>> GetCoinTypesTradableForUsd()
  {
    var apiKey = await _apiKeyManager.GetData();
    string responseBody = await _httpClientSingleton.SendGetRequest(apiKey, "/products", "products (tradable coins)");
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
    return results.OrderBy(x => x).ToList();
  }
}