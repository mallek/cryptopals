using Cryptopals.Set01;

namespace Cryptopals.Tests.Set01;

public class Challenge01Tests
{
    [Fact]
    public void HexToBase64_ProducesExpectedEncoding()
    {
        const string input =
            "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d";
        const string expected =
            "SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t";

        Assert.Equal(expected, Challenge01.HexToBase64(input));
    }
}
