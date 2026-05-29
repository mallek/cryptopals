using System.Diagnostics;

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
    public static string HexToBase64(string hex)
    {
        List<byte> byteList = new List<byte>();

        for (int i = 0; i < hex.Length; i += 2)
        {
            string segment = hex.Substring(i, 2);
            byte b = HexPairToByte(segment);
            byteList.Add(b);
        }

        byte[] bytes = byteList.ToArray();
        return ConvertBytesToBase64(bytes);
    }

    private static string ConvertBytesToBase64(byte[] bytes)
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
            // Combine 3 bytes into one 24-bit integer
            // First byte shifted left 16 bits, second left 8 bits, third stays as-is
            int block24Bits = (paddedBytes[i] << 16) | (paddedBytes[i + 1] << 8) | paddedBytes[i + 2];

            // Slice into four 6-bit decimal values using shift and mask (0x3F is binary 00111111)
            int c1 = (block24Bits >> 18) & 0x3F;
            int c2 = (block24Bits >> 12) & 0x3F;
            int c3 = (block24Bits >> 6) & 0x3F;
            int c4 = block24Bits & 0x3F;

            // Convert the 6-bit decimals to Base64 characters using your helper method
            resultChars.Add(IntToBase64Char(c1));
            resultChars.Add(IntToBase64Char(c2));
            resultChars.Add(IntToBase64Char(c3));
            resultChars.Add(IntToBase64Char(c4));

            // 5. Overwrite the trailing characters with '=' padding if the original input wasn't a multiple of 3
            if (i + 3 > bytes.Length) // If we added padding bytes
            {
                for (int j = 0; j < paddingCount; j++)
                {
                    resultChars[resultChars.Count - 1 - j] = '=';
                }
            }
        }
        return new string(resultChars.ToArray());
    }


    static byte HexPairToByte(string segment)
    {
        char highChar = segment[0];
        char lowChar = segment[1];

        int highVal = HexCharToInt(highChar);
        int lowVal = HexCharToInt(lowChar);

        // Pure math transformation
        int shiftedHigh = highVal * 16;
        int finalDecimal = shiftedHigh + lowVal;

        Debug.WriteLine($"--- Hex Segment: {segment} ---");
        Debug.WriteLine($"High: '{highChar}' -> {highVal,2} -> Binary: {IntTo4BitBinary(highVal)}");
        Debug.WriteLine($"Math: {highVal} * 16   -> {shiftedHigh,2} -> Binary: {IntTo8BitBinary(shiftedHigh)} (Shifted left 4 bits)");
        Debug.WriteLine($"Low:  '{lowChar}' -> {lowVal,2} -> Binary: {IntTo4BitBinary(lowVal)}");
        Debug.WriteLine($"Math: {shiftedHigh} + {lowVal}  -> {finalDecimal,2} -> Binary: {IntTo8BitBinary(finalDecimal)}");

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

    static string IntTo4BitBinary(int val)
    {
        char[] bits = new char[4];
        for (int i = 3; i >= 0; i--)
        {
            bits[3 - i] = (val & (1 << i)) != 0 ? '1' : '0';
        }
        return new string(bits);
    }

    static string IntTo8BitBinary(int val)
    {
        char[] bits = new char[8];
        for (int i = 7; i >= 0; i--)
        {
            bits[7 - i] = (val & (1 << i)) != 0 ? '1' : '0';
        }
        return new string(bits);
    }
}
