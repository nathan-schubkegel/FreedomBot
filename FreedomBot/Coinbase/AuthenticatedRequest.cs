using FreedomBot;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace FreedomBot.Coinbase;

public static class AuthenticatedRequest
{
  public static HttpRequestMessage MakeRequest(this ApiKeyData apiKey, HttpMethod method, string requestPath, JObject? body = null)
  {
    byte[] base64decodedSecretKey = Convert.FromBase64String(apiKey.ApiSecret);
    var timestamp = ((long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds).ToString();
    var methodText = method == HttpMethod.Get ? "GET" : method == HttpMethod.Post ? "POST" : throw new ArgumentException($"unsupported {nameof(method)} \"{method}\"");
    if (!requestPath.StartsWith("/")) requestPath = "/" + requestPath;
    string? bodyText = body?.ToString();
    byte[] prehash = Encoding.UTF8.GetBytes(timestamp + methodText + requestPath + bodyText);
    var hasher = new HMACSHA256(base64decodedSecretKey);
    byte[] hash = hasher.ComputeHash(prehash);
    var base64hash = Convert.ToBase64String(hash);

    var request = new HttpRequestMessage(method, $"https://api.exchange.coinbase.com" + requestPath);
    request.Headers.Add("Accept", "application/json");
    request.Headers.Add("User-Agent", HttpClientSingleton.UserAgent);
    request.Headers.Add("CB-ACCESS-KEY", apiKey.Id);
    request.Headers.Add("CB-ACCESS-SIGN", base64hash);
    request.Headers.Add("CB-ACCESS-TIMESTAMP", timestamp);
    request.Headers.Add("CB-ACCESS-PASSPHRASE", apiKey.Passphrase);
    if (bodyText != null)
    {
      request.Content = new StringContent(bodyText, Encoding.UTF8, "application/json");
    }

    return request;
  }
  
  public static async Task<string> SendGetRequest(this IHttpClientSingleton httpClientSingleton, ApiKeyData apiKey, string requestPath, string description)
  {
    string? responseBody = null;
    await httpClientSingleton.UseAsync($"fetching {description}", async http =>
    {
      var request = apiKey.MakeRequest(HttpMethod.Get, requestPath);
      var response = await http.SendAsync(request);
      responseBody = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"coinbase pro api for {description} returned {response.StatusCode}: {responseBody}");
      }
    });
    return responseBody ?? throw new NullReferenceException();
  }

  public static async Task<string> SendPostRequest(this IHttpClientSingleton httpClientSingleton, ApiKeyData apiKey, string requestPath, string description, JObject body)
  {
    string? responseBody = null;
    await httpClientSingleton.UseAsync($"posting {description}", async http =>
    {
      var request = apiKey.MakeRequest(HttpMethod.Post, requestPath, body);
      var response = await http.SendAsync(request);
      responseBody = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"coinbase pro api for posting {description} returned {response.StatusCode}: {responseBody}");
      }
    });
    return responseBody ?? throw new NullReferenceException();
  }
}