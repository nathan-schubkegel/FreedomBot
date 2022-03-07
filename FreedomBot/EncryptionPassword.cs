namespace FreedomBot;

public interface IEncryptionPassword
{
  Task<string> GetPassword();
}

public class EncryptionPassword : IEncryptionPassword
{
  private ISecureConsole _console;
  private string? _password;

  public EncryptionPassword(ISecureConsole console)
  {
    _console = console;
  }

  public async Task<string> GetPassword()
  {
    if (_password == null)
    {
      _password = await _console.ReadSensitiveLine("Enter password: ");
    }
    
    if (string.IsNullOrEmpty(_password))
    {
      throw new Exception("Invalid null or empty password provided!");
    }
    
    return _password!;
  }
}