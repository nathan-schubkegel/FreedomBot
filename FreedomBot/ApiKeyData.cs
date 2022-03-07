public class ApiKeyData
{
  public string Nickname { get; }
  public string Id { get; }
  public string Passphrase { get; }
  public string ApiSecret { get; }
  public string SanityCheck { get; }

  public const string ExpectedSanityCheck = "hello, I am Galstaff, sorcerer of light!";

  public ApiKeyData(string nickname, string id, string passphrase, string apiSecret, string sanityCheck)
  {
    Nickname = nickname ?? throw new ArgumentNullException(nameof(nickname));
    Id = id ?? throw new ArgumentNullException(nameof(id));
    Passphrase = passphrase ?? throw new ArgumentNullException(nameof(passphrase));
    ApiSecret = apiSecret ?? throw new ArgumentNullException(nameof(apiSecret));
    SanityCheck = sanityCheck ?? throw new ArgumentNullException(nameof(sanityCheck));
  }
}