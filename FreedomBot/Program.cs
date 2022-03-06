using Ninject;
using FreedomBot;

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
      
      var httpClientSingleton = kernel.Get<IHttpClientSingleton>();
      
      var request = new HttpRequestMessage(HttpMethod.Get, $"http://worldtimeapi.org/api/timezone/America/Los_Angeles");
      request.Headers.Add("Accept", "application/json");
      request.Headers.Add("User-Agent", HttpClientSingleton.UserAgent);
      await httpClientSingleton.UseAsync($"asking for the time", CancellationToken.None, async http => 
      {
        var response = await http.SendAsync(request);
        string responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
          throw new HttpRequestException($"time server returned {response.StatusCode}: {responseBody}");
        }
        
        Console.WriteLine(responseBody);
      });
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine("Unhandled program-level " + ex.ToString());
    }
  }
}