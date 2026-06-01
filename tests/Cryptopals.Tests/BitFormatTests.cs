using Cryptopals;

namespace Cryptopals.Tests;

public class BitFormatTests
{

    [Theory]
    [InlineData(73, 8, "01001001")]
    [InlineData(63, 6, "111111")]
    [InlineData(15, 4, "1111")]
    [InlineData(4794221, 24, "010010010010011101101101")]
    public void ToBinary_FormatsValueAtRequestedWidth(int value, int width, string expected)
    {
        string sut = value.ToBinary(width);
        sut.Should().Be(expected);
    }

    [Fact]
    public void ToBinary_ValueTooLargeForWidth_Throws()
    {
        Action act = () => 64.ToBinary(6);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ToBinary_NegativeValue_Throws()
    {
        Action act = () => (-1).ToBinary(8);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ToBinary_ByteOverload_FormatsEightBits()
    {
        byte value = 42;
        string sut = value.ToBinary();
        sut.Should().Be("00101010");
    }
}
