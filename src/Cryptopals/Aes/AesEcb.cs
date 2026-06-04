namespace Cryptopals.Aes;

/// <summary>
/// AES in ECB (Electronic Codebook) mode: each 16-byte block is encrypted/decrypted
/// INDEPENDENTLY with the same key. The simplest mode — and fatally flawed: identical
/// plaintext blocks produce identical ciphertext blocks, so structure leaks straight through
/// (the "ECB penguin"). That leak is exactly what Challenge 8 detects. Never use ECB for real.
/// </summary>
public static class AesEcb
{
    public const int BlockSize = 16;

    public static byte[] Encrypt(byte[] plaintext, byte[] key, Action<string>? trace = null)
    {
        // require plaintext length a multiple of 16; for each 16-byte block,
        //       Aes128.EncryptBlock(block, key) and concatenate the results.
        if (plaintext.Length % BlockSize != 0) throw new ArgumentException("Plaintext length must be a multiple of 16 bytes", nameof(plaintext));
        List<byte> ciphertext = new List<byte>(plaintext.Length);
        for (int i = 0; i < plaintext.Length; i += BlockSize)
        {            byte[] block = plaintext.Skip(i).Take(BlockSize).ToArray();
            byte[] encrypted = Aes128.EncryptBlock(block, key, trace: trace);
            ciphertext.AddRange(encrypted);
        }
        return ciphertext.ToArray();
    }

    public static byte[] Decrypt(byte[] ciphertext, byte[] key, Action<string>? trace = null)
    {
        // require ciphertext length a multiple of 16; for each 16-byte block,
        //       Aes128.DecryptBlock(block, key) and concatenate the results.
        if (ciphertext.Length % BlockSize != 0) throw new ArgumentException("Ciphertext length must be a multiple of 16 bytes", nameof(ciphertext));
        List<byte> plaintext = new List<byte>(ciphertext.Length);
        for (int i = 0; i < ciphertext.Length; i += BlockSize)
        {
            byte[] block = ciphertext.Skip(i).Take(BlockSize).ToArray();
            byte[] decrypted = Aes128.DecryptBlock(block, key, trace: trace);
            plaintext.AddRange(decrypted);
        }
        return plaintext.ToArray();
    }
}
