namespace Cryptopals;

/// <summary>
/// XOR combination of byte buffers — the fundamental operation every cipher
/// in this library is built on.
/// </summary>
public static class Xor
{
    /// <summary>
    /// XOR two equal-length buffers byte by byte: result[i] = a[i] ^ b[i].
    /// </summary>
    public static byte[] Apply(byte[] a, byte[] b, Action<string>? trace = null)
    {
        if (a.Length != b.Length) throw new ArgumentException("Buffers must be of equal length");
        
        List<byte> result = new List<byte>();
        
        for (int i = 0; i < a.Length; i++)
        {
            byte x = (byte)(a[i] ^ b[i]);
            result.Add(x);
            trace?.Invoke($"a[{i}] {a[i]:X2} XOR b[{i}] {b[i]:X2} = {x:X2}");
        }
        
        return result.ToArray();
    }
}
