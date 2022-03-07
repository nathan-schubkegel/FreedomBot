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
    Nickname = nickname;
    Id = id;
    Passphrase = passphrase;
    ApiSecret = apiSecret;
    SanityCheck = sanityCheck;
  }
}