using Cryptopals.Set01;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Set01;

public class Challenge05Tests
{
    private readonly ITestOutputHelper _output;

    public Challenge05Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Encrypt_ProducesExpectedHex()
    {
        // Note the \n joining the two lines — the expected hex was computed with a single
        // LF, not CRLF. (A raw string literal here would smuggle in the platform's newline.)
        const string plaintext =
            "Burning 'em, if you ain't quick and nimble\n" +
            "I go crazy when I hear a cymbal";
        const string key = "ICE";
        const string expected =
            "0b3637272a2b2e63622c2e69692a23693a2a3c6324202d623d63343c2a26226324" +
            "272765272a282b2f20430a652e2c652a3124333a653e2b2027630c692b20283165" +
            "286326302e27282f";

        Challenge05.Encrypt(plaintext, key, _output.WriteLine).Should().Be(expected);
    }
}
