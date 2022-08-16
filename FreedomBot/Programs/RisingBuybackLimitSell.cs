using System.Globalization;

namespace FreedomBot.Programs;

// Program that sells a quantity of coins when the average price over 1 minute meets some conditions.
//  - Failure Limit (provided by user) - if the price ever drops to/below this point, then sell.
//  - First Target (provided by user) - once the price rises to this point, then sell when
//                                      the price drops to [Maximum Price Observed minus Buyback Range] or less.
//  - Buyback Range (provided by user)
// Examples: with failure limit = $0.45, price starting at $0.50, first target = $0.55, Buyback Range = $0.03
//   - if price drops to/below $0.45, this program sells at $0.45
//   - if price floats between $0.46 and $0.54 then drops, this program sells at $0.45
//   - if price reaches $0.55 then drops, this program sells at $0.52
//   - if price reaches $0.60 then drops, this program sells at $0.57

public class RisingBuybackLimitSell
{
  private readonly Coinbase.IAccounts _accounts;
  private readonly Coinbase.IProductTicker _productTicker;
  private readonly Coinbase.ICreateOrder _createOrder;
  private readonly ProgramArgs _programArgs;
  
  public RisingBuybackLimitSell(Coinbase.IAccounts accounts, Coinbase.IProductTicker productTicker,
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

    Decimal? coinCount = null;
    if (_programArgs.Length <= 2) throw new Exception("missing 3rd arg; expected coin count, such as \"0.151\" or \"all\"");
    if (_programArgs[2].ToLowerInvariant() != "all")
    {
      if (!Decimal.TryParse(_programArgs[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var hardCoinCount))
      {
        throw new Exception($"invalid 3rd arg; expected coin count, such as \"0.151\" or \"all\"");
      }
      coinCount = hardCoinCount;
    }

    if (_programArgs.Length <= 3) throw new Exception("missing 4th arg; expected failure sell limit in USD, such as \"3.52\"");
    if (!Decimal.TryParse(_programArgs[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var failureLimit) || failureLimit < 0m)
    {
      throw new Exception($"invalid 4th arg; expected failure sell limit in USD, such as \"3.52\"");
    }

    if (_programArgs.Length <= 4) throw new Exception("missing 5th arg; expected first target in USD, such as \"5.00\"");
    if (!Decimal.TryParse(_programArgs[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var firstTarget) || firstTarget < 0m)
    {
      throw new Exception($"invalid 5th arg; expected first target in USD, such as \"5.00\"");
    }

    if (failureLimit >= firstTarget)
    {
      throw new Exception($"invalid 4th arg \"failure limit\" must be less than 5th arg \"first target\"");
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

    // check that coins are available to sell
    if (account.Available == 0m)
    {
      var availableAccounts = string.Join(", ", accounts.Where(x => x.CoinType != "USD" && x.Available > 0m));
      if (availableAccounts.Length > 0)
      {
        availableAccounts = "; you must specify a coin type with available funds such as " + availableAccounts;
      }
      throw new Exception($"unable to perform order; you have {account}{availableAccounts}");
    }
    
    // check that the desired number of coins are available to sell
    if (coinCount != null && coinCount.Value > account.Available)
    {
      throw new Exception($"invalid 3rd arg; insufficient coins, only {account.Available} available");
    }

    // check that the current price is within the desired range
    var ticker = await _productTicker.GetTicker(coinType);
    if (ticker.LastTradePrice <= failureLimit)
    {
      throw new Exception($"unable to perform order; the most recent trade for {coinType} was ${ticker.LastTradePrice} which is at/below your provided failure limit ${failureLimit}");
    }
    if (ticker.LastTradePrice > firstTarget - buybackRange)
    {
      Console.WriteLine($"the most recent trade for {coinType} was ${ticker.LastTradePrice} which is above your provided first target ${firstTarget} minus buyback range ${buybackRange} - are you sure? (Y/N)");
      if (Console.ReadLine()?.ToLowerInvariant() != "y") throw new Exception("cancelled");
    }

    string message = $"Watching {coinType} to sell {(coinCount?.ToString() ?? "all")} when average price drops to ${failureLimit} or climbs to/above ${firstTarget} and then drops by ${buybackRange}";
    Console.WriteLine(message);
    var lastTradePrices = new Queue<Decimal>();
    lastTradePrices.Enqueue(ticker.LastTradePrice);
    var average = ticker.LastTradePrice;
    int maxDecimals = ticker.LastTradePrice.GetDecimalDigits();
    var highestObservedAverage = average;
    int priceChecksSinceMessagePrint = 0;
    while (average > failureLimit)
    {
      if (highestObservedAverage >= firstTarget)
      {
        if (average <= highestObservedAverage - buybackRange)
        {
          break;
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
        highestObservedAverage = Math.Max(highestObservedAverage, average);
        maxDecimals = Math.Max(maxDecimals, ticker.LastTradePrice.GetDecimalDigits());
        var averageText = average.SetMaxDecimals(maxDecimals).ToString();
        var highestObservedAverageText = highestObservedAverage.SetMaxDecimals(maxDecimals).ToString();
        var lastTradePriceText = ticker.LastTradePrice.SetMaxDecimals(maxDecimals).ToString();
        Console.WriteLine($"highest-observed=${highestObservedAverageText} 1-minute-average=${averageText} last-trade-price=${lastTradePriceText}");
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }

    Console.WriteLine($"Ding fries are done!");
    if (coinCount == null)
    {
      accounts = await _accounts.GetAccounts();
      account = accounts.FirstOrDefault(x => x.CoinType == coinType) ?? throw new Exception($"somehow unable to learn coin count before sale");
      coinCount = account.Available;
    }
    Console.WriteLine($"selling {coinCount.Value} {coinType} at ${lastTradePrices.Peek()} at {DateTime.Now}");
    var order = await _createOrder.MarketSell(coinType, coinCount.Value);
    Console.WriteLine(order.Fields.ToString());
  }
}