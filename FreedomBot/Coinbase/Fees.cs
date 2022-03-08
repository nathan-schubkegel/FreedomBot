using FreedomBot;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Globalization;

namespace FreedomBot.Coinbase;

public interface IFees
{
  Task<FeeResult> GetFees();
}

public class FeeResult
{
  // these are in percent, like 0.002m for 0.2%
  public Decimal MakerFee { get; } // "51.0101984700000000"
  public Decimal TakerFee { get; } // "0.0000000000000000"
  public Decimal MaxFee => Math.Max(MakerFee, TakerFee);
  public Decimal ThirtyDayTrailingVolumeUsd { get; } // "51.0101984700000000"

  [JsonConstructor]
  public FeeResult(string maker_fee_rate, string taker_fee_rate, string usd_volume)
  {
    MakerFee = Decimal.Parse(maker_fee_rate, NumberStyles.Float, CultureInfo.InvariantCulture);
    TakerFee = Decimal.Parse(taker_fee_rate, NumberStyles.Float, CultureInfo.InvariantCulture);
    ThirtyDayTrailingVolumeUsd = Decimal.Parse(usd_volume, NumberStyles.Float, CultureInfo.InvariantCulture);
  }

  public override string ToString() => $"MakerFee={MakerFee.ToString("P2")}, TakerFee={TakerFee.ToString("P2")}";
}

public class Fees : IFees
{
  private readonly IHttpClientSingleton _httpClientSingleton;
  private readonly IApiKeyDataManager _apiKeyManager;
  
  public Fees(IHttpClientSingleton httpClientSingleton, IApiKeyDataManager apiKeyManager)
  {
    _httpClientSingleton = httpClientSingleton;
    _apiKeyManager = apiKeyManager;
  }
  
  public async Task<FeeResult> GetFees()
  {
    var apiKey = await _apiKeyManager.GetData();
    string responseBody = await _httpClientSingleton.SendGetRequest(apiKey, "/fees", "maker/taker fees (percent)");
    var result = JsonConvert.DeserializeObject<FeeResult>(responseBody);
    return result ?? throw new NullReferenceException();
  }
}