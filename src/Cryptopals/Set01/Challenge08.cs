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
            byte[] bytes = Hex.Decode(hex);
            IEnumerable<string> blocks = bytes.Chunk(16).Select(b => Hex.Encode(b));
            int totalBlocks = blocks.Count();
            int distinctBlocks = blocks.Distinct().Count();
            int repeatedBlocks = totalBlocks - distinctBlocks;
            trace?.Invoke($"Line {i}: {repeatedBlocks} repeated blocks");
            detections[i] = new EcbDetection(LineNumber: i, Hex: hex, RepeatedBlocks: repeatedBlocks);
        }

        return detections.OrderByDescending(kvp => kvp.Value.RepeatedBlocks).First().Value;
    }
}

/// <summary>The line judged to be AES-ECB: which line, its hex, and how many blocks repeated.</summary>
public record EcbDetection(int LineNumber, string Hex, int RepeatedBlocks);
