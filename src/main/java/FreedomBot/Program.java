package FreedomBot;

public class Program {
  public static void main(String[] args) {
    Greeter greeter = new Greeter();
    System.out.println(greeter.sayHello());
    PricePrinter.PrintThePrices();
  }
}