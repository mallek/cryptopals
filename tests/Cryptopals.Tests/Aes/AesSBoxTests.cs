using Cryptopals.Aes;

namespace Cryptopals.Tests.Aes;

public class AesSBoxTests
{
    // Spot-check canonical S-box entries against the FIPS-197 table so we KNOW the table is
    // transcribed correctly before SubBytes is built on top of it.
    [Theory]
    [InlineData(0x00, 0x63)]   // the famous first entry
    [InlineData(0x01, 0x7c)]
    [InlineData(0x53, 0xed)]   // FIPS-197 worked example
    [InlineData(0x10, 0xca)]
    [InlineData(0xff, 0x16)]   // last entry
    public void Substitute_MatchesKnownSBoxValues(byte input, byte expected)
    {
        AesSBox.Substitute(input).Should().Be(expected);
    }

    [Fact]
    public void SBox_IsABijection()
    {
        // Every output appears exactly once — that's what makes it invertible (decryption).
        AesSBox.Forward.Should().HaveCount(256);
        AesSBox.Forward.Distinct().Should().HaveCount(256);
    }
}
