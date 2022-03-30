using System.Globalization;

namespace FreedomBot.Programs;

// Program that buys a quantity of coins when the average price over 1 minute meets some conditions.
//  - Upper Limit (provided by user) - if the price is at/above this point, then do not buy.
//  - First Target (provided by user) - once the price drops to this point, then buy when
//                                      the price rises to [Minimum Price Observed plus Buyback Range] or greater.
//  - Buyback Range (provided by user)
// Examples: with upper limit = $0.45, price starting at $0.40, first target = $0.35, Buyback Range = $0.03
//   - whle price is at/above $0.45, this program does not buy
//   - until price drops to $0.35, this program does not buy
//   - if price drops to $0.33 then rises, this program does not buy
//   - if price drops to $0.32 then rises, this program buys at $0.35
//   - if price drops to $0.30 then rises, this program buys at $0.33
//   - if price drops to $0.20 then rises, this program buys at $0.23

public class FallingRecoveryLimitBuy
{
  private readonly Coinbase.IAccounts _accounts;
  private readonly Coinbase.IProductTicker _productTicker;
  private readonly Coinbase.ICreateOrder _createOrder;
  private readonly ProgramArgs _programArgs;
  
  public FallingRecoveryLimitBuy(Coinbase.IAccounts accounts, Coinbase.IProductTicker productTicker,
    ProgramArgs programArgs, Coinbase.ICreateOrder createOrder)
  {
    _accounts = accounts;
    _productTicker = productTicker;
    _programArgs = programArgs;
    _createOrder = createOrder;
  }
  
  public async Task Run()
  {
    if (_programArgs.Length <= 1) throw new Exception("missing 2nd arg; expected coin type, such as \"ETH\"");
    var coinType = _programArgs[1];

    Decimal? usdToSpend = null;
    if (_programArgs.Length <= 2) throw new Exception("missing 3rd arg; expected USD to spend, such as \"5\" or \"3.15\" or \"all\"");
    if (_programArgs[2].ToLowerInvariant() != "all")
    {
      if (!Decimal.TryParse(_programArgs[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var hardUsdToSpend))
      {
        throw new Exception($"invalid 3rd arg; expected USD to spend, such as \"5\" or \"0.15\" or \"all\"");
      }
      usdToSpend = hardUsdToSpend;
    }

    if (_programArgs.Length <= 3) throw new Exception("missing 4th arg; expected upper buy limit in USD, such as \"3.52\"");
    if (!Decimal.TryParse(_programArgs[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var upperLimit) || upperLimit < 0m)
    {
      throw new Exception($"invalid 4th arg; expected upper buy limit in USD, such as \"3.52\"");
    }

    if (_programArgs.Length <= 4) throw new Exception("missing 5th arg; expected first target in USD, such as \"5.00\"");
    if (!Decimal.TryParse(_programArgs[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var firstTarget) || firstTarget < 0m)
    {
      throw new Exception($"invalid 5th arg; expected first target in USD, such as \"5.00\"");
    }

    if (upperLimit <= firstTarget)
    {
      throw new Exception($"invalid 4th arg \"upper limit\" must be greater than 5th arg \"first target\"");
    }

    if (_programArgs.Length <= 5) throw new Exception("missing 6th arg; expected buyback range in USD, such as \"1.00\"");
    if (!Decimal.TryParse(_programArgs[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var buybackRange) || buybackRange < 0m)
    {
      throw new Exception($"invalid 6th arg; expected buyback range in USD, such as \"1.00\"");
    }

    // check that coinType is valid
    var accounts = await _accounts.GetAccounts();
    var account = accounts.FirstOrDefault(x => x.CoinType.ToLowerInvariant() == coinType.ToLowerInvariant() && x.CoinType != "USD")
      ?? throw new Exception($"invalid 2nd arg; unrecognized coin type; provide one of: " + 
      string.Join(", ", accounts.Where(x => x.CoinType != "USD").Select(x => x.CoinType)));
      
    // fix capitalization of user input
    coinType = account.CoinType;

    // check that USD is available to spend
    var usdAccount = accounts.Single(x => x.CoinType == "USD");
    if (usdAccount.Available == 0m)
    {
      throw new Exception($"unable to perform order; you have {usdAccount}");
    }

    // check that the desired amount of USD is available to spend
    if (usdToSpend != null && usdToSpend.Value > usdAccount.Available)
    {
      throw new Exception($"invalid 3rd arg; insufficient USD, only ${usdAccount.Available} available");
    }

    // check that the current price is within the desired range
    var ticker = await _productTicker.GetTicker(coinType);
    if (ticker.LastTradePrice >= upperLimit)
    {
      Console.WriteLine($"the most recent trade for {coinType} was ${ticker.LastTradePrice} which is at/above your provided upper limit ${upperLimit} - are you sure? (Y/N)");
      if (Console.ReadLine()?.ToLowerInvariant() != "y") throw new Exception("cancelled");
    }

    string message = $"Watching {coinType} to buy ${(usdToSpend ?? usdAccount.Available)} when average price is under ${upperLimit}, drops below ${firstTarget} and then rises by ${buybackRange}";
    Console.WriteLine(message);
    var lastTradePrices = new Queue<Decimal>();
    lastTradePrices.Enqueue(ticker.LastTradePrice);
    var average = ticker.LastTradePrice;
    int maxDecimals = ticker.LastTradePrice.GetDecimalDigits();
    var minimumObservedAverage = average;
    int priceChecksSinceMessagePrint = 0;
    while (true)
    {
      if (average < upperLimit)
      {
        if (minimumObservedAverage <= firstTarget)
        {
          if (average >= minimumObservedAverage + buybackRange)
          {
            break;
          }
        }
      }

      try
      {
        if (priceChecksSinceMessagePrint >= 10)
        {
          priceChecksSinceMessagePrint = 0;
          Console.WriteLine();
          Console.WriteLine(message);
        }
        priceChecksSinceMessagePrint++;
        await Task.Delay(4000);
        ticker = await _productTicker.GetTicker(coinType);
        lastTradePrices.Enqueue(ticker.LastTradePrice);
        while (lastTradePrices.Count > 12) lastTradePrices.Dequeue();
        average = lastTradePrices.Average();
        minimumObservedAverage = Math.Min(minimumObservedAverage, average);
        maxDecimals = Math.Max(maxDecimals, ticker.LastTradePrice.GetDecimalDigits());
        var averageText = average.SetMaxDecimals(maxDecimals).ToString();
        var lastTradePriceText = ticker.LastTradePrice.SetMaxDecimals(maxDecimals).ToString();
        Console.WriteLine($"1-minute-average=${averageText} last-trade-price=${lastTradePriceText}");
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }

    Console.WriteLine($"Ding fries are done!");
    if (usdToSpend == null)
    {
      accounts = await _accounts.GetAccounts();
      account = accounts.SingleOrDefault(x => x.CoinType == "USD") ?? throw new Exception($"somehow unable to learn available USD before sale");
      usdToSpend = account.Available;
    }
    Console.WriteLine($"buying ${usdToSpend.Value} of {coinType} at ${lastTradePrices.Peek()} each at {DateTime.Now}");
    var order = await _createOrder.MarketBuy(coinType, usdToSpend.Value);
    Console.WriteLine(order.Fields.ToString());
  }
}