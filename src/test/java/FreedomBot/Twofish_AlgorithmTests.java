import static org.junit.jupiter.api.Assertions.assertTrue;
import org.junit.jupiter.api.Test;
import FreedomBot.Twofish_Algorithm;
 
class Twofish_AlgorithmTests {
 
    @Test
    void selfTest() {
        boolean result = Twofish_Algorithm.self_test();
        assertTrue(result);
    }
}