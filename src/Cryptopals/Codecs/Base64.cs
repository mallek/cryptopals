namespace Cryptopals.Codecs;

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
            trace.Line($"Block {i / 3}  \"{new[] { paddedBytes[i], paddedBytes[i + 1], paddedBytes[i + 2] }.ToAscii()}\"");
            trace.Detail($"bytes    {paddedBytes[i].ToBinary()} {paddedBytes[i + 1].ToBinary()} {paddedBytes[i + 2].ToBinary()}   0x{paddedBytes[i]:X2} 0x{paddedBytes[i + 1]:X2} 0x{paddedBytes[i + 2]:X2}");
            // Combine 3 bytes into one 24-bit integer
            // First byte shifted left 16 bits, second left 8 bits, third stays as-is
            // Pure math alternative to the bitwise operations
            int block24Bits = (paddedBytes[i] << 16) + (paddedBytes[i + 1] << 8) + paddedBytes[i + 2];
            trace.Detail($"packed   {block24Bits.ToBinary(24)}   ({block24Bits:N0})");

            // Slice into four 6-bit decimal values using shift and mask (0x3F is binary 00111111)
            int c1 = (block24Bits >> 18) & 0x3F;
            int c2 = (block24Bits >> 12) & 0x3F;
            int c3 = (block24Bits >> 6) & 0x3F;
            int c4 = block24Bits & 0x3F;
            trace.Detail($"sliced   {c1.ToBinary(6)} | {c2.ToBinary(6)} | {c3.ToBinary(6)} | {c4.ToBinary(6)}");
            trace.Detail($"values   {c1,6} | {c2,6} | {c3,6} | {c4,6}");

            // Convert the 6-bit decimals to Base64 characters using your helper method
            resultChars.Add(IntToBase64Char(c1));
            resultChars.Add(IntToBase64Char(c2));
            resultChars.Add(IntToBase64Char(c3));
            resultChars.Add(IntToBase64Char(c4));
            trace.Detail($"chars    {resultChars[resultChars.Count - 4],6} | {resultChars[resultChars.Count - 3],6} | {resultChars[resultChars.Count - 2],6} | {resultChars[resultChars.Count - 1],6}");

            // 5. Overwrite the trailing characters with '=' padding if the original input wasn't a multiple of 3
            if (i + 3 > bytes.Length) // If we added padding bytes
            {
                trace.Detail($"padding  last {paddingCount} char(s) overwritten with '='");
                for (int j = 0; j < paddingCount; j++)
                {
                    resultChars[resultChars.Count - 1 - j] = '=';
                }
            }
            trace.Line();
        }
        return new string(resultChars.ToArray());
    }

    /// <summary>
    /// Decode base64 text back to raw bytes. "TWFu" → { 0x4D, 0x61, 0x6E }
    /// The mirror of Encode: char → 6-bit value, repack four 6-bit values into three bytes.
    /// </summary>
    public static byte[] Decode(string base64, Action<string>? trace = null)
    {
        var base64string = new string(base64.Where(IsBase64Char).ToArray());

        //make byte arrat the correct length: each 4 chars represent 3 bytes, but we need to account for padding chars which don't produce bytes
        int paddingCount = 0;
        for (int i = base64string.Length - 1; i >= 0 && base64string[i] == '='; i--)
        {
            paddingCount++;
        }
        byte[] decodedBytes = new byte[(base64string.Length * 6) / 8 - paddingCount]; // Each base64 char represents 6 bits, so total bits is length * 6, and we want bytes (8 bits)

        for(int i = 0; i < base64string.Length; i += 4)
        {
            int Value(char ch) => ch == '=' ? 0 : Base64CharToInt(ch);
            trace.Line($"chars    {base64string[i],6} | {base64string[i + 1],6} | {base64string[i + 2],6} | {base64string[i + 3],6}");
            int c1 = Value(base64string[i]);
            int c2 = Value(base64string[i + 1]);
            int c3 = Value(base64string[i + 2]);
            int c4 = Value(base64string[i + 3]);
            trace.Detail($"values   {c1,6} | {c2,6} | {c3,6} | {c4,6}");
            int block24Bits = (c1 << 18) + (c2 << 12) + (c3 << 6) + c4;
            trace.Detail($"packed   {block24Bits.ToBinary(24)}   ({block24Bits:N0})");
            byte b1 = (byte)((block24Bits >> 16) & 0xFF);
            byte b2 = (byte)((block24Bits >> 8) & 0xFF);
            byte b3 = (byte)(block24Bits & 0xFF);
            trace.Detail($"bytes    {b1.ToBinary()} {b2.ToBinary()} {b3.ToBinary()}   0x{b1:X2} 0x{b2:X2} 0x{b3:X2}");
            trace.Line($"block {i / 4}");
            decodedBytes[(i / 4) * 3] = b1;
            if (base64string[i + 2] != '=') decodedBytes[(i / 4) * 3 + 1] = b2; // If not padding, write second byte
            if (base64string[i + 3] != '=') decodedBytes[(i / 4) * 3 + 2] = b3; // If not padding, write third byte
        }

        return decodedBytes;

    }

    private static bool IsBase64Char(char arg)
    {
        return (arg >= 'A' && arg <= 'Z') ||
               (arg >= 'a' && arg <= 'z') ||
               (arg >= '0' && arg <= '9') ||
               arg == '+' || arg == '/' || arg == '=';
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

    // Reverse of IntToBase64Char: a base64 character back to its 6-bit value (0–63).
    static int Base64CharToInt(char c)
    {
        if (c >= 'A' && c <= 'Z') return c - 'A';
        if (c >= 'a' && c <= 'z') return c - 'a' + 26;
        if (c >= '0' && c <= '9') return c - '0' + 52;
        if (c == '+') return 62;
        if (c == '/') return 63;
        throw new ArgumentException($"Invalid base64 character: '{c}'");
    }
}
