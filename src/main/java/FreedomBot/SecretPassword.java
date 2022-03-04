package FreedomBot;

// Gives access to a password provided by the user presumably once at application start
public interface SecretPassword {
  byte[] getPassword() throws Exception;
}