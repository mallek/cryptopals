using Cryptopals.Set01;
using Cryptopals.Visualization;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Set01;

public class Challenge07Tests
{
    private readonly ITestOutputHelper _output;

    public Challenge07Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Decrypt_RecoversThePlaintext()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Set01", "07.txt");
        string base64 = string.Concat(File.ReadAllLines(path));

        byte[] plaintext = Challenge07.Decrypt(base64, "YELLOW SUBMARINE");

        _output.WriteLine($"({plaintext.Length} bytes)");
        _output.WriteLine(plaintext.ToAscii());

        // TODO: discovery-then-lock. Run it, read the lyrics, then pin the opening line.
        // (The text ends with a few PKCS#7 padding bytes — Challenge 9 territory. For now,
        //  StartsWith avoids worrying about the trailing padding.)
        plaintext.ToAscii().Should().StartWith("I'm back and I'm ringin' the bell");
    }
}
