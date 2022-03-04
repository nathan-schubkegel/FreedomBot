package FreedomBot;

import com.google.inject.Guice;
import com.google.inject.Injector;

public class Program {
  
  public static void main(String[] args) {
    Injector injector = Guice.createInjector(new ProgramDependencyInjectionModule());
    
    // demo that the SecretPassword service works
    var secretPassword = injector.getInstance(SecretPassword.class);
    byte[] password;
    try {
      password = secretPassword.getPassword();
    } 
    catch (Exception ex) {
      System.out.printf("Failed to get password: %s", ex.toString());
      return;
    }
    System.out.printf("You typed: ");
    for (int i = 0; i < password.length; i++) {
      System.out.printf("%c", (char)password[i]);
    }
    System.out.printf("\n");
    
    // demo that the PriceFetcher service works
    PriceFetcher priceFetcher = injector.getInstance(PriceFetcher.class);
    String lastPrices = null;
    for (int i = 0; i < 10; i++) {
      lastPrices = priceFetcher.FetchThePrices();
      System.out.println("fetched some prices...");
    }
    System.out.println(lastPrices);
  }
}