namespace FreedomBot;

using System.Text;
using Ninject;
using Newtonsoft.Json;

public interface IApiKeyDataManager
{
  string ApiKeyDataFilePath { get; set; }
  Task<ApiKeyData> GetData();
}

public class ApiKeyDataManager : IApiKeyDataManager
{
  private readonly ISecureConsole _console;
  private readonly IEncryptionPassword _password;
  private readonly IEncryptor _encryptor;
  private ApiKeyData? _apiKeyData;
  
  public string ApiKeyDataFilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "apiKey.data");

  public ApiKeyDataManager(ISecureConsole console, IEncryptionPassword password, IEncryptor encryptor)
  {
    _console = console;
    _password = password;
    _encryptor = encryptor;
  }

  public async Task<ApiKeyData> GetData()
  {
    if (_apiKeyData != null) return _apiKeyData;
  
    // get the encryption/decryption password from the user
    var passwordText = await _password.GetPassword();
    var passwordBytes = Encoding.UTF8.GetBytes(passwordText);

    // does the user already have encrypted data?
    var apiKeyDataPath = ApiKeyDataFilePath;
    if (File.Exists(apiKeyDataPath))
    {
      try
      {
        var bytesEncrypted = File.ReadAllBytes(apiKeyDataPath);
        var bytesDecrypted = _encryptor.Decrypt(bytesEncrypted, passwordBytes);
        var text = Encoding.UTF8.GetString(bytesDecrypted);
        var apiKeyData = JsonConvert.DeserializeObject<ApiKeyData>(text) ?? throw new Exception("unexpected null encrypted JSON data");
        if (apiKeyData.SanityCheck != ApiKeyData.ExpectedSanityCheck) throw new Exception("unexpected SanityCheck value found");

        _apiKeyData = apiKeyData;
      }
      catch (Exception ex)
      {
        throw new Exception("Failed to decrypt " + apiKeyDataPath + " due to: " + ex.GetType().FullName + " - " + ex.Message);
      }
    }
    else
    {
      var nickname = await _console.ReadLine("Enter API Key Nickname: ");
      var id = await _console.ReadLine("Enter API Key Id: ");
      var passphrase = await _console.ReadSensitiveLine("Enter API Key Passphrase: ");
      var apiSecret = await _console.ReadSensitiveLine("Enter API Key Secret: ");
      
      var apiKeyData = new ApiKeyData(
        nickname: nickname,
        id: id,
        passphrase: passphrase,
        apiSecret: apiSecret,
        sanityCheck: ApiKeyData.ExpectedSanityCheck);

      var text = JsonConvert.SerializeObject(apiKeyData);
      var bytes = Encoding.UTF8.GetBytes(text);
      var bytesEncrypted = _encryptor.Encrypt(bytes, passwordBytes);
      File.WriteAllBytes(apiKeyDataPath, bytesEncrypted);

      _apiKeyData = apiKeyData;
    }
    
    return _apiKeyData;
  }
}