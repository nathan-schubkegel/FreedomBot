namespace FreedomBot;

using System.Text;

public interface ISecureConsole
{
  Task<string> ReadLine(string? prompt = null);
  Task<string> ReadSensitiveLine(string? prompt = null);
}

public class SecureConsole : ISecureConsole
{
  public Task<string> ReadLine(string? prompt = null) => Task.Run(() =>
  {
    if (!string.IsNullOrEmpty(prompt))
    {
      Console.Write(prompt ?? string.Empty);
    }
    return Console.ReadLine() ?? throw new Exception("Standard input exhausted");
  });
  
  public Task<string> ReadSensitiveLine(string? prompt = null) => Task.Run(() =>
  {
    if (!string.IsNullOrEmpty(prompt))
    {
      Console.Write(prompt ?? string.Empty);
    }
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
  });
}