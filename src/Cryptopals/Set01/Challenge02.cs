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
        byte[] bytesA = Hex.Decode(hexA, trace);
        byte[] bytesB = Hex.Decode(hexB, trace);
        byte[] xored = Xor.Apply(bytesA, bytesB, trace);

        trace?.Invoke(bytesA.ToAscii());
        trace?.Invoke(bytesB.ToAscii());
        trace?.Invoke(xored.ToAscii());

        return Hex.Encode(xored, trace);
    }
}
