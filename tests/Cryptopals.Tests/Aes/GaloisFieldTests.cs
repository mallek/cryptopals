using Cryptopals.Aes;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Aes;

public class GaloisFieldTests
{
    private readonly ITestOutputHelper _output;

    public GaloisFieldTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(0x01, 0x02)]   // no overflow: just << 1
    [InlineData(0x57, 0xAE)]   // 0x57 = 0101.. high bit clear → 0xAE, no reduction
    [InlineData(0x80, 0x1B)]   // overflow: 0x80<<1 = 0x100 → reduces to 0x1B
    [InlineData(0xC0, 0x9B)]   // overflow: (0xC0<<1=0x180 → 0x80) ^ 0x1B = 0x9B
    public void XTime_DoublesInTheField(byte input, byte expected)
    {
        GaloisField.XTime(input, _output.WriteLine).Should().Be(expected);
    }

    [Theory]
    [InlineData(0x00, 0xFF, 0x00)]   // ×0 = 0
    [InlineData(0x01, 0xAB, 0xAB)]   // ×1 = identity
    [InlineData(0x02, 0x80, 0x1B)]   // = xtime(0x80), the overflow case
    [InlineData(0x57, 0x83, 0xC1)]   // THE classic FIPS-197 worked example
    [InlineData(0x57, 0x13, 0xFE)]   // another FIPS-197 example
    [InlineData(0xFF, 0x00, 0x00)]   // ×0 = 0, even for 0xFF
    public void Multiply_MatchesKnownFieldProducts(byte a, byte b, byte expected)
    {
        GaloisField.Multiply(a, b, _output.WriteLine).Should().Be(expected);
    }

    [Fact]
    public void Multiply_IsCommutative()
    {
        // a·b = b·a — a sanity check that the field behaves like multiplication should.
        GaloisField.Multiply(0x57, 0x83).Should().Be(GaloisField.Multiply(0x83, 0x57));
    }
}
