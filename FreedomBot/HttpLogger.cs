namespace FreedomBot;

public class HttpLogger : IHttpLogger
{
  public void Log(string text)
  {
    System.Console.WriteLine(text);
  }
}