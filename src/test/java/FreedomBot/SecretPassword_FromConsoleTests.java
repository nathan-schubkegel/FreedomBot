import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertArrayEquals;
import org.junit.jupiter.api.Test;
import FreedomBot.SecretPassword_FromConsole;
 
public class SecretPassword_FromConsoleTests {
  
  @Test
  public void getPassword_WhateverUserTypes_ReturnsThoseBytes() throws Exception {
    var console = new Console_Mock();
    var input = "some wurdz\nblahaha excess data".getBytes("UTF-8");
    for (int i = 0; i < input.length; i++) console.Input.add((char)input[i]);
    var service = new SecretPassword_FromConsole(console);
    
    byte[] result = service.getPassword();

    assertArrayEquals("some wurdz".getBytes("UTF-8"), result);
  }
}