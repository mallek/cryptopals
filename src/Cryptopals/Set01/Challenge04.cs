using Cryptopals.Ciphers;
using Cryptopals.Codecs;
using Cryptopals.Scoring;
using Cryptopals.Visualization;

namespace Cryptopals.Set01;

/// <summary>
/// Set 1 · Challenge 4 — Detect single-character XOR.
/// https://cryptopals.com/sets/1/challenges/4
///
/// One line in a file of ~327 hex strings was encrypted with single-byte XOR;
/// the other ~326 are random noise. Crack every line, then keep the single
/// best-scoring result across all of them — the needle floats to the top.
///
/// This is Challenge 3 wrapped in one more "take the max": Crack already finds
/// the best key for ONE ciphertext; Detect finds the best line among MANY.
/// </summary>
public static class Challenge04
{
    /// <summary>
    /// Find the one line that was single-byte XOR encrypted, by cracking each and
    /// keeping the highest-scoring result.
    /// </summary>
    public static Detection Detect(IReadOnlyList<string> hexLines, Action<string>? trace = null)
    {
        // TODO:
        //   for each line (with its index):
        //     decode the hex, Crack it QUIETLY (pass null — 327×256 scorings, no trace),
        //     keep the best CrackResult AND remember which line produced it.
        //   then explain the winner LOUDLY once at the end (re-trace just that line's crack).
        //
        // Design note (question 1): CrackResult carries key/plaintext/score but not the
        // source line. This Detection record pairs the crack with its line context. If you'd
        // rather track the line some other way, change the shape — your call.
        Detection? best = null;

        for (int i = 0; i < hexLines.Count; i++)
        {
            string hex = hexLines[i];
            //guard against invalid hex, but don't fail the whole thing if one line is bad:
            if (!Hex.IsValid(hex))            {
                trace?.Invoke($"line {i}: invalid hex, skipping");
                continue;
            }
            byte[] ciphertext = Hex.Decode(hex);
            CrackResult result = Challenge03.Crack(ciphertext, null);
            trace?.Invoke($"line {i}: score {result.Score} for key 0x{result.Key:X2}");
            if (best is null || result.Score > best.Result.Score)
            {
                best = new Detection(i, hex, result);
            }
        }

        if (best is null)
        {
            trace?.Invoke("No valid lines found.");
            throw new InvalidOperationException("No valid lines found.");
        }

        // Explain the winner loudly: re-run JUST the winning decryption with the sink,
        // so the detailed per-byte breakdown appears for the answer only.
        trace?.Section($"Winner: line {best.LineNumber} with key 0x{best.Result.Key:X2} " +
                       $"score {best.Result.Score:F2} → \"{best.Result.Plaintext.ToAscii()}\"");
        trace?.Line("XOR decryption:");
        Xor.SingleByte(Hex.Decode(best.CipherHex), best.Result.Key, trace);
        trace?.Line("English scoring:");
        EnglishScore.Score(best.Result.Plaintext, trace);
        trace?.Line($"plaintext: \"{best.Result.Plaintext.ToAscii()}\"");


        return best ?? throw new InvalidOperationException("No valid detection found");
    }
}

/// <summary>Which line won, its ciphertext, and the crack result for it.</summary>
public record Detection(int LineNumber, string CipherHex, CrackResult Result);
