package FreedomBot;

import java.io.ByteArrayOutputStream;
import java.io.PrintStream;
import com.google.inject.Inject;
import java.util.ArrayList;

public class SecretPassword_FromConsole implements SecretPassword {
  private Console _console;
  private byte[] _password;
  
  // The @Inject annotation marks this constructor as eligible to be used by Guice.
  @Inject
  public SecretPassword_FromConsole(Console console) {
    _console = console;
  }
  
  public byte[] getPassword() throws Exception {
    if (_password == null) {
      char[] data = _console.readPassword();
      _password = new byte[data.length];
      for (int j = 0; j < _password.length; j++) _password[j] = (byte)data[j];
    }
    var result = new byte[_password.length];
    for (int i = 0; i < _password.length; i++) result[i] = _password[i];
    return _password;
  }
}