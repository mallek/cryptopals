using Cryptopals.Scoring;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Scoring;

public class EnglishScoreTests
{
    private readonly ITestOutputHelper _output;

    public EnglishScoreTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // These tests pin the CONTRACT, not the exact numbers — you don't yet know what
    // your scoring formula will output, but you DO know the ordering it must produce.
    // Write them as comparisons between two candidates, so they survive you tweaking
    // the formula later. (Adjust the direction of the comparison to match your design:
    // if lower = more English, flip the > to <.)

    [Fact]
    public void RealEnglish_ScoresBetterThanRandomBytes()
    {
        byte[] english = "Now that the party is jumping"u8.ToArray();
        byte[] garbage = { 0x00, 0xFF, 0x7F, 0x01, 0xFE, 0x80, 0x1B, 0xC3 };

        double englishScore = EnglishScore.Score(english);
        double garbageScore = EnglishScore.Score(garbage);

        englishScore.Should().BeGreaterThan(garbageScore);
    }

    [Fact]
    public void RealEnglish_ScoresBetterThanGibberishLetters()
    {
        byte[] english = "the quick brown fox"u8.ToArray();
        byte[] gibberish = "zxqj wkvb mfpx qzwk"u8.ToArray();

        double englishScore = EnglishScore.Score(english);
        double gibberishScore = EnglishScore.Score(gibberish);

        englishScore.Should().BeGreaterThan(gibberishScore);
    }

    [Fact]
    public void DumpCorpusFrequencies()
    {
        foreach (var kvp in EnglishScore.Frequencies.OrderByDescending(k => k.Value))
            _output.WriteLine($"{kvp.Key}: {kvp.Value:F2}%");
    }
}
