using FreedomBot;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Globalization;

namespace FreedomBot.Coinbase;

public interface ICreateOrder
{
  Task<CreatedOrder> MarketBuy(string coinType, Decimal usd);
  Task<CreatedOrder> MarketSell(string coinType, Decimal coinCount);
}

public class CreatedOrder
{
/* Many fields are available
{
  "id": "a9625b04-fc66-4999-a876-543c3684d702",
  "price": "10.00000000",
  "size": "1.00000000",
  "product_id": "BTC-USD",
  "profile_id": "8058d771-2d88-4f0f-ab6e-299c153d4308",
  "side": "buy",
  "type": "limit",
  "time_in_force": "GTC",
  "post_only": true,
  "created_at": "2020-03-11T20:48:46.622052Z",
  "fill_fees": "0.0000000000000000",
  "filled_size": "0.00000000",
  "executed_value": "0.0000000000000000",
  "status": "open",
  "settled": false
}
*/
  public string OrderId { get; }
  public JObject Fields { get; }

  public CreatedOrder(JObject fields)
  {
    Fields = fields ?? throw new ArgumentNullException(nameof(fields));
    OrderId = fields["id"]?.Value<string>() ?? throw new ArgumentNullException("fields.id");
  }

  public override string ToString() => Fields.ToString();
}

public class CreateOrder : ICreateOrder
{
  private readonly IHttpClientSingleton _httpClientSingleton;
  private readonly IApiKeyDataManager _apiKeyManager;

  public CreateOrder(IHttpClientSingleton httpClientSingleton, IApiKeyDataManager apiKeyManager)
  {
    _httpClientSingleton = httpClientSingleton;
    _apiKeyManager = apiKeyManager;
  }

  public async Task<CreatedOrder> MarketBuy(string coinType, Decimal usd)
  {
    // TODO: make sure 'usd' is $X.XX and not fractional
    
    var apiKey = await _apiKeyManager.GetData();
    string responseBody = await _httpClientSingleton.SendPostRequest(apiKey, "/orders", $"market buy order for {usd.ToString("c", CultureInfo.InvariantCulture)} {coinType}",
      new JObject()
      {
        ["profile_id"] = apiKey.Id,
        ["type"] = "market",
        ["side"] = "buy",
        ["product_id"] = $"{coinType}-USD",
        ["funds"] = usd,
      });

    var fields = JObject.Parse(responseBody);
    return new CreatedOrder(fields) ?? throw new NullReferenceException();
  }
  
  public async Task<CreatedOrder> MarketSell(string coinType, Decimal coinCount)
  {
    // TODO: get the base_min_size, base_max_size, and base_increment of the product_id
    // and make sure 'coinCount' adheres
    
    var apiKey = await _apiKeyManager.GetData();
    string responseBody = await _httpClientSingleton.SendPostRequest(apiKey, "/orders", $"market sell order for {coinCount} {coinType}",
      new JObject()
      {
        ["profile_id"] = apiKey.Id,
        ["type"] = "market",
        ["side"] = "sell",
        ["product_id"] = $"{coinType}-USD",
        ["size"] = coinCount,
      });

    var fields = JObject.Parse(responseBody);
    return new CreatedOrder(fields) ?? throw new NullReferenceException();
  }
}