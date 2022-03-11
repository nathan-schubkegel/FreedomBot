using System.Globalization;

namespace FreedomBot.Programs;

public class DoubleSidedLimitSell
{
  private readonly Coinbase.IAccounts _accounts;
  private readonly Coinbase.IProductTicker _productTicker;
  private readonly Coinbase.ICreateOrder _createOrder;
  private readonly ProgramArgs _programArgs;
  private string _coinType = string.Empty;
  private Decimal? _coinCount;
  private Decimal _low;
  private Decimal _high;
  
  public DoubleSidedLimitSell(Coinbase.IAccounts accounts, Coinbase.IProductTicker productTicker,
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
    _coinType = _programArgs[1];
    
    if (_programArgs.Length <= 2) throw new Exception("missing 3rd arg; expected coin count, such as \"0.151\" or \"all\"");
    if (_programArgs[2].ToLowerInvariant() != "all")
    {
      if (!Decimal.TryParse(_programArgs[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var coinCount))
      {
        throw new Exception($"invalid 3rd arg; expected coin count, such as \"0.151\" or \"all\"");
      }
      _coinCount = coinCount;
    }

    if (_programArgs.Length <= 3) throw new Exception("missing 4th arg; expected low sell limit in USD, such as \"3.52\"");
    if (!Decimal.TryParse(_programArgs[3], NumberStyles.Float, CultureInfo.InvariantCulture, out _low))
    {
      throw new Exception($"invalid 4th arg; expected low sell limit in USD, such as \"3.52\"");
    }

    if (_programArgs.Length <= 4) throw new Exception("missing 5th arg; expected high sell limit in USD, such as \"5.00\"");
    if (!Decimal.TryParse(_programArgs[4], NumberStyles.Float, CultureInfo.InvariantCulture, out _high))
    {
      throw new Exception($"invalid 5th arg; expected high sell limit in USD, such as \"5.00\"");
    }

    if (_low >= _high)
    {
      throw new Exception($"invalid 4th arg \"low sell limit\" must be less than 5th arg \"high sell limit\"");
    }

    // check that _coinType is valid
    var accounts = await _accounts.GetAccounts();
    var account = accounts.FirstOrDefault(x => x.CoinType.ToLowerInvariant() == _coinType.ToLowerInvariant() && x.CoinType != "USD")
      ?? throw new Exception($"invalid 2nd arg; unrecognized coin type; provide one of: " + 
      string.Join(", ", accounts.Where(x => x.CoinType != "USD").Select(x => x.CoinType)));
      
    // fix capitalization of user input
    _coinType = account.CoinType;

    // check that coins are available to sell
    if (account.Available == 0m)
    {
      var availableAccounts = string.Join(", ", accounts.Where(x => x.CoinType != "USD" && x.Available > 0m));
      if (availableAccounts.Length > 0)
      {
        availableAccounts = "; you must specify a coin type with available funds such as " + availableAccounts;
      }
      throw new Exception($"unable to perform double-sided limit order; you have {account}{availableAccounts}");
    }
    
    // check that the desired number of coins are available to sell
    if (_coinCount != null && _coinCount.Value > account.Available)
    {
      throw new Exception($"invalid 3rd arg; insufficient coins, only {account.Available.ToString(CultureInfo.InvariantCulture)} available");
    }

    // check that the current price is within the desired range
    var ticker = await _productTicker.GetTicker(_coinType);
    if (ticker.LastTradePrice < _low)
    {
      throw new Exception($"unable to perform double-sided limit order; the most recent trade for {_coinType} was " +
        $"${ticker.LastTradePrice.ToString(CultureInfo.InvariantCulture)} which is below your provided low sell limit " +
        $"${_low.ToString(CultureInfo.InvariantCulture)}");
    }
    if (ticker.LastTradePrice > _high)
    {
      throw new Exception($"unable to perform double-sided limit order; the most recent trade for {_coinType} was " +
        $"${ticker.LastTradePrice.ToString(CultureInfo.InvariantCulture)} which is above your provided high sell limit " +
        $"${_high.ToString(CultureInfo.InvariantCulture)}");
    }

    Console.WriteLine($"Entering loop to watch current price of {_coinType}, " +
      $"to sell {(_coinCount?.ToString(CultureInfo.InvariantCulture) ?? "all")} " +
      $"when average price over 1 minute drops to ${_low.ToString(CultureInfo.InvariantCulture)} " +
      $"or climbs to ${_high.ToString(CultureInfo.InvariantCulture)}");

    var lastTradePrices = new Queue<Decimal>();
    lastTradePrices.Enqueue(ticker.LastTradePrice);
    var average = ticker.LastTradePrice;
    int maxDecimals = ticker.LastTradePrice.GetDecimalDigits();
    while (average > _low && average < _high)
    {
      try
      {
        ticker = await _productTicker.GetTicker(_coinType);
        lastTradePrices.Enqueue(ticker.LastTradePrice);
        while (lastTradePrices.Count > 60) lastTradePrices.Dequeue();
        average = lastTradePrices.Average();
        maxDecimals = Math.Max(maxDecimals, ticker.LastTradePrice.GetDecimalDigits());
        var averageText = average.SetMaxDecimals(maxDecimals).ToString(CultureInfo.InvariantCulture);
        var lastTradePriceText = ticker.LastTradePrice.SetMaxDecimals(maxDecimals).ToString(CultureInfo.InvariantCulture);
        Console.WriteLine($"1-minute-average=${averageText} last-trade-price=${lastTradePriceText}");
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }

    Console.WriteLine($"Ding fries are done!");
    accounts = await _accounts.GetAccounts();
    account = accounts.FirstOrDefault(x => x.CoinType == _coinType) ?? throw new Exception($"somehow unable to learn coin count before sale");
    Console.WriteLine($"selling {account.Available} {_coinType} at ${lastTradePrices.Peek().ToString(CultureInfo.InvariantCulture)} at {DateTime.Now}");
    var order = await _createOrder.MarketSell(_coinType, account.Available);
    Console.WriteLine(order.Fields.ToString());
  }
}