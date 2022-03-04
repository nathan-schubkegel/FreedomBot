package FreedomBot;

import java.io.ByteArrayOutputStream;
import java.io.PrintStream;
import com.google.inject.Inject;
import java.util.ArrayList;

public class SecretPassword_FromConsole implements SecretPassword {
  private byte[] _password;
  
  // The @Inject annotation marks this constructor as eligible to be used by Guice.
  @Inject
  public SecretPassword_FromConsole() {
  }
  
  public byte[] getPassword() throws Exception {
    if (_password == null) {
      // not using System.console() because it returns null in unit tests
      System.out.printf("Password: ");
      
      // redirect stdout so the user's typed keys don't appear in the console
      ByteArrayOutputStream stream = new ByteArrayOutputStream();
      PrintStream newOutput = new PrintStream(stream);
      PrintStream oldOutput = System.out;
      System.setOut(newOutput);
      try {
        var meh = new ArrayList<Byte>();
        while (true) {
          int i = System.in.read();
          if (i == -1) {
            break;
          }
          if (i == (byte)'\n' || i == (byte)'\r') {
            break;
          }
          meh.add((byte)i);
        }
        _password = new byte[meh.size()];
        for (int j = 0; j < _password.length; j++) _password[j] = meh.get(j);
      }
      finally {
        System.setOut(oldOutput);
      }
      System.out.printf("\n");
    }
    var result = new byte[_password.length];
    for (int i = 0; i < _password.length; i++) result[i] = _password[i];
    return _password;
  }
}