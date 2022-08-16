using FreedomBot;
using Ninject;
using System.Globalization;
using System.Linq;

namespace NicoPriceChecker;

public static class Program
{
  public static async Task<int> Main(string[] argsArray)
  {
    try
    {
      // Make it so ToString() and TryParse() never need to be given CultureInfo.InvariantCulture
      CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
      CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
      
      var args = new ProgramArgs(argsArray);
      StandardKernel kernel = CreateServiceProvider(args);
      
      // handle command line argument dictating api key file path
      var i = args.IndexOf("--apiKeyFile");
      if (i >= 0)
      {
        kernel.Get<IApiKeyDataManager>().ApiKeyDataFilePath = System.IO.Path.GetFullPath(args.Args[i+1]);
      }

      // run the program
      await kernel.Get<PortfolioReport>().Run();
      return 0;
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine(ex.ToString());
      return 1;
    }
  }
  
  public static StandardKernel CreateServiceProvider(ProgramArgs args)
  {
    var kernel = new StandardKernel();
    kernel.Bind<IHttpClientSingleton>().To<HttpClientSingleton>().InSingletonScope();
    kernel.Bind<IHttpLogger>().To<SilentHttpLogger>().InSingletonScope();
    kernel.Bind<IEncryptor>().To<Encryptor>().InSingletonScope();
    kernel.Bind<ISecureConsole>().To<SecureConsole>().InSingletonScope();
    kernel.Bind<IEncryptionPassword>().To<EncryptionPassword>().InSingletonScope();
    kernel.Bind<IApiKeyDataManager>().To<ApiKeyDataManager>().InSingletonScope();
    kernel.Bind<FreedomBot.Coinbase.IAccounts>().To<FreedomBot.Coinbase.Accounts>().InSingletonScope();
    kernel.Bind<FreedomBot.Coinbase.IProductTicker>().To<FreedomBot.Coinbase.ProductTicker>().InSingletonScope();
    kernel.Bind<PortfolioReport>().ToSelf();
    kernel.Bind<ProgramArgs>().ToMethod(_ => args).InSingletonScope();
    return kernel;
  }
}