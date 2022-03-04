import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertArrayEquals;
import org.junit.jupiter.api.Test;
import java.io.InputStream;
import java.io.ByteArrayInputStream;
import FreedomBot.SecretPassword_FromConsole;
 
public class SecretPassword_FromConsoleTests {
  
  @Test
  public void getPassword_WhateverUserTypes_ReturnsThoseBytes() throws Exception {
    var service = new SecretPassword_FromConsole();
    byte[] result;
    
    InputStream oldInput = System.in;
    ByteArrayInputStream newInput = new ByteArrayInputStream("some wurdz\nblahaha excess data".getBytes("UTF-8"));
    System.setIn(newInput);
    try {
      result = service.getPassword();
    }
    finally {
      // reset System.in to its original
      System.setIn(oldInput);
    }

    assertArrayEquals("some wurdz".getBytes("UTF-8"), result);
  }
}