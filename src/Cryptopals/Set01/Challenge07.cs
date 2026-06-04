using Cryptopals.Aes;

namespace Cryptopals.Set01;

/// <summary>
/// Set 1 · Challenge 7 — AES-128 in ECB mode.
/// https://cryptopals.com/sets/1/challenges/7
///
/// Decrypt a base64 file that was AES-128-ECB encrypted under a known key — using the AES
/// you hand-rolled, not a library. Pure composition: Base64.Decode → AesEcb.Decrypt.
/// </summary>
public static class Challenge07
{
    public static byte[] Decrypt(string base64Ciphertext, string key, Action<string>? trace = null)
    {
        // TODO: Base64.Decode the content, ToBytes the key, AesEcb.Decrypt. ~3 lines.
        byte[] ciphertext = Base64.Decode(base64Ciphertext);
        byte[] keyBytes = ByteFormat.ToBytes(key);
        return AesEcb.Decrypt(ciphertext, keyBytes, trace: trace);
    }
}
