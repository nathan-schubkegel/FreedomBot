using FreedomBot;
using System.Security.Cryptography;
using System.Text;

namespace FreedomBot.Coinbase;

public static class AuthenticatedRequest
{
  public static HttpRequestMessage MakeRequest(this ApiKeyData apiKey, HttpMethod method, string requestPath, string? body = null)
  {
    byte[] base64decodedSecretKey = Convert.FromBase64String(apiKey.ApiSecret);
    var timestamp = ((long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds).ToString();
    var methodText = method == HttpMethod.Get ? "GET" : throw new ArgumentException($"unsupported {nameof(method)} \"{method}\"");
    if (!requestPath.StartsWith("/")) requestPath = "/" + requestPath;
    body ??= string.Empty;
    byte[] prehash = Encoding.UTF8.GetBytes(timestamp + methodText + requestPath + body);
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

    return request;
  }
}