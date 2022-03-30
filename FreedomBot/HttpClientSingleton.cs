namespace FreedomBot;

public interface IHttpClientSingleton
{
  Task UseAsync(string description, Func<HttpClient, Task> action);
}

public class HttpClientSingleton : IHttpClientSingleton
{
  public static readonly string UserAgent = "FreedomBot/0.0.0";
  private readonly IHttpLogger _logger;
  private readonly HttpClient _client = new HttpClient();
  private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
  private System.Diagnostics.Stopwatch? _timer;
  
  public HttpClientSingleton(IHttpLogger logger)
  {
    _logger = logger;
  }

  public async Task UseAsync(string description, Func<HttpClient, Task> action)
  {
    await _semaphore.WaitAsync();
    try
    {
      // ensure 1 full second between all coinbase api requests
      if (_timer == null)
      {
        _timer = System.Diagnostics.Stopwatch.StartNew();
      }
      else
      {
        int msRemaining = Math.Max(0, 1000 - (int)_timer.ElapsedMilliseconds);
        if (msRemaining > 0)
        {
          await Task.Delay(msRemaining);
        }
      }
      if (description != null)
      {
        _logger.Log($"HttpClientSingleton: {description}");
      }
      await action(_client);
    }
    finally
    {
      _semaphore.Release();
      _timer?.Restart();
    }
  }
}