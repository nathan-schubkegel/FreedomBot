import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.Disabled;
import java.util.Arrays;
import FreedomBot.AppDataDirectory_JarLocation;
 
public class AppDataDirectory_JarLocationTests {
  
  @Test
  public void getDir_Always_IsDirectoryNamedData() throws Exception {
    var service = new AppDataDirectory_JarLocation();
    String result = service.getDir().getName();
    assertEquals("data", result);
  }
  
  @Test
  public void getDir_Always_DirectoryExists() throws Exception {
    var service = new AppDataDirectory_JarLocation();
    boolean result = service.getDir().exists();
    assertEquals(true, result, "directory exists?");
    result = service.getDir().isDirectory();
    assertEquals(true, result, "is directory?");
  }
  
  // can't really test this because at test time there's no jar file... just build output directories and .class files...
  @Disabled
  @Test
  public void getDir_Always_IsDirectoryNextToJarFile() throws Exception {
    var service = new AppDataDirectory_JarLocation();
    String[] result = service.getDir().getParentFile().list();
    assertTrue(Arrays.asList(result).contains("FreedomBot.jar"));
  }
}