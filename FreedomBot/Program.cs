using Ninject;
using FreedomBot;
using System.Text;
using Newtonsoft.Json;

namespace FreedomBot;

public class Program
{
  public static void Main(string[] args)
  {
    try
    {
      var kernel = new StandardKernel();
      kernel.Bind<IHttpClientSingleton>().To<HttpClientSingleton>().InSingletonScope();
      kernel.Bind<IHttpLogger>().To<HttpLogger>().InSingletonScope();
      kernel.Bind<IEncryptor>().To<Encryptor>().InSingletonScope();
      
      // ask the user for his password
      var passwordText = ConsoleReadSensitiveString("Enter password: ");
      var passwordBytes = Encoding.UTF8.GetBytes(passwordText);
      
      // load the user's encrypted saved data
      var apiKeyDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "apiKey.data");
      ApiKeyData apiKeyData;
      if (File.Exists(apiKeyDataPath))
      {
        var bytesEncrypted = File.ReadAllBytes(apiKeyDataPath);
        var bytesDecrypted = kernel.Get<IEncryptor>().Decrypt(bytesEncrypted, passwordBytes);
        var text = Encoding.UTF8.GetString(bytesDecrypted);
        apiKeyData = JsonConvert.DeserializeObject<ApiKeyData>(text) ?? throw new Exception("Failed to deserialize saved data from " + apiKeyDataPath);
      }
      else
      {
        apiKeyData = new ApiKeyData();
        apiKeyData.Nickname = ConsoleReadSensitiveString("Enter API Key Nickname: ");
        apiKeyData.Id = ConsoleReadSensitiveString("Enter API Key Id: ");
        apiKeyData.Passphrase = ConsoleReadSensitiveString("Enter API Key Passphrase: ");
        apiKeyData.ApiSecret = ConsoleReadSensitiveString("Enter API Key Secret: ");
        var text = JsonConvert.SerializeObject(apiKeyData);
        var bytes = Encoding.UTF8.GetBytes(text);
        var bytesEncrypted = kernel.Get<IEncryptor>().Encrypt(bytes, passwordBytes);
        File.WriteAllBytes(apiKeyDataPath, bytesEncrypted);
      }
      
      // lol... print out the sensitive info we just collected
      Console.WriteLine(JsonConvert.SerializeObject(apiKeyData, Formatting.Indented));
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine("Unhandled program-level " + ex.ToString());
    }
  }
  
  private static string ConsoleReadSensitiveString(string prompt)
  {
    Console.Write(prompt ?? string.Empty);
    var passwordText = new StringBuilder();
    while (true)
    {
      var key = Console.ReadKey(true);
      if (key.Key == ConsoleKey.Enter) break;
      if (key.Key == ConsoleKey.Backspace)
      {
        if (passwordText.Length > 0)
        {
          passwordText.Length = passwordText.Length - 1;
        }
      }
      else
      {
        passwordText.Append(key.KeyChar);
      }
    }
    if (!string.IsNullOrEmpty(prompt))
    {
      Console.WriteLine();
    }
    return passwordText.ToString();
  }
}