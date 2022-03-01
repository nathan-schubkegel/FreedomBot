package FreedomBot;

import com.google.inject.Inject;
import java.io.File;
import java.nio.file.Paths;

public class AppDataDirectory_JarLocation implements AppDataDirectory {
  private File _value;
  
  // The @Inject annotation marks this constructor as eligible to be used by Guice.
  @Inject
  public AppDataDirectory_JarLocation() {
    File jarFile = new File(SecretManager.class.getProtectionDomain().getCodeSource().getLocation().toURI());
    _value = Paths.get(jarFile.getParent().getAbsolutePath(), "data").toFile();
    _value.mkdirs();
  }
  
  public File GetValue() {
    return _value;
  }
}