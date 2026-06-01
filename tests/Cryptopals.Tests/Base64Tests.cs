namespace Cryptopals.Tests;

public class Base64Tests
{
    // "Man" → "TWFu" is the canonical example (it's the one on the Wikipedia base64 page).
    // The other two rows are THE point of this file: they exercise the '=' padding path,
    // which until now has never been executed by any test in this project.
    [Theory]
    [InlineData(new byte[] { 0x4D, 0x61, 0x6E }, "TWFu")]   // "Man" — 3 bytes, no padding
    [InlineData(new byte[] { 0x4D, 0x61 }, "TWE=")]          // "Ma"  — 2 bytes, 1 pad char
    [InlineData(new byte[] { 0x4D }, "TQ==")]                // "M"   — 1 byte,  2 pad chars
    public void Encode_ConvertsBytesToBase64Text(byte[] bytes, string expected)
    {
        string actual = Base64.Encode(bytes);
        actual.Should().Be(expected);
    }

    [Fact]
    public void Encode_EmptyArray_ReturnsEmptyString()
    {
        string actual = Base64.Encode(new byte[0]);
        actual.Should().Be("");
    }

    [Fact]
    public void Encode_DoesNotModifyInput()
    {
        // The padding logic copies into a bigger array — make sure it's a copy, not a disguise.
        string hexInput = "4D61"; // "Ma" — 2 bytes, 1 pad char. Exercise the padding logic, but not the trivial no-padding path.
        byte[] bytes = Hex.Decode(hexInput);
        Base64.Encode(bytes);
        bytes.Should().Equal(0x4D, 0x61);
    }

    // Regression anchor: the exact bytes Challenge 1 encodes, pinned to the exact answer
    // cryptopals publishes. If the extraction changes ANY behavior, this catches it.
    [Fact]
    public void Encode_Challenge1Bytes_MatchesKnownAnswer()
    {
        const string hexInput =
            "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d";
        const string expected =
            "SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t";

        string actual = Base64.Encode(Hex.Decode(hexInput));
        actual.Should().Be(expected);
    }
}
