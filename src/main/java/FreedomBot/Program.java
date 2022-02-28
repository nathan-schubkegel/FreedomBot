package FreedomBot;

import com.google.inject.Guice;
import com.google.inject.Injector;

public class Program {
  
  public static void main(String[] args) {
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