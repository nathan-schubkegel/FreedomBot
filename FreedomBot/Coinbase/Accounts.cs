using FreedomBot;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Globalization;

namespace FreedomBot.Coinbase;

public interface IAccounts
{
  Task<List<Account>> GetAccounts();
}

public class Account
{
  public string Id { get; } // usually a guid
  public string Currency { get; } // "USD", "ETH", whatever
  public Decimal Balance { get; } // "51.0101984700000000"
  public Decimal Hold { get; } // "0.0000000000000000"
  public Decimal Available { get; } // "51.0101984700000000"
  public string ProfileId { get; } // usually a guid
  public bool TradingEnabled { get; }

  [JsonConstructor]
  public Account(string id, string currency, string balance, string hold, string available, string profile_id, bool trading_enabled)
  {
    Id = id ?? throw new ArgumentNullException(nameof(id));
    Currency = currency ?? throw new ArgumentNullException(nameof(id));
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
      return $"{Currency}: 0";
    }
    else
    {
      var percent = Available / Balance;
      return $"{Currency}: {Balance.ToString("G29")} ({percent.ToString("P0")} available)";
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

    List<Account>? result = null;
    await _httpClientSingleton.UseAsync("fetching accounts (how much money/coins we hold)", async http => 
    {
      var request = apiKey.MakeRequest(HttpMethod.Get, "/accounts");
      var response = await http.SendAsync(request);
      string responseBody = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"coinbase pro api for accounts (how much money/coins we hold) returned {response.StatusCode}: {responseBody}");
      }
      result = JsonConvert.DeserializeObject<List<Account>>(responseBody);
    });
    
    return result ?? throw new NullReferenceException();
  }
}