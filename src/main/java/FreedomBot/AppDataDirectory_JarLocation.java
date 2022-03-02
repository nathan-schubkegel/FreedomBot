package FreedomBot;

import com.google.inject.Inject;
import java.io.File;
import java.nio.file.Paths;

public class AppDataDirectory_JarLocation implements AppDataDirectory {
  private File _dir;
  
  // The @Inject annotation marks this constructor as eligible to be used by Guice.
  @Inject
  public AppDataDirectory_JarLocation() throws Exception {
    File jarFile = new File(AppDataDirectory_JarLocation.class.getProtectionDomain().getCodeSource().getLocation().toURI());
    _dir = Paths.get(jarFile.getParent(), "data").toAbsolutePath().toFile();
    _dir.mkdirs();
  }
  
  public File getDir() {
    return _dir;
  }
}