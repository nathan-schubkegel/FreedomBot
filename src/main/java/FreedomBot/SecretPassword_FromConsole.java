package FreedomBot;

import java.io.Console;
import com.google.inject.Inject;

public class SecretPassword_FromConsole implements SecretPassword {
  private String _password;
  
  // The @Inject annotation marks this constructor as eligible to be used by Guice.
  @Inject
  public SecretPassword_FromConsole() {
  }
  
  private String GetPassword() {
    if (_password == null) {
      char[] data = System.console().readPassword("[%s]", "Password:");
      _password = new String(data);
    }
    return _password;
  }
}