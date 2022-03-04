package FreedomBot;

public class Console_SystemConsole implements Console {
  public char[] readPassword() {
    return System.console().readPassword("[%s]", "Password:");
  }
}