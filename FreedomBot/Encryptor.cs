using System.Security.Cryptography;
using Medo.Security.Cryptography;

namespace FreedomBot;

public interface IEncryptor
{
  byte[] Encrypt(byte[] data, byte[] password);
  byte[] Decrypt(byte[] data, byte[] password);
}

public class Encryptor : IEncryptor
{
  private const int _saltLength = 16;

  public byte[] Encrypt(byte[] data, byte[] password)
  {
    var salt = new byte[_saltLength];
    var rng = RandomNumberGenerator.Create();
    rng.GetBytes(salt);
    
    var pw = GetKey(password);

    var algorithm = new TwofishManaged();
    ICryptoTransform transform = algorithm.CreateEncryptor(pw, salt);
    return salt.Concat(transform.TransformFinalBlock(data, 0, data.Length)).ToArray();
  }

  public byte[] Decrypt(byte[] data, byte[] password)
  {
    var salt = data.Take(_saltLength).ToArray();
    if (salt.Length != _saltLength) throw new Exception("Decryption failed; input data was not long enough");

    var pw = GetKey(password);
    
    var algorithm = new TwofishManaged();
    ICryptoTransform transform = algorithm.CreateDecryptor(pw, salt);
    return transform.TransformFinalBlock(data, _saltLength, data.Length - _saltLength);
  }
  
  private byte[] GetKey(byte[] password)
  {
    if (password.Length < 1) throw new Exception("Cryptography failed - you must provide at least 1 byte of password");
    var pw = new byte[32];
    // put first 32 bytes from 'password' into 'pw'
    for (int i = 0; i < password.Length && i < pw.Length; i++) pw[i] = password[i];
    // XOR remaining bytes from 'password' into 'pw'
    for (int i = pw.Length; i < password.Length; i++) pw[i % pw.Length] ^= password[i];
    return pw;
  }
}
