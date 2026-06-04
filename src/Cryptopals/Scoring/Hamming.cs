namespace Cryptopals.Scoring;

/// <summary>
/// Hamming distance: the number of differing BITS between two equal-length byte buffers.
/// Low distance between two stretches of XOR ciphertext means they decrypt to similar
/// (English-vs-English) bytes — which is how Challenge 6 detects the key length.
/// </summary>
public static class Hamming
{
    /// <summary>
    /// Total number of differing bits across all positions: sum over i of popcount(a[i] ^ b[i]).
    /// Cryptopals check: "this is a test" vs "wokka wokka!!!" = 37.
    /// </summary>
    public static int Distance(byte[] a, byte[] b, Action<string>? trace = null)
    {
        // TODO: require equal lengths (decide the guard). For each position, XOR the two bytes
        // and count the set bits in the result. Sum across all positions.
        // (The per-byte bit-count is the BitsDiffer trick from the Challenge 6 viewer.)
        if (a.Length != b.Length) throw new ArgumentException("Buffers must be of equal length");
        int totalDistance = 0;
        for (int i = 0; i < a.Length; i++)
        {
            byte x = (byte)(a[i] ^ b[i]);
            int bitCount = CountSetBits(x);
            totalDistance += bitCount;
            trace.Detail($"{a[i]:X2} ^ {b[i]:X2} = {x:X2}  '{x.ToAscii()}' → {bitCount} bits differ total so far: {totalDistance}");
        }
        return totalDistance;
    }

    private static int CountSetBits(byte x)
    {
        int count = 0;
        while (x != 0)
        {
            count += x & 1; // Increment count if the least significant bit is set
            x >>= 1;        // Shift right to check the next bit
        }
        return count;
    }

}
