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

      // get api key
      var apiKeyData = await kernel.Get<IApiKeyDataManager>().GetData();
      
      // lol... print out the sensitive info we just collected
      Console.WriteLine(JsonConvert.SerializeObject(apiKeyData, Formatting.Indented));
      Console.WriteLine(string.Join("\n", await kernel.Get<Coinbase.IProducts>().GetCoinTypesTradableForUsd()));
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine("Unhandled program-level " + ex.ToString());
    }
  }
}