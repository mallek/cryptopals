using Cryptopals.Set01;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Set01;

public class Challenge02Tests
{
    private readonly ITestOutputHelper _output;

    public Challenge02Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void FixedXor_ProducesExpectedResult()
    {
        const string inputA = "1c0111001f010100061a024b53535009181c";
        const string inputB = "686974207468652062756c6c277320657965";
        const string expected = "746865206b696420646f6e277420706c6179";
        
        Challenge02.FixedXor(inputA, inputB, _output.WriteLine).Should().Be(expected);
    }
}
