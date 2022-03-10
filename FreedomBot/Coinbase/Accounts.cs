using FreedomBot;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Globalization;

namespace FreedomBot.Coinbase;

// Accounts are how much money/coins you have on the coinbase pro exchange.
// There's one for each tradable coin type.
public interface IAccounts
{
  Task<List<Account>> GetAccounts();
}

public class Account
{
  public string Id { get; } // usually a guid
  public string CoinType { get; } // "USD", "ETH", whatever
  public Decimal Balance { get; } // "51.0101984700000000"
  public Decimal Hold { get; } // "0.0000000000000000"
  public Decimal Available { get; } // "51.0101984700000000"
  public string ProfileId { get; } // usually a guid
  public bool TradingEnabled { get; }

  [JsonConstructor]
  public Account(string id, string currency, string balance, string hold, string available, string profile_id, bool trading_enabled)
  {
    Id = id ?? throw new ArgumentNullException(nameof(id));
    CoinType = currency ?? throw new ArgumentNullException(nameof(id));
    Balance = Decimal.Parse(balance, NumberStyles.Float, CultureInfo.InvariantCulture);
    Hold = Decimal.Parse(hold, NumberStyles.Float, CultureInfo.InvariantCulture);
    Available = Decimal.Parse(available, NumberStyles.Float, CultureInfo.InvariantCulture);
    ProfileId = profile_id ?? throw new ArgumentNullException(nameof(profile_id));
    TradingEnabled = trading_enabled;
  }

  public override string ToString()
  {
    if (Balance == 0m)
    {
      return $"{CoinType}: 0";
    }
    else
    {
      var percent = Available / Balance;
      return $"{CoinType}: {Balance.ToString("G29")} ({percent.ToString("P0")} available)";
    }
  }
}

public class Accounts : IAccounts
{
  private readonly IHttpClientSingleton _httpClientSingleton;
  private readonly IApiKeyDataManager _apiKeyManager;
  
  public Accounts(IHttpClientSingleton httpClientSingleton, IApiKeyDataManager apiKeyManager)
  {
    _httpClientSingleton = httpClientSingleton;
    _apiKeyManager = apiKeyManager;
  }
  
  public async Task<List<Account>> GetAccounts()
  {
    var apiKey = await _apiKeyManager.GetData();
    string responseBody = await _httpClientSingleton.SendGetRequest(apiKey, "/accounts", "accounts (how much money/coins we hold)");
    var result = JsonConvert.DeserializeObject<List<Account>>(responseBody);
    return result ?? throw new NullReferenceException();
  }
}