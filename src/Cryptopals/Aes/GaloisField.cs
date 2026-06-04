namespace Cryptopals.Aes;

/// <summary>
/// GF(2⁸) arithmetic — the finite field AES mixes bytes in. A byte is a polynomial (its bits
/// are coefficients); addition is XOR; multiplication is polynomial multiply REDUCED modulo the
/// AES irreducible polynomial x⁸+x⁴+x³+x+1 (0x11B). This is the math under MixColumns (and the
/// S-box). Despite the heavy name, every operation here is just shift and XOR.
/// </summary>
public static class GaloisField
{
    // When a left-shift overflows past 8 bits, x⁸ ≡ x⁴+x³+x+1 (mod the AES polynomial),
    // and x⁴+x³+x+1 = 0001 1011 = 0x1B — so we XOR 0x1B to fold the overflow back into a byte.
    private const byte Reduce = 0x1B;

    /// <summary>
    /// Multiply by x (i.e., by 2): shift left one bit; if the old high bit was set (overflow),
    /// reduce by XORing 0x1B. The single core operation of the whole field.
    /// </summary>
    public static byte XTime(byte b, Action<string>? trace = null)
    {
        bool overflow = (b & 0x80) != 0;

        trace.Detail($"b       {((int)b).ToBinary(9)}   0x{b:X2}");
        trace.Detail($"b<<1    {(b << 1).ToBinary(9)}   {(overflow ? "← bit 8 overflowed (about to fall off)" : "")}");

        byte shifted = (byte)(b << 1);            // the cast drops bit 8
        trace.Detail($"(byte)  {((int)shifted).ToBinary(9)}   0x{shifted:X2}  ← bit 8 dropped");

        if (overflow)
        {
            shifted ^= Reduce;
            trace.Detail($"^0x1B   {((int)Reduce).ToBinary(9)}   x⁸ ≡ x⁴+x³+x+1, folded back in");
            trace.Detail($"result  {((int)shifted).ToBinary(9)}   0x{shifted:X2}");
        }
        return shifted;
    }

    /// <summary>
    /// Multiply two bytes in GF(2⁸) by shift-and-add: walk the bits of b low→high; for each set
    /// bit, XOR in the running value of a (doubled that many times via XTime). Binary multiply,
    /// but "+" is XOR and "double" is XTime.
    /// </summary>
    public static byte Multiply(byte a, byte b, Action<string>? trace = null)
    {
        byte result = 0;
        byte running = a;
        for (int i = 0; i < 8; i++)
        {
            if ((b & (1 << i)) != 0)            {
                result ^= running;
                trace.Detail($"bit {i} of b is set → add a·2^{i} = {running:X2} to result → {result:X2}");
            }
            else            {
                trace.Detail($"bit {i} of b is not set → no change to result");
            }
            running = XTime(running, trace);
            trace.Detail($"running value for next bit: {running:X2}");
        }
        return result;
    }
}
