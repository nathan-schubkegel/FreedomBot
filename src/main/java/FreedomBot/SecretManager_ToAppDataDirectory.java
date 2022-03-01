package FreedomBot;

import com.google.inject.Inject;
import java.util.HashMap;
import java.io.File;
import java.nio.file.Files;
import java.nio.file.Paths;

public class SecretManager_ToAppDataDirectory implements SecretManager {
  
  private const String _secretFileName = "secrets.json";
  private File _secretFile;
  private String _salt;
  private HashMap<String, String> _data;
  private Object _twoFishSessionKey;

  class SecretData {
    public String salt;
    public String data;
  }
  
  // The @Inject annotation marks this constructor as eligible to be used by Guice.
  @Inject
  public SecretManager_ToAppDataDirectory(AppDataDirectory appDataDirectory) {
    _secretFile = Paths.get(appDataDirectory.GetValue(), _secretFileName).toFile();
  }
  
  private void EnsureDataLoaded() {
    if (_secretData != null) return;
    
    if (_secretFile.exists()) {
      String c = Files.readString(fileName, Charset.forName("UTF-8"));
      _secretData = gson.fromJson(c, SecretData.class);
    }
    else {
      _secretData = new SecretData();
      _secretData.salt = // TODO: make a random new salt
      _secretData.data = new HashMap<String, String>();
    }
  }

  // Assigns the password to be used to load and save values.
  // This must be invoked before loading/saving will work.
  public void SetPassword(String password) {
    byte[] passwordBytes = password.getByte(Charset.forName("UTF-8"));

    // TwoFish expects a 64/128/192/256-bit hashing key
    // so if the user typed a short password, repeat its characters
    byte[] hashingKey = new byte[32]; // 32 bytes is 256 bits
    for (int i = 0; i < 32; i++)
    {
      hashingKey[i] = passwordBytes[i % passwordBytes.size()];
    }

    // xor with a random salt because salt is good
    SecureRandom random = new SecureRandom();
    _salt = new byte[32];
    random.nextBytes(_salt);
    for (int i = 0; i < 32; i++)
    {
      
    }
    
    _twoFishSessionKey = Twofish_Algorithm.makeKey(hashingKey);
  }
  
  // Loads the given named value from storage.
  public String LoadValue(String name) throws Exception {
    
    if (_twoFishSessionKey == null) { throw new Exception("SetPassword() must be called before LoadValue() will work"); }
    
    // FUTURE: read the data from file
    // TODO: handle null values
    var encryptedData = _data.get(name);
    if (encryptedData == null) return null;
    
    if (encryptedData.size() % Twofish_Algorithm.blockSize() != 0) {
      throw new Exception("Found invalid encrypted data length");
    }
    
    byte[] clearData = new byte[encryptedData.size()];
    for (int i = 0; i < encryptedData.size(); i += Twofish_Algorithm.blockSize()) {
      byte[] part = Twofish_Algorithm.blockDecrypt(encryptedData, i, _twoFishSessionKey);
      for (int j = 0; j < part.size(); j++)
      {
        clearData[i + j] = part[j];
      }
    }
    
    // learn the length of the original string by dropping zero-padding bytes from the end
    int unused = 0;
    for (int i = clearData.size() - 1; i >= 0; i--)
    {
      if (clearData[i] == 0) unused++;
      else break;
    }
    
    return new String(clearData, 0, clearData.size() - unused, "UTF-8");
  }
  
  // Saves the given named value to storage.
  public void SaveValue(String name, String value) throws Exception {
    
    if (_twoFishSessionKey == null) { throw new Exception("SetPassword() must be called before SaveValue() will work"); }
    
    // TODO: handle null values
    byte[] valueBytes = value.getByte(Charset.forName("UTF-8"));
    
    // Twofish requires BLOCKSIZE sized input, so add zero-byte padding if needed
    if (valueBytes.size() % Twofish_Algorithm.blockSize() != 0) {
      byte[] betterBytes = new byte[((valueBytes.size() / Twofish_Algorithm.blockSize()) + 1) * Twofish_Algorithm.blockSize()];
      for (int i = 0; i < betterBytes.size(); i++) {
        if (i < valueBytes.size()) {
          betterBytes[i] = valueBytes[i];
        }
        else {
          betterBytes[i] = 0;
        }
      }
      valueBytes = betterBytes;
    }
    
    byte[] encryptedData = new byte[betterBytes.size()];
  }
}
/*
    import com.google.crypto.tink.CleartextKeysetHandle;
    import com.google.crypto.tink.KeysetHandle;
    import com.google.crypto.tink.KeyTemplates;
    import com.google.crypto.tink.JsonKeysetWriter;
    import java.io.File;

    // Generate the key material...
    KeysetHandle keysetHandle = KeysetHandle.generateNew(
        KeyTemplates.get("AES256_GCM"));

    // and write it to a file.
    String keysetFilename = "my_keyset.json";
    CleartextKeysetHandle.write(keysetHandle, JsonKeysetWriter.withFile(
        new File(keysetFilename)));
        
        
        
        
        
    import com.google.crypto.tink.CleartextKeysetHandle;
    import com.google.crypto.tink.KeysetHandle;
    import java.io.File;

    String keysetFilename = "my_keyset.json";
    KeysetHandle keysetHandle = CleartextKeysetHandle.read(
        JsonKeysetReader.withFile(new File(keysetFilename)));
        
        
        
        
        
    import com.google.crypto.tink.Aead;
    import com.google.crypto.tink.KeysetHandle;
    import com.google.crypto.tink.KeyTemplates;

    // 1. Generate the key material.
    KeysetHandle keysetHandle = KeysetHandle.generateNew(
        KeyTemplates.get("AES256_GCM"));

    // 2. Get the primitive.
    Aead aead = keysetHandle.getPrimitive(Aead.class);

    // 3. Use the primitive to encrypt a plaintext,
    byte[] ciphertext = aead.encrypt(plaintext, aad);

    // ... or to decrypt a ciphertext.
    byte[] decrypted = aead.decrypt(ciphertext, aad);
    */