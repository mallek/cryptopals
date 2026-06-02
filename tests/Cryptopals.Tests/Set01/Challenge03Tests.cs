using Cryptopals;
using Cryptopals.Set01;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Set01;

public class Challenge03Tests
{
    private readonly ITestOutputHelper _output;

    public Challenge03Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Crack_RecoversTheHiddenMessage()
    {
        const string ciphertextHex =
            "1b37373331363f78151b7f2b783431333d78397828372d363c78373e783a393b3736";

        byte[] ciphertext = Hex.Decode(ciphertextHex);
        CrackResult result = Challenge03.Crack(ciphertext, _output.WriteLine);

        // Trace the winner so you can read it the first time through:
        _output.WriteLine($"key  = 0x{result.Key:X2} ('{((byte)result.Key).ToAscii()}')");
        _output.WriteLine($"text = \"{result.Plaintext.ToAscii()}\"");

        // TODO: once you've SEEN the answer, replace these with the real key and text.
        result.Key.Should().Be(0x58);
        result.Plaintext.ToAscii().Should().Be("Cooking MC's like a pound of bacon");
    }

    //Add a regression case: encrypt a known string with key 0xFF, assert you recover it.
    [Fact]
    public void Crack_RecoversKnownMessage()
    {
        byte[] ciphertext = Xor.SingleByte("Hello, World!".ToBytes(), 0xFF);
        CrackResult result = Challenge03.Crack(ciphertext);
        result.Key.Should().Be(0xFF);
        result.Plaintext.ToAscii().Should().Be("Hello, World!");
    }
}
