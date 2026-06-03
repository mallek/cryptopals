using Cryptopals;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Playground;

// These are VIEWERS, not tests — they have no assertions; you read their output.
// Run with:  dotnet test --filter "Playground" -l "console;verbosity=detailed"
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
        string plaintext = "Burning 'em, if you ain't quick and nimble, I go crazy when I hear a cymbal";
        string key = "ICE";
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
}
