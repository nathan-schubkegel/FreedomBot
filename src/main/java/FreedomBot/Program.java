package FreedomBot;

import com.google.inject.Guice;
import com.google.inject.Injector;
import com.google.crypto.tink.aead.AeadConfig;

public class Program {

  public static void main(String[] args) {
    // Register all AEAD key types with the Tink runtime.
    try {
      AeadConfig.register();
    }
    catch (Exception ex) {
      System.out.println(ex.toString());
      return;
    }

    Injector injector = Guice.createInjector(new ProgramDependencyInjectionModule());
    PriceFetcher priceFetcher = injector.getInstance(PriceFetcher.class);
    String lastPrices = null;
    for (int i = 0; i < 10; i++) {
      lastPrices = priceFetcher.FetchThePrices();
      System.out.println("fetched some prices...");
    }
    System.out.println(lastPrices);
  }
}