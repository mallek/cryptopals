using Xunit.Abstractions;

namespace Cryptopals.Tests;

public class Base64Tests
{

    private readonly ITestOutputHelper _output;

    public Base64Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    // "Man" → "TWFu" is the canonical example (it's the one on the Wikipedia base64 page).
    // The other two rows are THE point of this file: they exercise the '=' padding path,
    // which until now has never been executed by any test in this project.
    [Theory]
    [InlineData(new byte[] { 0x4D, 0x61, 0x6E }, "TWFu")]   // "Man" — 3 bytes, no padding
    [InlineData(new byte[] { 0x4D, 0x61 }, "TWE=")]          // "Ma"  — 2 bytes, 1 pad char
    [InlineData(new byte[] { 0x4D }, "TQ==")]                // "M"   — 1 byte,  2 pad chars
    public void Encode_ConvertsBytesToBase64Text(byte[] bytes, string expected)
    {
        string actual = Base64.Encode(bytes, _output.WriteLine);
        actual.Should().Be(expected);
    }

    [Fact]
    public void Encode_EmptyArray_ReturnsEmptyString()
    {
        string actual = Base64.Encode(new byte[0]);
        actual.Should().Be("");
    }

    // ───────────────────────────── Decode: base64 text → bytes ─────────────────────────────

    [Theory]
    [InlineData("TWFu", new byte[] { 0x4D, 0x61, 0x6E })]   // "Man" — no padding
    [InlineData("TWE=", new byte[] { 0x4D, 0x61 })]          // "Ma"  — 1 pad → drop 1 byte
    [InlineData("TQ==", new byte[] { 0x4D })]                // "M"   — 2 pad → drop 2 bytes
    public void Decode_ConvertsBase64TextToBytes(string base64, byte[] expected)
    {
        Base64.Decode(base64).Should().Equal(expected);
    }

    [Fact]
    public void Decode_StripsNonBase64Characters()
    {
        // A wrapped file has newlines between lines — Decode must ignore them.
        Base64.Decode("TW\nFu").Should().Equal(0x4D, 0x61, 0x6E);
    }

    [Fact]
    public void DecodeThenEncode_RoundTrips()
    {
        // Round-trip catches paired bugs the one-way tests miss.
        byte[] original = "Any carnal pleasure."u8.ToArray();
        Base64.Decode(Base64.Encode(original)).Should().Equal(original);
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
