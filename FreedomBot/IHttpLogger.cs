
namespace FreedomBot
{
  public interface IHttpLogger
  {
    void Log(string text);
  }

  public class HttpLogger : IHttpLogger
  {
    public void Log(string text)
    {
      System.Console.WriteLine(text);
    }
  }
}