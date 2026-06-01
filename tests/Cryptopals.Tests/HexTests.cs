namespace Cryptopals.Tests;

public class HexTests
{
    // ───────────────────────────── Decode: hex text → bytes ─────────────────────────────

    [Theory]
    [InlineData("49276d", new byte[] { 0x49, 0x27, 0x6D })]
    [InlineData("49276D", new byte[] { 0x49, 0x27, 0x6D })]   // uppercase input must work too
    [InlineData("00", new byte[] { 0x00 })]
    [InlineData("ff", new byte[] { 0xFF })]
    public void Decode_ConvertsHexTextToBytes(string hex, byte[] expected)
    {
        byte[] actual = Hex.Decode(hex);
        actual.Should().Equal(expected);
    }

    [Fact]
    public void Decode_EmptyString_ReturnsEmptyArray()
    {
        byte[] actual = Hex.Decode("");
        actual.Should().Equal(new byte[0]);
    }

    [Fact]
    public void Decode_InvalidCharacter_Throws()
    {
        Action act = () => Hex.Decode("zz");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Decode_OddLength_Throws()
    {
        Action act = () => Hex.Decode("123");
        act.Should().Throw<ArgumentException>().WithMessage("*even*");
        act.Should().Throw<ArgumentException>();
    }

    // ───────────────────────────── Encode: bytes → hex text ─────────────────────────────

    [Theory]
    [InlineData(new byte[] { 0x49, 0x27, 0x6D }, "49276d")]   // lowercase output — challenge expects it
    [InlineData(new byte[] { 0x00 }, "00")]                    // single digit values still take two chars
    [InlineData(new byte[] { 0xFF }, "ff")]
    [InlineData(new byte[] { 0x0A }, "0a")]                    // leading zero must not be dropped
    public void Encode_ConvertsBytesToLowercaseHexText(byte[] bytes, string expected)
    {
        string actual = Hex.Encode(bytes);
        actual.Should().Be(expected);
    }

    [Fact]
    public void Encode_EmptyArray_ReturnsEmptyString()
    {
        string actual = Hex.Encode(new byte[0]);
        actual.Should().Be("");
    }

    // ───────────────────────────── The two directions agree ─────────────────────────────

    [Fact]
    public void DecodeThenEncode_RoundTripsToTheSameString()
    {        
        // Round-trip tests catch bugs that one-directional tests miss: if Decode and Encode
        // make the SAME mistake in opposite directions, each looks right alone but the pair lies.
        // Use Challenge 1's full hex string as the input.
        string input =
            "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d";

        string roundTripped = Hex.Encode(Hex.Decode(input));
        roundTripped.Should().Be(input);
    }
}
