namespace Cryptopals;

/// <summary>
/// Hex codec: text representation ↔ raw bytes.
/// Convention: Decode goes toward bytes, Encode goes toward text. (Base64 will follow the same pattern.)
/// </summary>
public static class Hex
{
    /// <summary>
    /// Decode a hex string into its raw bytes. "49276d" → { 0x49, 0x27, 0x6D }
    /// </summary>
    public static byte[] Decode(string hex, Action<string>? trace = null)
    {
       if (hex.Length % 2 != 0) throw new ArgumentException("Hex string must have an even number of characters");

        List<byte> byteList = new List<byte>();

        for (int i = 0; i < hex.Length; i += 2)
        {
            string segment = hex.Substring(i, 2);
            byte b = HexPairToByte(segment, trace);
            byteList.Add(b);
        }

        return byteList.ToArray();
    }

    /// <summary>
    /// Encode raw bytes as a lowercase hex string. { 0x49, 0x27, 0x6D } → "49276d"
    /// </summary>
    public static string Encode(byte[] bytes, Action<string>? trace = null)
    {        
        List<char> charList = new List<char>();
        
        foreach (byte b in bytes)
        {
            // Split byte into high and low nibbles
            int highNibble = (b >> 4) & 0x0F; // Shift right 4 bits to get the high nibble
            int lowNibble = b & 0x0F;         // Mask with 0x0F to get the low nibble

            // Convert nibbles to hex characters
            char highChar = IntToHexChar(highNibble);
            char lowChar = IntToHexChar(lowNibble);

            charList.Add(highChar);
            charList.Add(lowChar);

            // Mirror of Decode's trace, arrows reversed: hex pair ← nibbles ← byte value, ascii
            trace?.Invoke($"  {highChar}{lowChar} ← {highNibble.ToBinary(4)} {lowNibble.ToBinary(4)} ← {b,3}  '{b.ToAscii()}'");
        }
        
        return new string(charList.ToArray());
    }

    private static char IntToHexChar(int nibble)
    {
        if (nibble >= 0 && nibble <= 9) return (char)('0' + nibble);
        if (nibble >= 10 && nibble <= 15) return (char)('a' + (nibble - 10));
        throw new ArgumentException("Invalid nibble value");    
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

}
