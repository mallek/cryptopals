using Cryptopals.Codecs;

namespace Cryptopals.Set01;

/// <summary>
/// Set 1 · Challenge 2 — Fixed XOR.
/// https://cryptopals.com/sets/1/challenges/2
/// </summary>
public static class Challenge02
{
    /// <summary>
    /// XOR two equal-length hex strings against each other, returning the result as hex.
    /// </summary>
    public static string FixedXor(string hexA, string hexB, Action<string>? trace = null)
    {
        trace.Section("Decode A");
        byte[] bytesA = Hex.Decode(hexA, trace);

        trace.Section("Decode B");
        byte[] bytesB = Hex.Decode(hexB, trace);

        trace.Section("A XOR B");
        byte[] xored = Xor.Apply(bytesA, bytesB, trace);

        // The story: three buffers as text, stacked so the result reads against its inputs.
        trace.Section("Result");
        trace.Line($"a:   \"{bytesA.ToAscii()}\"");
        trace.Line($"b:   \"{bytesB.ToAscii()}\"");
        trace.Line($"xor: \"{xored.ToAscii()}\"");

        return Hex.Encode(xored, trace);
    }
}
