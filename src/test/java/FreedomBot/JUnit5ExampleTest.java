import static org.junit.jupiter.api.Assertions.assertEquals;
import org.junit.jupiter.api.Test;
 
class JUnit5ExampleTest {
 
    @Test
    void justAnExample() {
        System.out.println("This test method should be run");
    }
    
    @Test
    void justAnExampleFailure() {
        assertEquals(false, true);
    }
}