using Cryptopals.Scoring;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Scoring;

public class HammingTests
{

    private readonly ITestOutputHelper _output;

    public HammingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Distance_CryptopalsCheck_Is37()
    {
        // The canonical check the challenge gives you, so you KNOW your distance is right
        // before you build the key-length detector on top of it.
        byte[] a = "this is a test"u8.ToArray();
        byte[] b = "wokka wokka!!!"u8.ToArray();
        byte[] c = "does this work"u8.ToArray();
        Hamming.Distance(a, b, _output.WriteLine).Should().Be(37);
        Hamming.Distance(a, c, _output.WriteLine).Should().Be(29);
    }

    [Theory]
    [InlineData(new byte[] { 0b0000 }, new byte[] { 0b0000 }, 0)]   // identical → 0
    [InlineData(new byte[] { 0b0000 }, new byte[] { 0b1111 }, 4)]   // 4 bits differ
    [InlineData(new byte[] { 0x00 }, new byte[] { 0xFF }, 8)]        // all 8 bits differ
    public void Distance_CountsDifferingBits(byte[] a, byte[] b, int expected)
    {
        Hamming.Distance(a, b).Should().Be(expected);
    }

    [Fact]
    public void Distance_UnequalLengths_Throws()
    {
        Action act = () => Hamming.Distance(new byte[] { 0x00 }, new byte[] { 0x00, 0x00 });
        act.Should().Throw<ArgumentException>();
    }
}
