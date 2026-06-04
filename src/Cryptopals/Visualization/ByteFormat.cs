namespace Cryptopals.Visualization;

public static class ByteFormat
{
    /// <summary>
    /// Render a byte array as ASCII text for visualization.
    /// Printable characters (32–126) appear as themselves; everything else becomes '·'.
    /// The middle dot is deliberately NOT an ASCII character, so it can never be
    /// confused with real output.
    /// </summary>
    public static string ToAscii(this byte[] bytes)
    {
        char[] chars = [.. bytes.Select(b => (b >= ' ' && b <= '~') ? (char)b : '·')];
        return new string(chars);
    }

    public static string ToAscii(this byte b)
    {
        return new byte[] { b }.ToAscii();
    }

    //string to byte array
    public static byte[] ToBytes(this string str)
    {
        return str.Select(c => (byte)c).ToArray();
    }

}
