using Cryptopals;
using Cryptopals.Set01;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Set01;

public class Challenge06Tests
{
    private readonly ITestOutputHelper _output;

    public Challenge06Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Break_RecoversKeyAndPlaintext()
    {
        // The file is base64, wrapped across many lines — join them, then Base64.Decode.
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "Set01", "06.txt");
        string base64 = string.Concat(File.ReadAllLines(path));
        byte[] ciphertext = Base64.Decode(base64);

        Challenge06Result result = Challenge06.Break(ciphertext, trace: _output.WriteLine);

        // Discovery aids — read these the first time through:
        _output.WriteLine($"key ({result.Key.Length} bytes): \"{result.Key.ToAscii()}\"");
        _output.WriteLine("--- plaintext ---");
        _output.WriteLine(result.Plaintext.ToAscii());

        // TODO: discovery-then-lock. Run it, confirm the key and the plaintext read as English,
        // then pin them:
          result.Key.ToAscii().Should().Be("Terminator X: Bring the noise");
          result.Plaintext.ToAscii().Should().StartWith("I'm back and I'm ringin' the bell");
    }

    // Count how many BITS differ between two bytes (Hamming distance for one byte).
    static int BitsDiffer(byte a, byte b)
    {
        int x = a ^ b, count = 0;
        while (x != 0) { count += x & 1; x >>= 1; }
        return count;
    }

    // Viewer (no assertions): SEE why the right key length has the lowest Hamming distance.
    [Fact]
    public void DemonstrateHammingIntuition()
    {
        _output.WriteLine("Two ENGLISH letters XOR'd — they cluster in ASCII, so FEW bits differ:");
        foreach (var (a, b) in new[] { ('e', 't'), ('h', 'e'), ('o', 'u'), ('a', 'n') })
            _output.WriteLine($"  '{a}' {((byte)a).ToBinary()}   '{b}' {((byte)b).ToBinary()}   " +
                              $"xor {((byte)(a ^ b)).ToBinary()}  → {BitsDiffer((byte)a, (byte)b)} bits");

        _output.WriteLine("");
        _output.WriteLine("Two RANDOM bytes XOR'd — spread across all 8 bits, ~4 differ:");
        byte[] rA = { 0xA3, 0x5C, 0x17, 0xF0, 0x8B, 0x29, 0xDE, 0x44 };
        byte[] rB = { 0x6E, 0xB2, 0xC9, 0x05, 0x3F, 0x91, 0x7A, 0xED };
        for (int i = 0; i < 3; i++)
            _output.WriteLine($"  0x{rA[i]:X2} {rA[i].ToBinary()}   0x{rB[i]:X2} {rB[i].ToBinary()}   " +
                              $"xor {((byte)(rA[i] ^ rB[i])).ToBinary()}  → {BitsDiffer(rA[i], rB[i])} bits");

        byte[] eA = "the quick brown fox jumps over"u8.ToArray();
        byte[] eB = "lazy dogs and clever foxes ran"u8.ToArray();
        double engAvg = 0;
        for (int i = 0; i < eA.Length; i++) engAvg += BitsDiffer(eA[i], eB[i]);
        engAvg /= eA.Length;

        double rndAvg = 0;
        for (int i = 0; i < rA.Length; i++) rndAvg += BitsDiffer(rA[i], rB[i]);
        rndAvg /= rA.Length;

        _output.WriteLine("");
        _output.WriteLine($"average English-vs-English:  {engAvg:F2} bits/byte   ← the SIGNAL");
        _output.WriteLine($"average random-vs-random:    {rndAvg:F2} bits/byte   ← the NOISE");
        _output.WriteLine("");
        _output.WriteLine("Why key length falls out of this:");
        _output.WriteLine("  same key byte:  (P1^K) ^ (P2^K)  = P1^P2           ← key CANCELS (a^a=0) → English^English → LOW");
        _output.WriteLine("  diff key bytes: (P1^Ka)^(P2^Kb)  = P1^P2^(Ka^Kb)   ← key survives → random-ish        → HIGH");
        _output.WriteLine("  → the right key length K is the chunk size whose chunks have the LOWEST Hamming distance");
    }

    [Fact]
    public void Transpose_SimpleExample()
    {
        byte[] data = { 1, 2, 3, 4, 5, 6 };
        byte[][] buckets = Challenge06.Transpose(data, 3, _output.WriteLine);
        buckets[0].Should().Equal(new byte[] { 1, 4 });
        buckets[1].Should().Equal(new byte[] { 2, 5 });
        buckets[2].Should().Equal(new byte[] { 3, 6 });
    }
}
