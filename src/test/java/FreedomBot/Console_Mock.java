import java.util.LinkedList;
import FreedomBot.Console;

public class Console_Mock implements Console {
  public LinkedList<Character> Input = new LinkedList<Character>();
  
  public char[] readPassword() {
    // count how many characters to consume
    int count = 0;
    for (Character c : Input) {
      if (c == '\r' || c == '\n') break;
      count++;
    }
    
    // consume them characters!
    char[] result = new char[count];
    for (int i = 0; i < count; i++) {
      result[i] = Input.pop();
    }

    // consume trailing \n or \r\n
    if (Input.size() > 0 && Input.peek() == '\r') {
      Input.pop();
      if (Input.size() > 0 && Input.peek() == '\n') {
        Input.pop();
      }
    }
    else if (Input.size() > 0 && Input.peek() == '\n') {
      Input.pop();
    }

    return result;
  }
}