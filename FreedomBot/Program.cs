﻿using Ninject;
using FreedomBot;
using System.Text;

namespace FreedomBot
{
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
          
          var encryptor = kernel.Get<IEncryptor>();
          var encrypted = encryptor.Encrypt(Encoding.UTF8.GetBytes(responseBody), Encoding.UTF8.GetBytes("hey it's my password"));
          var decrypted = encryptor.Decrypt(encrypted, Encoding.UTF8.GetBytes("hey it's my password"));
          Console.WriteLine(Encoding.UTF8.GetString(decrypted));
        });
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine("Unhandled program-level " + ex.ToString());
      }
    }
  }
}