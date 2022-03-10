using FreedomBot;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Globalization;

namespace FreedomBot.Coinbase;

public interface IProductTicker
{
  Task<ProductTickerResult> GetTicker(string coinType);
}
 
/* It typically looks like
{
  "trade_id": 86326522,
  "price": "6268.48",
  "size": "0.00698254",
  "time": "2020-03-20T00:22:57.833897Z",
  "bid": "6265.15",
  "ask": "6267.71",
  "volume": "53602.03940154"
} */
public class ProductTickerResult
{
  public Decimal LastTradePrice { get; }
  public Decimal BestBid { get; }
  public Decimal BestAsk { get; }
  public Decimal Volume24h{ get; }

  [JsonConstructor]
  public ProductTickerResult(string price, string bid, string ask, string volume)
  {
    LastTradePrice = Decimal.Parse(price, NumberStyles.Float, CultureInfo.InvariantCulture);
    BestBid = Decimal.Parse(bid, NumberStyles.Float, CultureInfo.InvariantCulture);
    BestAsk = Decimal.Parse(ask, NumberStyles.Float, CultureInfo.InvariantCulture);
    Volume24h = Decimal.Parse(volume, NumberStyles.Float, CultureInfo.InvariantCulture);
  }
  
  public override string ToString()
  {
    return $"LastTradePrice={LastTradePrice}, BestBid={BestBid}, BestAsk={BestAsk}, Volume24h={Volume24h}";
  }
}

public class ProductTicker : IProductTicker
{
  private readonly IHttpClientSingleton _httpClientSingleton;
  private readonly IApiKeyDataManager _apiKeyManager;
  
  public ProductTicker(IHttpClientSingleton httpClientSingleton, IApiKeyDataManager apiKeyManager)
  {
    _httpClientSingleton = httpClientSingleton;
    _apiKeyManager = apiKeyManager;
  }

  public async Task<ProductTickerResult> GetTicker(string coinType)
  {
    var apiKey = await _apiKeyManager.GetData();
    string responseBody = await _httpClientSingleton.SendGetRequest(apiKey, $"/products/{coinType}-USD/ticker", $"ticker (current price) for {coinType}");
    var result = JsonConvert.DeserializeObject<ProductTickerResult>(responseBody);
    return result ?? throw new NullReferenceException();
  }
}