namespace Cryptopals.Tests;

public class EnglishScoreTests
{
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

        Assert.Fail("TODO: assert english scores BETTER than garbage (direction = your design)");
    }

    [Fact]
    public void RealEnglish_ScoresBetterThanGibberishLetters()
    {
        // The sharper test: both are printable ASCII. Only letter FREQUENCY tells them apart.
        // A naive "count printable chars" scorer will TIE here — which is fine for now, but
        // this test documents the case that pushes you toward frequency weighting later.
        byte[] english = "the quick brown fox"u8.ToArray();
        byte[] gibberish = "zxqj wkvb mfpx qzwk"u8.ToArray();

        Assert.Fail("TODO (may stay red until you add frequency weighting — that's an honest IOU)");
    }
}
