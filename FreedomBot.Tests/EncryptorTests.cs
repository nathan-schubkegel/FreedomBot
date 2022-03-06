namespace FreedomBot.Tests;

using Xunit;
using FreedomBot;
using System.Text;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
      string responseBody = @"{
  'name': 'modern major general',
  'information': 'vegetable, animal, and mineral'
}";
      var encryptor = new Encryptor();
      var encryptedBytes = encryptor.Encrypt(Encoding.UTF8.GetBytes(responseBody), Encoding.UTF8.GetBytes("hey it's my password"));
      var decryptedBytes = encryptor.Decrypt(encryptedBytes, Encoding.UTF8.GetBytes("hey it's my password"));
      var decryptedText = Encoding.UTF8.GetString(decryptedBytes);
      Assert.Equal(responseBody, decryptedText);
    }
}