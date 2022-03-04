package FreedomBot;

// Dependency injection for console interaction, which isn't necessarily available during unit tests
public interface Console {
  char[] readPassword() throws Exception;
}