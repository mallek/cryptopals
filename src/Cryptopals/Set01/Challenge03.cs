namespace Cryptopals.Set01;

/// <summary>
/// Set 1 · Challenge 3 — Single-byte XOR cipher.
/// https://cryptopals.com/sets/1/challenges/3
///
/// A message was XOR'd against one secret byte. Recover the key and the message
/// by trying all 256 possible keys and keeping whichever decryption scores most
/// like English.
/// </summary>
public static class Challenge03
{
    /// <summary>
    /// Brute-force a single-byte XOR cipher. Returns the winning key, the decrypted
    /// bytes, and the score so the caller can inspect the result.
    /// </summary>
    /// <remarks>
    /// Returning a small record (rather than just the plaintext) is a design choice:
    /// the key and score are useful for tests and for the trace. If you'd rather
    /// return something else, that's your call — change the shape to fit.
    /// </remarks>
    public static CrackResult Crack(byte[] ciphertext, Action<string>? trace = null)
    {
        // TODO: for key in 0..255 — decrypt, score, keep the best.
        //   - Xor.SingleByte to decrypt
        //   - EnglishScore.Score to rank
        //   - track the winner (mind your score DIRECTION — best = highest or lowest?)
        throw new NotImplementedException();
    }
}

/// <summary>The outcome of cracking a single-byte XOR cipher.</summary>
public record CrackResult(byte Key, byte[] Plaintext, double Score);
