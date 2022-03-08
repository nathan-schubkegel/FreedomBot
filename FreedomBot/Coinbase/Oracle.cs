using FreedomBot;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Globalization;

namespace FreedomBot.Coinbase;

public interface IOracle
{
  Task<OracleResult> GetSignedPrices();
}
 
/* It typically looks like
{
  ... some stuff I don't understand ...
  "prices": {
    "BTC": "38167.295",
    "ETH": "2538.4700000000003",
    "XTZ": "2.96",
    "DAI": "0.999776",
    "REP": "13.32",
    "ZRX": "0.47214449999999997",
    "BAT": "0.635722",
    "KNC": "2.3252",
    "LINK": "13.02",
    "COMP": "100.985",
    "UNI": "8.31",
    "GRT": "0.32184999999999997",
    "SNX": "3.5300000000000002"
  }
} */
public class OracleResult
{
  public Dictionary<string, Decimal> Prices { get; }

  [JsonConstructor]
  public OracleResult(Dictionary<string, string> prices)
  {
    Prices = new Dictionary<string, Decimal>();
    foreach (var kvp in prices)
    {
      Prices[kvp.Key] = Decimal.Parse(kvp.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
  }
}

public class Oracle : IOracle
{
  private readonly IHttpClientSingleton _httpClientSingleton;
  private readonly IApiKeyDataManager _apiKeyManager;
  
  public Oracle(IHttpClientSingleton httpClientSingleton, IApiKeyDataManager apiKeyManager)
  {
    _httpClientSingleton = httpClientSingleton;
    _apiKeyManager = apiKeyManager;
  }

  public async Task<OracleResult> GetSignedPrices()
  {
    var apiKey = await _apiKeyManager.GetData();
    string responseBody = await _httpClientSingleton.SendGetRequest(apiKey, "/oracle", "signed prices from oracle");
    var result = JsonConvert.DeserializeObject<OracleResult>(responseBody);
    return result ?? throw new NullReferenceException();
  }
}