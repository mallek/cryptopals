using Cryptopals;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Playground;

// These are VIEWERS, not tests — they have no assertions; you read their output.
// Tagged Viewer so the fast suite skips them; run on demand with:
//   dotnet test --filter "Category=Viewer" -l "console;verbosity=detailed"
[Trait("Category", "Viewer")]
public class Playground
{
    private readonly ITestOutputHelper _output;

    public Playground(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void MyExperiment()
    {
        // ▼▼▼ EDIT THESE — your own message and your own key ▼▼▼
        string plaintext = "Burning 'em, if you ain't quick and nimble, I go crazy when I hear a cymbal or a high hat.. I'm back and I'm ringin' the bell, so I kick on back, maybe a few rounds, all on the house, I'll tell you when it's over, so pour a potion and we'll dive in it, I'll give you something to bump about.."; // must be long enough to break!
        string key = "what if god was one of us?";
        // ▲▲▲ EDIT THESE ▲▲▲

        // Pass _output.WriteLine to watch the full attack (phases + key assembly).
        // Swap to null if you just want the summary below.
        BreakReport r = Lab.Run(plaintext, key.ToBytes(), _output.WriteLine);

        _output.WriteLine("");
        _output.WriteLine("════════════════════════════════════════");
        _output.WriteLine($"  text length      : {r.TextLength}");
        _output.WriteLine($"  actual key       : \"{r.ActualKey}\" ({r.ActualKeyLength} bytes)");
        _output.WriteLine($"  bytes per bucket : {r.BytesPerBucket:F1}");
        _output.WriteLine($"  guessed key len  : {r.GuessedKeyLength}");
        _output.WriteLine($"  recovered key    : \"{r.RecoveredKey}\"");
        _output.WriteLine($"  recovered text   : \"{r.RecoveredText}\"");
        _output.WriteLine($"  SUCCESS          : {(r.Success ? "✓ yes" : "✗ no")}");
        _output.WriteLine("════════════════════════════════════════");
    }

    [Fact]
    public void SeamSweep()
    {
        // Fix one LONG message, sweep the key length, and watch WHERE the break dies as the
        // buckets starve. The message must be long — the attack needs ciphertext many times
        // the key length, or detection itself goes noisy (see the 165-byte cautionary tale).
        // EnglishScore.CorpusText is a handy ~1500-byte non-repetitive English passage.
        string plaintext = EnglishScore.CorpusText;
        byte[] plainBytes = plaintext.ToBytes();
        var rng = new Random(42); // seeded → reproducible

        _output.WriteLine($"text length: {plainBytes.Length}  (attack's natural ceiling is ~{plainBytes.Length / 2}-byte keys)");
        _output.WriteLine("");
        _output.WriteLine($"{"keyLen",6} {"bytes/bucket",13} {"break?",9}");
        _output.WriteLine(new string('─', 32));

        // CrackKnownLength tells the break the right length, so this isolates the FUNDAMENTAL
        // seam: per-bucket cracking. (Detecting the length is a separate, earlier hurdle.)
        int[] keyLengths = { 4, 16, 48, 120, 300, 450, 600, 800, 1000, 1200, plainBytes.Length / 2 };
        foreach (int keyLen in keyLengths.Distinct().Where(k => k >= 2 && k <= plainBytes.Length).OrderBy(k => k))
        {
            byte[] key = new byte[keyLen];
            rng.NextBytes(key);
            BreakReport r = Lab.CrackKnownLength(plaintext, key);
            _output.WriteLine($"{keyLen,6} {r.BytesPerBucket,13:F1} {(r.Success ? "✓" : "✗"),9}");
        }

        _output.WriteLine("");
        _output.WriteLine("(key length is GIVEN here, so this is the pure bucket seam.)");
        _output.WriteLine("Watch the ✓→✗ flip: it happens when bytes/bucket drops too low for the");
        _output.WriteLine("frequency score to find each key byte. That boundary IS the seam —");
        _output.WriteLine("and at bytes/bucket = 1 (key length = message length) it's a one-time pad.");
    }

    [Fact]
    public void ScorerShowdown()
    {
        // Corpus table (measured from EnglishScore.CorpusText) vs the general STANDARD table.
        // Tested on a NEUTRAL passage neither table has seen — testing on the corpus's own
        // text would rig it for the corpus scorer. Fair question: which generalizes better?
        string neutralText = NeutralText;
        byte[] plainBytes = neutralText.ToBytes();
        var rng = new Random(42);

        _output.WriteLine($"text length: {plainBytes.Length}   corpus table vs standard table, on UNSEEN text");
        _output.WriteLine("");
        _output.WriteLine($"{"keyLen",6} {"bytes/bkt",10} {"corpus",8} {"standard",10}");
        _output.WriteLine(new string('─', 40));

        foreach (int keyLen in new[] { 20, 40, 60, 80, 100, 130 })
        {
            if (keyLen >= plainBytes.Length) continue;
            byte[] key = new byte[keyLen];
            rng.NextBytes(key);
            var corpus = Lab.CrackKnownLength(neutralText, key, EnglishScore.ScoreCorpus);
            var standard = Lab.CrackKnownLength(neutralText, key, EnglishScore.ScoreStandard);
            _output.WriteLine($"{keyLen,6} {corpus.BytesPerBucket,10:F1} {corpus.KeyAccuracy,8:P0} {standard.KeyAccuracy,10:P0}");
        }

        _output.WriteLine("");
        _output.WriteLine("% = key bytes recovered correctly. Compare at the MARGIN (where neither is");
        _output.WriteLine("100%) — that gap is the difference in table quality. A table tuned to the");
        _output.WriteLine("message's DOMAIN would win on that domain (your TikTok-vs-Wikipedia idea).");
    }

    [Fact]
    public void PerfectCorpusCeiling()
    {
        // The extreme of domain-matching: score against the EXACT text we're cracking — its
        // own letter distribution. This is the theoretical ceiling of any single-letter
        // scorer (the model literally cannot match the plaintext better than this).
        string target = NeutralText;
        byte[] plainBytes = target.ToBytes();
        var selfTable = EnglishScore.CorpusAnalysis(target);   // the plaintext's own distribution
        Func<byte[], double> selfScorer = b => EnglishScore.Score(b, selfTable);

        var rng = new Random(42);
        _output.WriteLine($"text length: {plainBytes.Length}   corpus / standard / SELF (perfect match)");
        _output.WriteLine("");
        _output.WriteLine($"{"keyLen",6} {"bytes/bkt",10} {"corpus",8} {"standard",10} {"self",7}");
        _output.WriteLine(new string('─', 48));

        foreach (int keyLen in new[] { 60, 80, 100, 130, 160, 200 })
        {
            if (keyLen >= plainBytes.Length) continue;
            byte[] key = new byte[keyLen];
            rng.NextBytes(key);
            var c = Lab.CrackKnownLength(target, key, EnglishScore.ScoreCorpus);
            var s = Lab.CrackKnownLength(target, key, EnglishScore.ScoreStandard);
            var self = Lab.CrackKnownLength(target, key, selfScorer);
            _output.WriteLine($"{keyLen,6} {c.BytesPerBucket,10:F1} {c.KeyAccuracy,8:P0} {s.KeyAccuracy,10:P0} {self.KeyAccuracy,7:P0}");
        }

        _output.WriteLine("");
        _output.WriteLine("SELF scores against the exact text's own distribution — the ceiling of any");
        _output.WriteLine("single-LETTER scorer. Note it STILL can't crack a near-1-byte bucket: a");
        _output.WriteLine("lone byte has no distribution to match. Breaking past that needs word");
        _output.WriteLine("structure (bigrams), not better letter counts.");
    }

    [Fact]
    public void TrueCeiling()
    {
        // Map the FULL accuracy curve as buckets starve, for the best general scorer (standard)
        // AND the "perfect match" self table — on a long text, all the way toward 1 byte/bucket.
        // The question: does a perfect-match table push the floor lower, or is the floor set by
        // BUCKET SIZE regardless of the table?
        string target = EnglishScore.CorpusText;
        byte[] plainBytes = target.ToBytes();
        var selfTable = EnglishScore.CorpusAnalysis(target);
        Func<byte[], double> selfScorer = b => EnglishScore.Score(b, selfTable);
        var rng = new Random(7);

        _output.WriteLine($"text length: {plainBytes.Length}   (random key = 1/256 ≈ 0.4% accuracy is pure chance)");
        _output.WriteLine("");
        _output.WriteLine($"{"keyLen",6} {"bytes/bkt",10} {"standard",9} {"self",7}");
        _output.WriteLine(new string('─', 36));

        int[] lens = { 60, 130, 270, 450, 670, 900, 1200, 1500, plainBytes.Length * 2 / 3 };
        foreach (int keyLen in lens.Where(k => k >= 2 && k < plainBytes.Length).Distinct().OrderBy(k => k))
        {
            byte[] key = new byte[keyLen];
            rng.NextBytes(key);
            var s = Lab.CrackKnownLength(target, key, EnglishScore.ScoreStandard);
            var self = Lab.CrackKnownLength(target, key, selfScorer);
            _output.WriteLine($"{keyLen,6} {s.BytesPerBucket,10:F1} {s.KeyAccuracy,9:P0} {self.KeyAccuracy,7:P0}");
        }

        _output.WriteLine("");
        _output.WriteLine("If 'standard' and 'self' track each other down the whole curve, the floor is");
        _output.WriteLine("set by BUCKET SIZE, not the table — the true ceiling of independent per-bucket");
        _output.WriteLine("single-letter cracking. Going lower needs a smarter ALGORITHM, not a better table.");
    }

    // A neutral English passage neither the corpus nor standard table was built from.
    private const string NeutralText =
        "Across the wide valley the morning fog began to lift, and the slow river caught the " +
        "first pale light of the sun. A heron stood among the reeds, perfectly still, waiting " +
        "for the silver flash of a fish beneath the surface. Far off, a farmer opened the gate " +
        "to his field and the cattle moved out slowly, their breath rising in soft clouds. The " +
        "road beside the water was empty but for a single cart, its wheels creaking under the " +
        "weight of the early harvest. There was a quiet to the hour that felt older than the " +
        "town itself, as though the land remembered a time before the houses had been built.";
}
