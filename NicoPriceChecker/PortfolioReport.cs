using FreedomBot;
using System.Globalization;

namespace NicoPriceChecker;

// Program that reports how much of each coin is held, and what they're worth in $USD
public class PortfolioReport
{
  private readonly FreedomBot.Coinbase.IAccounts _accounts;
  private readonly FreedomBot.Coinbase.IProductTicker _productTicker;
  
  public PortfolioReport(FreedomBot.Coinbase.IAccounts accounts, FreedomBot.Coinbase.IProductTicker productTicker)
  {
    _accounts = accounts;
    _productTicker = productTicker;
  }
  
  public async Task Run()
  {
    var totalValueUsd = 0m;
    var accounts = (await _accounts.GetAccounts()).Where(a => a.Balance > 0m).ToList();
    foreach (var account in accounts)
    {
      if (account.CoinType != "USD")
      {
        var ticker = await _productTicker.GetTicker(account.CoinType);
        var balance = account.Balance.SetMaxDecimals(account.Balance.GetDecimalDigits());
        var valueUsd = (ticker.LastTradePrice * account.Balance).SetMaxDecimals(2);
        totalValueUsd += valueUsd;
        Console.WriteLine($"You have {balance} {account.CoinType}, and it's worth ${valueUsd} USD");
      }
      else
      {
        Console.WriteLine($"You have ${account.Balance.SetMaxDecimals(2)} USD");
      }
    }
    if (accounts.Count == 0)
    {
      Console.WriteLine($"You have nothing! You lose!");
    }
    else
    {
      Console.WriteLine($"Total Value = ${totalValueUsd} USD");
    }
    Console.WriteLine("(press any key to continue)");
    Console.ReadKey(true);
  }
}