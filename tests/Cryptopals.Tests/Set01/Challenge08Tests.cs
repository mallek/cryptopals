using Cryptopals.Set01;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Set01;

public class Challenge08Tests
{
    private readonly ITestOutputHelper _output;

    public Challenge08Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Detect_FindsTheEcbLine()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Set01", "08.txt");
        string[] lines = File.ReadAllLines(path);

        EcbDetection result = Challenge08.Detect(lines, _output.WriteLine);

        _output.WriteLine($"line {result.LineNumber}: {result.RepeatedBlocks} repeated block(s)");
        _output.WriteLine(result.Hex);

        // TODO: discovery-then-lock. Run it, confirm exactly ONE line has repeats (the rest
        // should be 0 — random/CBC ciphertexts never repeat a 16-byte block), then pin it:
          result.LineNumber.Should().Be(132);
          result.RepeatedBlocks.Should().BeGreaterThan(0);
    }
}
