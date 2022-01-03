package FreedomBot;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.net.http.HttpResponse.BodyHandlers;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Base64;
import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;

public class TickerFetcher {

  public static void PrintTheTicker() {
    try {
      var key = "<your key>";
      var secret = "<your secret>";
      var passphrase = "<your passphrase>";
      var coinType = "ETH";

      var timestamp = String.valueOf(System.currentTimeMillis() / 1000l);
      var method = "GET";
      var body = "";
      var requestPath = "/products/" + coinType + "-USD/ticker";
      var message = timestamp + method + requestPath + body;

      var secretBytes = Base64.getDecoder().decode(secret);
      var thingy = Mac.getInstance("HmacSHA256");
      thingy.init(new SecretKeySpec(secretBytes, "HmacSHA256"));
      var signBytes = thingy.doFinal(message.getBytes(StandardCharsets.UTF_8));
      var sign = Base64.getEncoder().encodeToString(signBytes);

      // create a client and request
      var client = HttpClient.newHttpClient();
      var request = HttpRequest.newBuilder(URI.create("https://api.exchange.coinbase.com" + requestPath))
        .header("CB-ACCESS-KEY", key)
        .header("CB-ACCESS-SIGN", sign)
        .header("CB-ACCESS-TIMESTAMP", timestamp)
        .header("CB-ACCESS-PASSPHRASE", passphrase)
        .header("accept", "application/json")
        .build();

      // use the client to send the request
      var responseFuture = client.sendAsync(request, BodyHandlers.ofString());

      // TODO: google to learn what is the Java equivalent of C#'s async/await and use it here

      // This blocks until the request is complete
      HttpResponse<String> response = responseFuture.get();

      // the response:
      //System.out.println(response.body());

      GsonBuilder builder = new GsonBuilder(); 
      builder.setPrettyPrinting(); 
      Gson gson = builder.create(); 
      TickerResponse k = gson.fromJson(response.body(), TickerResponse.class);
      
      System.out.println("");
      System.out.println("Ticker for " + coinType + ":");
      System.out.println("price:  " + k.price);
      System.out.println("size:   " + k.size);
      System.out.println("time:   " + k.time);
      System.out.println("bid:    " + k.bid);
      System.out.println("ask:    " + k.ask);
      System.out.println("volume: " + k.volume);
      System.out.println("");
    }
    catch (Exception ex) {
      System.out.println(ex.toString());
    }
  }

  class TickerResponse {
    public long trade_id;
    public String price;
    public String size;
    public String time;
    public String bid;
    public String ask;
    public String volume;
  }
}