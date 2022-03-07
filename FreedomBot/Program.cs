using Ninject;
using FreedomBot;
using System.Text;
using Newtonsoft.Json;

namespace FreedomBot;

public class Program
{
  public static async Task Main(string[] args)
  {
    try
    {
      var kernel = new StandardKernel();
      kernel.Bind<IHttpClientSingleton>().To<HttpClientSingleton>().InSingletonScope();
      kernel.Bind<IHttpLogger>().To<HttpLogger>().InSingletonScope();
      kernel.Bind<IEncryptor>().To<Encryptor>().InSingletonScope();
      kernel.Bind<ISecureConsole>().To<SecureConsole>().InSingletonScope();
      kernel.Bind<IEncryptionPassword>().To<EncryptionPassword>().InSingletonScope();
      kernel.Bind<IApiKeyDataManager>().To<ApiKeyDataManager>().InSingletonScope();
      kernel.Bind<Coinbase.IProducts>().To<Coinbase.Products>().InSingletonScope();
      kernel.Bind<Coinbase.IAccounts>().To<Coinbase.Accounts>().InSingletonScope();
      kernel.Bind<Coinbase.IOracle>().To<Coinbase.Oracle>().InSingletonScope();
      kernel.Bind<Coinbase.IProductTicker>().To<Coinbase.ProductTicker>().InSingletonScope();

      // get api key
      var apiKeyData = await kernel.Get<IApiKeyDataManager>().GetData();

      Console.WriteLine(string.Join("\n", (await kernel.Get<Coinbase.IProducts>().GetCoinTypesTradableForUsd())));
      Console.WriteLine(string.Join("\n", (await kernel.Get<Coinbase.IAccounts>().GetAccounts()).Where(x => x.Balance > 0m)));
      Console.WriteLine(string.Join("\n", (await kernel.Get<Coinbase.IOracle>().GetSignedPrices()).Prices.Select(x => $"{x.Key} = {x.Value.ToString("G29")}")));
      Console.WriteLine(string.Join("\n", (await kernel.Get<Coinbase.IProductTicker>().GetTicker("AVAX"))));
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine("Unhandled program-level " + ex.ToString());
    }
  }
}