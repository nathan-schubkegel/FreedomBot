namespace FreedomBot;

public interface IHttpClientSingleton
{
  Task UseAsync(string description, CancellationToken stoppingToken, Func<HttpClient, Task> action);
}

public class HttpClientSingleton : IHttpClientSingleton
{
  public static readonly string UserAgent = "FreedomBot/0.0.0";
  private readonly IHttpLogger _logger;
  private readonly HttpClient _client = new HttpClient();
  private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
  
  public HttpClientSingleton(IHttpLogger logger)
  {
    _logger = logger;
  }

  public async Task UseAsync(string description, CancellationToken stoppingToken, Func<HttpClient, Task> action)
  {
    await _semaphore.WaitAsync(stoppingToken);
    try
    {
      _logger.Log($"HttpClientSingleton: {description}");
      await action(_client);
    }
    finally
    {
      await Task.Delay(1000, stoppingToken); // ensure 1 full second between all coinbase api requests
      _semaphore.Release();
    }
  }
}