import static org.junit.jupiter.api.Assertions.assertEquals;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.AfterEach;
 
public class JUnit5ExampleTest {
 
  private static int _count;

  public JUnit5ExampleTest() {
    _count++;
    System.out.printf("constructor %d\n", _count);
  }

  @Test
  public void justAnExample() throws Exception {
    System.out.printf("justAnExample %d\n", _count);
    Thread.sleep(100); // just long enough to confirm concurrency when you look at the test output
  }
    
  @Test
  public void justAnotherExample() throws Exception {
    System.out.printf("justAnotherExample %d\n", _count);
    Thread.sleep(100); // just long enough to confirm concurrency when you look at the test output
  }
  
  // these are here for me to learn/confirm that
  // - YES INDEED the tests are running in parallel
  // - YES INDEED a new instance of this class is created for each test that runs
  @BeforeAll
  public static void beforeAll() {
    System.out.printf("beforeAll %d\n", _count);
  }

  @BeforeEach
  public void beforeEach() {
    System.out.printf("beforeEach %d\n", _count);
  }

  @AfterEach
  public void afterEach() {
    System.out.printf("afterEach %d\n", _count);
  }

  @AfterAll
  public static void afterAll() {
    System.out.printf("afterAll %d\n", _count);
  }
}