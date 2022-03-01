package FreedomBot;

import com.google.inject.Inject;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.net.http.HttpResponse.BodyHandlers;
import java.util.concurrent.CompletableFuture;

public class SharedHttpClient_RateLimited implements SharedHttpClient {
  
  private boolean _hasLastUsedTime;
  private long _lastUsedTime;
  private HttpClient _client;
  
  // The @Inject annotation marks this constructor as eligible to be used by Guice.
  @Inject
  public SharedHttpClient_RateLimited() {
    _client = HttpClient.newHttpClient();
  }
  
  public <T> CompletableFuture<HttpResponse<T>> sendAsync(HttpRequest request, HttpResponse.BodyHandler<T> responseBodyHandler) {
    // wait until the most recent 'sendAsync' completed more than 1 second ago
    if (_hasLastUsedTime)
    {
      while (true) {
        long now = System.nanoTime();
        long difference = now - _lastUsedTime;
        if (difference > 1000000000)
        {
          break;
        }

        try {
          Thread.sleep(100);
        }
        catch (Exception ex) {
          return CompletableFuture.failedFuture(ex);
        }
      }
    }
    
    // send the request
    try {
      var r = _client.sendAsync(request, responseBodyHandler);
      
      // this blocks the thread until the response has arrived
      // which isn't great, but it's what I need for now
      // until I figure out how to do async for reals in java --nathschu
      try {
        var r2 = r.get();
      }
      catch (Exception ex)
      {
        return CompletableFuture.failedFuture(ex);
      }
      
      return r;
    }
    finally {
      _hasLastUsedTime = true;
      _lastUsedTime = System.nanoTime();  
    }
  }
}