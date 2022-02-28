package FreedomBot;

import java.net.URI;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.net.http.HttpResponse.BodyHandlers;
import java.util.HashMap;
import com.google.inject.Inject;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;

public class PriceFetcher {
  private SharedHttpClient _client;

  // The @Inject annotation marks this constructor as eligible to be used by Guice.
  @Inject
  public PriceFetcher(SharedHttpClient client) {
    _client = client;
  }

  public String FetchThePrices() {
    try {
      // create a request
      var request = HttpRequest.newBuilder(
             URI.create("https://api.kucoin.com/api/v1/prices?base=USD"))
         .header("accept", "application/json")
         .build();

      // use the client to send the request
      var responseFuture = _client.sendAsync(request, BodyHandlers.ofString());

      // TODO: google to learn what is the Java equivalent of C#'s async/await and use it here

      // This blocks until the request is complete
      HttpResponse<String> response = responseFuture.get();

      // the response:
      //System.out.println(response.body());
      
      GsonBuilder builder = new GsonBuilder(); 
      builder.setPrettyPrinting(); 
      Gson gson = builder.create(); 
      KucoinResponse k = gson.fromJson(response.body(), KucoinResponse.class);
      
      var sb = new StringBuilder();
      for (String key : k.data.keySet()) {
        String value = k.data.get(key);
        sb.append(key + ": " + value + "\n");
      }
      return sb.toString();
    }
    catch (Exception ex) {
      return ex.toString();
    }
  }
  
  class KucoinResponse {
    public HashMap<String, String> data;
  }
}