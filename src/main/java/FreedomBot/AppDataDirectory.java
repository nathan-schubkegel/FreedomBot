package FreedomBot;

import java.io.File;

// Provides a directory where this application's data can be stored.
public interface AppDataDirectory {
  // Gets the application directory path
  File getDir();
}
