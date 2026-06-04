using Cryptopals.Codecs;

namespace Cryptopals.Set01;

/// <summary>
/// Set 1 · Challenge 8 — Detect AES-128-ECB.
/// https://cryptopals.com/sets/1/challenges/8
///
/// Find the ECB-encrypted ciphertext with NO key and NO decryption. ECB is a deterministic
/// codebook, so repeated 16-byte plaintext blocks become repeated 16-byte ciphertext blocks.
/// A 128-bit block repeating by chance is ~1 in 2¹²⁸ — impossible — so ANY repeat is structural.
/// The line with repeated blocks is the ECB one.
/// </summary>
public static class Challenge08
{
    /// <summary>Return the line most likely AES-ECB: the one with the most repeated 16-byte blocks.</summary>
    public static EcbDetection Detect(IReadOnlyList<string> hexLines, Action<string>? trace = null)
    {
        // TODO: for each line (track its index):
        //   Hex.Decode → split into 16-byte blocks → count repeats = (total blocks − distinct blocks).
        //   Hash each block by its hex (e.g. Hex.Encode(block)) into a HashSet to count distinct.
        //   Keep the line with the MOST repeated blocks.
        var detections = new Dictionary<int, EcbDetection>();

        for (int i = 0; i < hexLines.Count; i++)
        {
            string hex = hexLines[i];
            // .ToList() so Count()/Distinct() don't re-run the decode twice on a lazy sequence.
            var blocks = Hex.Decode(hex).Chunk(16).Select(b => Hex.Encode(b)).ToList();
            int repeatedBlocks = blocks.Count - blocks.Distinct().Count();
            if (repeatedBlocks > 0)                         // only the interesting lines (cuts 203 zeros)
                trace.Line($"line {i}: {repeatedBlocks} repeated block(s)");
            detections[i] = new EcbDetection(i, hex, repeatedBlocks);
        }

        EcbDetection winner = detections.OrderByDescending(kvp => kvp.Value.RepeatedBlocks).First().Value;

        // Show the winning line's blocks, flagging the one(s) that repeat — the ECB fingerprint
        // made visible: a single 16-byte value appearing several times in a sea of unique blocks.
        trace.Section($"ECB found: line {winner.LineNumber}");
        var winnerBlocks = Hex.Decode(winner.Hex).Chunk(16).Select(b => Hex.Encode(b)).ToList();
        var counts = winnerBlocks.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());
        for (int b = 0; b < winnerBlocks.Count; b++)
        {
            int n = counts[winnerBlocks[b]];
            trace.Detail($"block {b,2}: {winnerBlocks[b]}{(n > 1 ? $"   ← repeats ×{n}" : "")}");
        }

        return winner;
    }
}

/// <summary>The line judged to be AES-ECB: which line, its hex, and how many blocks repeated.</summary>
public record EcbDetection(int LineNumber, string Hex, int RepeatedBlocks);
