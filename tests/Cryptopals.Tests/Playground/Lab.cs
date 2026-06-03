using Cryptopals;
using Cryptopals.Set01;

namespace Cryptopals.Tests.Playground;

/// <summary>
/// Experimentation harness — NOT a test of the library. Encrypt your own text with your
/// own key, run the Challenge 6 break against it, and get a report of what happened:
/// did it recover the key? how much plaintext landed in each bucket? where's the seam?
/// </summary>
public static class Lab
{
    public static BreakReport Run(string plaintext, byte[] key, Action<string>? trace = null)
    {
        byte[] plainBytes = plaintext.ToBytes();
        byte[] ciphertext = Xor.RepeatingKey(plainBytes, key);

        // bytes-per-bucket is THE number that predicts success: how much plaintext each
        // single-byte slice gets for frequency analysis to chew on. Big → easy, ~1 → hopeless.
        double bytesPerBucket = key.Length == 0 ? 0 : (double)ciphertext.Length / key.Length;

        int guessedKeyLen = 0;
        string recoveredKey = "", recoveredText = "";
        bool success = false;

        try
        {
            // Cap the search so detection stays reliable: each candidate length needs ENOUGH
            // chunks (≥4 here), and a wide range biases toward spurious large lengths (the
            // "minimum of many noisy estimates" trap the SeamSweep exposed). 40 is the classic.
            int maxKeyLen = Math.Clamp(ciphertext.Length / 4, 2, 40);
            Challenge06Result result = Challenge06.Break(ciphertext, minKeyLen: 2, maxKeyLen: maxKeyLen, trace: trace);
            guessedKeyLen = result.Key.Length;
            recoveredKey = result.Key.ToAscii();
            recoveredText = result.Plaintext.ToAscii();
            success = result.Plaintext.SequenceEqual(plainBytes);
        }
        catch (Exception ex)
        {
            recoveredText = $"<break failed: {ex.GetType().Name}>";
        }

        return new BreakReport(plainBytes.Length, key.ToAscii(), key.Length,
                               guessedKeyLen, recoveredKey, recoveredText, bytesPerBucket, success);
    }

    /// <summary>
    /// Like Run, but TELLS the break the correct key length instead of guessing it. This
    /// isolates the FUNDAMENTAL seam — per-bucket cracking — from the separate problem of
    /// detecting the key length. Use this to find where bytes-per-bucket gets too thin.
    /// </summary>
    public static BreakReport CrackKnownLength(string plaintext, byte[] key)
    {
        byte[] plainBytes = plaintext.ToBytes();
        byte[] ciphertext = Xor.RepeatingKey(plainBytes, key);
        double bytesPerBucket = (double)ciphertext.Length / key.Length;

        // Transpose with the KNOWN length, crack each single-byte bucket, reassemble.
        byte[][] buckets = Challenge06.Transpose(ciphertext, key.Length, null);
        byte[] recovered = new byte[key.Length];
        for (int b = 0; b < buckets.Length; b++)
            recovered[b] = Challenge03.Crack(buckets[b], null).Key;

        byte[] recoveredText = Xor.RepeatingKey(ciphertext, recovered);
        bool success = recoveredText.SequenceEqual(plainBytes);

        return new BreakReport(plainBytes.Length, key.ToAscii(), key.Length, key.Length,
                               recovered.ToAscii(), recoveredText.ToAscii(), bytesPerBucket, success);
    }
}

public record BreakReport(
    int TextLength, string ActualKey, int ActualKeyLength,
    int GuessedKeyLength, string RecoveredKey, string RecoveredText,
    double BytesPerBucket, bool Success);
