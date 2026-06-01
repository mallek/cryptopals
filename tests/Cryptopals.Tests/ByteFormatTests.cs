using Cryptopals;

namespace Cryptopals.Tests;

public class ByteFormatTests
{
    [Theory]
    [InlineData(new byte[] { 0x49, 0x27, 0x6D }, "I'm")]   // happy path, an old friend
    [InlineData(new byte[] { 0x48, 0x09, 0x69 }, "H·i")]   // control char (tab) substituted
    [InlineData(new byte[] { 0x20 }, " ")]                  // boundary: lowest printable
    [InlineData(new byte[] { 0x7E }, "~")]                  // boundary: highest printable
    [InlineData(new byte[] { 0x1F }, "·")]                  // boundary: one below printable
    [InlineData(new byte[] { 0x7F }, "·")]                  // boundary: one above printable (DEL)
    [InlineData(new byte[] { 0xFF }, "·")]                  // high bytes aren't ASCII
    public void ToAscii_FormatsBytesAsReadableText(byte[] bytes, string expected)
    {
        string sut = bytes.ToAscii();
        sut.Should().Be(expected);
    }

    [Fact]
    public void ToAscii_EmptyArray_ReturnsEmptyString()
    {
        string sut = new byte[0].ToAscii();
        sut.Should().Be("");
    }

    [Fact]
    public void ToAscii_doesnt_modify_original_array()
    {
        byte[] bytes = new byte[] { 0x48, 0x09, 0x69 };   // "H·i" — the middle byte is a tab, which should be substituted
        string sut = bytes.ToAscii();
        sut.Should().Be("H·i");
        bytes.Should().Equal(0x48, 0x09, 0x69);
    }

    // The payoff test: decode Challenge 1's hex input and read the hidden message.
    //
    // Blocked (deliberately): this needs a public hex→bytes method, but hex decoding
    // currently lives as a private helper inside Challenge01. Extracting a public
    // Hex codec is already on the roadmap — Challenge 2 needs it too.
    // [Fact(Skip = "...")] shows as yellow in test results: an acknowledged IOU,
    // visible every run, unlike a TODO comment that vanishes into the codebase.
    [Fact(Skip = "Requires a public hex decoder — extract one when starting Challenge 2")]
    public void ToAscii_Challenge1Bytes_RevealTheHiddenMessage()
    {
        const string hexInput =
            "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d";

        // TODO once hex decoding is public:
        //   bytes = <decode hexInput>
        //   bytes.ToAscii().Should().Be("I'm killing your brain like a poisonous mushroom");
        Assert.Fail("Unblock me by extracting a public hex decoder");
    }
}
