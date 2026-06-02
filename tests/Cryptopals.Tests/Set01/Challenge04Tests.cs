using Cryptopals;
using Cryptopals.Set01;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Set01;

public class Challenge04Tests
{
    private readonly ITestOutputHelper _output;

    public Challenge04Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Detect_FindsTheEncryptedLine()
    {
        // Data-file convention: files live under Data/<set>/ in the test project and are
        // copied next to the test assembly at build (see the csproj <None Include="Data\**\*">).
        // AppContext.BaseDirectory IS that output folder, so this resolves no matter what
        // directory the test runner launches from — unlike a bare relative path.
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Set01", "04.txt");
        string[] lines = File.ReadAllLines(path);

        Detection winner = Challenge04.Detect(lines, _output.WriteLine);

        // Discovery aid — read this the first time through:
        _output.WriteLine($"line {winner.LineNumber}: key 0x{winner.Result.Key:X2} " +
                          $"score {winner.Result.Score:F2} → \"{winner.Result.Plaintext.ToAscii()}\"");

        winner.Result.Key.Should().Be(0x35);
        winner.Result.Plaintext.ToAscii().Should().Be("Now that the party is jumping·");
    }

    // Viewer (no assertions): crack every line, then dump each line's BEST candidate as ASCII,
    // sorted best-score-first. The top line is the real message; everything below is the
    // "least-bad of 256 garbage decryptions" for a noise line. Watch the score cliff after #1 —
    // that gap is why automated scoring works: the signal stands clear above the noise floor.
    [Fact]
    public void DumpAllLineCandidates()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Set01", "04.txt");
        string[] lines = File.ReadAllLines(path);

        // Data is known-valid here, so decode directly (Hex.IsValid is internal — not
        // visible to this separate test assembly without InternalsVisibleTo).
        var candidates = new List<(int Line, CrackResult Result)>();
        for (int i = 0; i < lines.Length; i++)
            candidates.Add((i, Challenge03.Crack(Hex.Decode(lines[i]), null)));

        foreach (var c in candidates.OrderByDescending(c => c.Result.Score))
            _output.WriteLine($"{c.Result.Score,8:F2}  line {c.Line,3}  key 0x{c.Result.Key:X2}  \"{c.Result.Plaintext.ToAscii()}\"");
    }
}
