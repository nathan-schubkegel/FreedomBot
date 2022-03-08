using Ninject;
using FreedomBot;
using System.Text;
using Newtonsoft.Json;

namespace FreedomBot;

public class Program
{
  public static async Task<int> Main(string[] args)
  {
    try
    {
      var kernel = CreateServiceProvider();

      // get api key at the start of the app, so that the first thing the app always does is ask for password
      var apiKeyData = await kernel.Get<IApiKeyDataManager>().GetData();

      return 0;
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine("Unhandled program-level " + ex.ToString());
      return 1;
    }
  }
  
  public static StandardKernel CreateServiceProvider()
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
    kernel.Bind<Coinbase.IFees>().To<Coinbase.Fees>().InSingletonScope();
    return kernel;
  }
}