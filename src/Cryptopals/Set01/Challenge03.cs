using Cryptopals.Ciphers;
using Cryptopals.Scoring;

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
    public static CrackResult Crack(byte[] ciphertext, Action<string>? trace = null, Func<byte[], double>? scorer = null)
    {
        // Pluggable scorer — defaults to the corpus-based English score. Swap it to score
        // against a different language model (a domain-tuned corpus, a sharper bigram scorer).
        var score01 = scorer ?? EnglishScore.ScoreCorpus;

        double bestScore = double.NegativeInfinity;
        byte bestKey = 0;
        byte[] bestPlaintext = Array.Empty<byte>();

        // Search quietly: withhold the sink from the 256 sub-calls so we don't get
        // the full per-byte story of every wrong guess. Crack traces its own one-line
        // summary per key instead.
        for(int key = 0; key < 256; key++)
        {
            byte keyedByte = (byte)key;
            byte[] decrypted = Xor.SingleByte(ciphertext, keyedByte, null);
            double score = score01(decrypted);
            trace.Line($"Key 0x{key:X2} ('{keyedByte.ToAscii()}') → score {score,8:F2}");
            if (score > bestScore)
            {
                bestScore = score;
                bestKey = keyedByte;
                bestPlaintext = decrypted;
                trace.Detail($"↑ new best: 0x{bestKey:X2} ('{bestKey.ToAscii()}') score {bestScore:F2}");
            }
        }

        // Explain the winner loudly: re-run JUST the winning decryption with the sink,
        // so the detailed per-byte breakdown appears for the answer only.
        trace.Section($"Winner: key 0x{bestKey:X2} ('{bestKey.ToAscii()}') score {bestScore:F2}");
        trace.Line("XOR decryption:");
        Xor.SingleByte(ciphertext, bestKey, trace);
        trace.Line("English scoring:");
        EnglishScore.Score(bestPlaintext, trace);
        trace.Line($"plaintext: \"{bestPlaintext.ToAscii()}\"");

        return new CrackResult(bestKey, bestPlaintext, bestScore);
    }
}

/// <summary>The outcome of cracking a single-byte XOR cipher.</summary>
public record CrackResult(byte Key, byte[] Plaintext, double Score);
