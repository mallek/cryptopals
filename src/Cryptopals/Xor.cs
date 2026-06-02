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
            trace.Detail($"{a[i]:X2} ^ {b[i]:X2} = {x:X2}  '{x.ToAscii()}'");
        }

        return result.ToArray();
    }

    /// <summary>
    /// XOR every byte of <paramref name="data"/> against a single repeated key byte.
    /// This is both the cipher (encrypt) and its inverse (decrypt) — same operation, same as Apply.
    /// </summary>
    public static byte[] SingleByte(byte[] data, byte key, Action<string>? trace = null)
        => RepeatingKey(data, new[] { key }, trace);   // single-byte XOR is repeating-key with a 1-byte key

    /// <summary>
    /// XOR <paramref name="data"/> against a repeating multi-byte key that cycles across
    /// the message: data[i] ^ key[i mod key.Length]. Encrypt and decrypt are the same call.
    /// SingleByte is just this with a 1-byte key.
    /// </summary>
    public static byte[] RepeatingKey(byte[] data, byte[] key, Action<string>? trace = null)
    {
        // TODO: build the same idea as SingleByte, but the key buffer CYCLES instead of
        // repeating one byte. For position i, which key byte applies? (question 2 — the
        // operator that "wraps around"). Decide what an empty key should do, too.
        if (key.Length == 0) throw new ArgumentException("Key cannot be empty");
            byte[] keyBuffer = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            keyBuffer[i] = key[i % key.Length];
        }
        return Apply(data, keyBuffer, trace);
    }
}
