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
    
    byte[] base64decodedSecretKey = Convert.FromBase64String(apiKey.ApiSecret);
    var timestamp = ((long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds).ToString();
    var method = "GET";
    var requestPath = "/accounts";
    var body = string.Empty;
    byte[] prehash = Encoding.UTF8.GetBytes(timestamp + method + requestPath + body);
    var hasher = new HMACSHA256(base64decodedSecretKey);
    byte[] hash = hasher.ComputeHash(prehash);
    var base64hash = Convert.ToBase64String(hash);

    var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.exchange.coinbase.com/accounts");
    request.Headers.Add("Accept", "application/json");
    request.Headers.Add("User-Agent", HttpClientSingleton.UserAgent);
    request.Headers.Add("CB-ACCESS-KEY", apiKey.Id);
    request.Headers.Add("CB-ACCESS-SIGN", base64hash);
    request.Headers.Add("CB-ACCESS-TIMESTAMP", timestamp);
    request.Headers.Add("CB-ACCESS-PASSPHRASE", apiKey.Passphrase);
    List<Account>? result = null;
    await _httpClientSingleton.UseAsync("fetching all trading accounts", async http => 
    {
      var response = await http.SendAsync(request);
      string responseBody = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"coinbase pro api for all trading accounts returned {response.StatusCode}: {responseBody}");
      }
      result = JsonConvert.DeserializeObject<List<Account>>(responseBody);
    });
    
    return result ?? throw new NullReferenceException();
  }
}