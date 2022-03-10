using Ninject;

namespace FreedomBot;

public static class Program
{
  public static async Task<int> Main(string[] argsArray)
  {
    try
    {
      var args = new ProgramArgs(argsArray);
      StandardKernel kernel = CreateServiceProvider(args);
      switch (args[0].ToLowerInvariant())
      {
        case "double-sided-limit-sell":
          await kernel.Get<Programs.DoubleSidedLimitSell>().Run();
          break;
          
        default:
          throw new Exception("invalid arguments; first argument must be double-sided-limit-sell");
      };
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
    kernel.Bind<Coinbase.ICreateOrder>().To<Coinbase.CreateOrder>().InSingletonScope();
    kernel.Bind<Programs.DoubleSidedLimitSell>().ToSelf();
    kernel.Bind<ProgramArgs>().ToMethod(_ => args).InSingletonScope();
    return kernel;
  }
}