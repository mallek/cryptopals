namespace Cryptopals;

/// <summary>
/// Base64 codec: raw bytes ↔ base64 text.
/// Same convention as Hex: Encode goes toward text, Decode goes toward bytes.
/// (Decode arrives with Challenge 6.)
/// </summary>
public static class Base64
{
    /// <summary>
    /// Encode raw bytes as base64 text. { 0x4D, 0x61, 0x6E } → "TWFu"
    /// </summary>
    public static string Encode(byte[] bytes, Action<string>? trace = null)
    {
        // 1. Calculate how much padding the original input needs to be a multiple of 3 bytes (24 bits)
        int remainder = bytes.Length % 3;
        int paddingCount = (remainder == 0) ? 0 : 3 - remainder;

        // 2. Create a new byte array that includes the original bytes plus the necessary padding bytes (0x00)
        byte[] paddedBytes = new byte[bytes.Length + paddingCount];
        Array.Copy(bytes, paddedBytes, bytes.Length);

        // 3. Process the padded byte array in 3-byte (24-bit) blocks
        List<char> resultChars = new List<char>();
        for (int i = 0; i < paddedBytes.Length; i += 3)
        {
            trace?.Invoke($"Block {i / 3}  \"{new[] { paddedBytes[i], paddedBytes[i + 1], paddedBytes[i + 2] }.ToAscii()}\"");
            trace?.Invoke($"  bytes    {paddedBytes[i].ToBinary()} {paddedBytes[i + 1].ToBinary()} {paddedBytes[i + 2].ToBinary()}   0x{paddedBytes[i]:X2} 0x{paddedBytes[i + 1]:X2} 0x{paddedBytes[i + 2]:X2}");
            // Combine 3 bytes into one 24-bit integer
            // First byte shifted left 16 bits, second left 8 bits, third stays as-is
            // Pure math alternative to the bitwise operations
            int block24Bits = (paddedBytes[i] << 16) + (paddedBytes[i + 1] << 8) + paddedBytes[i + 2];
            trace?.Invoke($"  packed   {block24Bits.ToBinary(24)}   ({block24Bits:N0})");

            // Slice into four 6-bit decimal values using shift and mask (0x3F is binary 00111111)
            int c1 = (block24Bits >> 18) & 0x3F;
            int c2 = (block24Bits >> 12) & 0x3F;
            int c3 = (block24Bits >> 6) & 0x3F;
            int c4 = block24Bits & 0x3F;
            trace?.Invoke($"  sliced   {c1.ToBinary(6)} | {c2.ToBinary(6)} | {c3.ToBinary(6)} | {c4.ToBinary(6)}");
            trace?.Invoke($"  values   {c1,6} | {c2,6} | {c3,6} | {c4,6}");

            // Convert the 6-bit decimals to Base64 characters using your helper method
            resultChars.Add(IntToBase64Char(c1));
            resultChars.Add(IntToBase64Char(c2));
            resultChars.Add(IntToBase64Char(c3));
            resultChars.Add(IntToBase64Char(c4));
            trace?.Invoke($"  chars    {resultChars[resultChars.Count - 4],6} | {resultChars[resultChars.Count - 3],6} | {resultChars[resultChars.Count - 2],6} | {resultChars[resultChars.Count - 1],6}");

            // 5. Overwrite the trailing characters with '=' padding if the original input wasn't a multiple of 3
            if (i + 3 > bytes.Length) // If we added padding bytes
            {
                trace?.Invoke($"  padding  last {paddingCount} char(s) overwritten with '='");
                for (int j = 0; j < paddingCount; j++)
                {
                    resultChars[resultChars.Count - 1 - j] = '=';
                }
            }
            trace?.Invoke("");
        }
        return new string(resultChars.ToArray());
    }

    static char IntToBase64Char(int val)
    {
        if (val >= 0 && val <= 25) return (char)('A' + val);
        if (val >= 26 && val <= 51) return (char)('a' + (val - 26));
        if (val >= 52 && val <= 61) return (char)('0' + (val - 52));
        if (val == 62) return '+';
        if (val == 63) return '/';
        throw new ArgumentException("Invalid base64 value");
    }
}
