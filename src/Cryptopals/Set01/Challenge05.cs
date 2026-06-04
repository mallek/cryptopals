using Cryptopals.Codecs;

namespace Cryptopals.Set01;

/// <summary>
/// Set 1 · Challenge 5 — Implement repeating-key XOR.
/// https://cryptopals.com/sets/1/challenges/5
///
/// Encrypt text under a repeating multi-byte key (Vigenère, in binary) and
/// return the result as hex.
/// </summary>
public static class Challenge05
{
    /// <summary>
    /// Encrypt <paramref name="plaintext"/> under <paramref name="key"/> using
    /// repeating-key XOR, returning the ciphertext as a lowercase hex string.
    /// </summary>
    public static string Encrypt(string plaintext, string key, Action<string>? trace = null)
    {
        // TODO: pure composition — text to bytes, key to bytes, repeating-key XOR, hex-encode.
        // If this is more than ~4 lines, a primitive is missing.
        byte[] plainTextBytes = ByteFormat.ToBytes(plaintext);
        byte[] keyBytes = ByteFormat.ToBytes(key);
        byte[] ciphertextBytes = Xor.RepeatingKey(plainTextBytes, keyBytes, trace);
        return Hex.Encode(ciphertextBytes);}
}
