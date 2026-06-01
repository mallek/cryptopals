namespace Cryptopals.Set01;

/// <summary>
/// Set 1 · Challenge 1 — Convert hex to base64.
/// https://cryptopals.com/sets/1/challenges/1
/// </summary>
public static class Challenge01
{
    /// <summary>
    /// Decode a hex string to its raw bytes, then re-encode those bytes as base64.
    /// Always operate on the raw bytes — never go hex-string -> base64-string directly.
    /// </summary>
    public static string HexToBase64(string hex, Action<string>? trace = null)
    {
        trace?.Invoke($"hex input ({hex.Length} chars): {hex}");
        trace?.Invoke("");
        trace?.Invoke("─── Hex → Bytes ───");

        List<byte> byteList = new List<byte>();

        for (int i = 0; i < hex.Length; i += 2)
        {
            string segment = hex.Substring(i, 2);
            byte b = HexPairToByte(segment, trace);
            byteList.Add(b);
        }

        byte[] bytes = byteList.ToArray();
        trace?.Invoke($"decoded ({bytes.Length} bytes): \"{bytes.ToAscii()}\"");
        trace?.Invoke("");

        string base64 = ConvertBytesToBase64(bytes, trace);
        trace?.Invoke($"result: {base64}");
        return base64;
    }

    private static string ConvertBytesToBase64(byte[] bytes, Action<string>? trace)
    {
        // 1. Calculate how much padding the original input needs to be a multiple of 3 bytes (24 bits)
        int remainder = bytes.Length % 3;
        int paddingCount = (remainder == 0) ? 0 : 3 - remainder;

        // 2. Create a new byte array that includes the original bytes plus the necessary padding bytes (0x00)
        byte[] paddedBytes = new byte[bytes.Length + paddingCount];
        Array.Copy(bytes, paddedBytes, bytes.Length);

        // 3. Process the padded byte array in 3-byte (24-bit) blocks
        List<char> resultChars = new List<char>();
        trace?.Invoke("─── Bytes → Base64 ───");
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


    static byte HexPairToByte(string segment, Action<string>? trace)
    {
        char highChar = segment[0];
        char lowChar = segment[1];

        int highVal = HexCharToInt(highChar);
        int lowVal = HexCharToInt(lowChar);

        // Pure math transformation
        int shiftedHigh = highVal * 16;
        int finalDecimal = shiftedHigh + lowVal;

        // One line per pair: hex → high/low nibbles → byte value → character
        trace?.Invoke($"  {segment} → {highVal.ToBinary(4)} {lowVal.ToBinary(4)} → {finalDecimal,3}  '{((byte)finalDecimal).ToAscii()}'");

        return (byte)finalDecimal;
    }

    static int HexCharToInt(char c)
    {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'A' && c <= 'F') return c - 'A' + 10;
        if (c >= 'a' && c <= 'f') return c - 'a' + 10;
        throw new ArgumentException("Invalid hex character");
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
