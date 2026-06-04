namespace Cryptopals.Visualization;

public static class BitFormat
{
    public static string ToBinary(this int value, int width)   // 73.ToBinary(8) → "01001001"
    {

        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative");
        if (value >= (1 << width)) throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is too large to fit in {width} bits");

        char[] bits = new char[width];
        for (int i = width - 1; i >= 0; i--)
        {
            bits[width - 1 - i] = (value & (1 << i)) != 0 ? '1' : '0';
        }

        return new string(bits);
    }

    public static string ToBinary(this byte value)
    {
        return ToBinary(value, 8);
    }  
}