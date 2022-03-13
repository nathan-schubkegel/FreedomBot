#nullable disable

using Ninject;
using FreedomBot;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace FreedomBot.ManualTests;

public class Program
{
  public static StandardKernel _kernel;
  
  public static async Task<int> Main(string[] args)
  {
    try
    {
      // Make it so ToString() and TryParse() never need to be given CultureInfo.InvariantCulture
      CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
      CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

      _kernel = FreedomBot.Program.CreateServiceProvider(new ProgramArgs(new string[0]));

      // get api key at the start of the test, so that the first thing the test always does is ask for password
      var apiKeyData = await _kernel.Get<IApiKeyDataManager>().GetData();

      await TestProducts();
      await TestAccounts();
      await TestOracle();
      await TestProductTicker();
      await TestFees();
      Console.WriteLine("All manual tests passed!");
      return 0;
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine("Unhandled program-level " + ex.ToString());
      return 1;
    }
  }
  
  public static async Task TestProducts()
  {
    var products = await _kernel.Get<Coinbase.IProducts>().GetCoinTypesTradableForUsd();
    Console.WriteLine(string.Join(" ", products));
    Assert.Contains("AVAX", products);
    Assert.Contains("BTC", products);
    Assert.Contains("ETH", products);
  }
  
  public static async Task TestAccounts()
  {
    var accounts = await _kernel.Get<Coinbase.IAccounts>().GetAccounts();
    Console.WriteLine(string.Join("\n", accounts.Where(a => a.Balance > 0m)));
    Assert.Single(accounts, a => a.CoinType == "ETH");
    
    var eth = accounts.Single(a => a.CoinType == "ETH");
    Assert.InRange(eth.Balance, 0m, Decimal.MaxValue);
    Assert.InRange(eth.Hold, 0m, Decimal.MaxValue);
    Assert.InRange(eth.Available, 0m, Decimal.MaxValue);
    Assert.Equal(eth.Balance, eth.Hold + eth.Available);
  }
  
  public static async Task TestOracle()
  {
    var prices = await _kernel.Get<Coinbase.IOracle>().GetSignedPrices();
    Console.WriteLine(string.Join("\n", prices.Prices.Select(p => $"{p.Key} = {p.Value}")));
    var ethPrice = prices.Prices["ETH"];
    Assert.InRange(ethPrice, 0m, Decimal.MaxValue);
  }
  
  public static async Task TestProductTicker()
  {
    var ticker = await _kernel.Get<Coinbase.IProductTicker>().GetTicker("AVAX");
    Console.WriteLine(ticker);
    Assert.InRange(ticker.LastTradePrice, 0m, Decimal.MaxValue);
    Assert.InRange(ticker.BestBid, 0m, Decimal.MaxValue);
    Assert.InRange(ticker.BestAsk, 0m, Decimal.MaxValue);
    Assert.InRange(ticker.Volume24h, 0m, Decimal.MaxValue);
  }
  
  public static async Task TestFees()
  {
    var fees = await _kernel.Get<Coinbase.IFees>().GetFees();
    Console.WriteLine(fees);
    Assert.InRange(fees.MakerFee, 0.0001m, 1m);
    Assert.InRange(fees.TakerFee, 0.0001m, 1m);
    Assert.Equal(Math.Max(fees.MakerFee, fees.TakerFee), fees.MaxFee);
    Assert.InRange(fees.ThirtyDayTrailingVolumeUsd, 0m, Decimal.MaxValue);
  }
}