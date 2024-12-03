using System;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionUtility
{
    private static readonly string encryptionKey = "FNSWGcdWsyz32QwaWadU43zYSJyUXD6z"; // 32文字のキーを使用

    public static string EncryptString(string plainText)
    {
        byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.GenerateIV();
            byte[] iv = aesAlg.IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

            using (var msEncrypt = new System.IO.MemoryStream())
            {
                msEncrypt.Write(iv, 0, iv.Length); // IVを先頭に書き込む

                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                    csEncrypt.FlushFinalBlock();

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
    }

    public static string DecryptString(string cipherText)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherText);
        byte[] key = Encoding.UTF8.GetBytes(encryptionKey);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;

            byte[] iv = new byte[aesAlg.BlockSize / 8];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aesAlg.IV = iv;

            int cipherTextOffset = iv.Length;
            int cipherTextLength = fullCipher.Length - iv.Length;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (var msDecrypt = new System.IO.MemoryStream(fullCipher, cipherTextOffset, cipherTextLength))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    byte[] plainBytes = new byte[cipherTextLength];
                    int decryptedByteCount = csDecrypt.Read(plainBytes, 0, plainBytes.Length);

                    return Encoding.UTF8.GetString(plainBytes, 0, decryptedByteCount);
                }
            }
        }
    }
}
