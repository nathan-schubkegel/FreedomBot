import static org.junit.jupiter.api.Assertions.assertEquals;
import org.junit.jupiter.api.Test;
 
class Twofish_Algorithm_Tests {
 
    @Test
    void selfTest() {
        boolean result = Twofish_Algorithm.self_test();
        assertTrue(result);
    }
}