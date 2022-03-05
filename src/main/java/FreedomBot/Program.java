package FreedomBot;

import com.google.inject.Guice;
import com.google.inject.Injector;

import javafx.application.Application;
import javafx.scene.Scene;
import javafx.scene.control.Label;
import javafx.scene.layout.StackPane;
import javafx.stage.Stage;

public class Program extends Application {
  
  private static Injector _injector;
  
  public static void main(String[] args) {
    _injector = Guice.createInjector(new ProgramDependencyInjectionModule());
    PriceFetcher priceFetcher = _injector.getInstance(PriceFetcher.class);
    String lastPrices = null;
    for (int i = 0; i < 10; i++) {
      lastPrices = priceFetcher.FetchThePrices();
      System.out.println("fetched some prices...");
    }
    System.out.println(lastPrices);
    
    launch();
  }
  
  @Override
  public void start(Stage stage) {
    String javaVersion = System.getProperty("java.version");
    String javafxVersion = System.getProperty("javafx.version");
    Label l = new Label("Hello, JavaFX " + javafxVersion + ", running on Java " + javaVersion + ".");
    Scene scene = new Scene(new StackPane(l), 640, 480);
    stage.setScene(scene);
    stage.show();
  }
}