namespace FreedomBot.Tests;

using Xunit;
using FreedomBot;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

public class HttpClientSingletonTests
{
  public class MockHttpLogger : IHttpLogger
  {
    public List<string> Logs = new List<string>();
    public void Log(string text) => Logs.Add(text);
  }
  
  [Fact]
  public async Task UseAsync_ForReasonableData_PerformsTheRequest()
  {
    var logger = new MockHttpLogger();
    var service = new HttpClientSingleton(logger);
    var request = new HttpRequestMessage(HttpMethod.Get, $"http://worldtimeapi.org/api/timezone/America/Los_Angeles");
    request.Headers.Add("Accept", "application/json");
    request.Headers.Add("User-Agent", HttpClientSingleton.UserAgent);
    string? responseBody = null;
    await service.UseAsync($"asking for the time", CancellationToken.None, async http => 
    {
      var response = await http.SendAsync(request);
      responseBody = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"time server returned {response.StatusCode}: {responseBody}");
      }
    });
    
    Assert.Contains("\"datetime\":", responseBody);
  }
}