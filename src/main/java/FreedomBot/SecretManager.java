package FreedomBot;

public interface SecretManager {
  // Loads the given named value from storage.
  String LoadValue(String name) throws Exception;

  // Saves the given named value to storage.
  void SaveValue(String name, String value) throws Exception;
}