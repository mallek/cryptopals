using Cryptopals.Set01;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Set01;

public class Challenge01Tests
{
    private ITestOutputHelper _output;


    public Challenge01Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void HexToBase64_ProducesExpectedEncoding()
    {
        const string input =
            "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d";
        const string expected =
            "SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t";

        Assert.Equal(expected, Challenge01.HexToBase64(input, _output.WriteLine));
    }
}
