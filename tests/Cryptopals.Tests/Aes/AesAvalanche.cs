using Cryptopals.Aes;
using Cryptopals.Scoring;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Aes;

// Viewer (no assertions): the diffusion experiment from day one. Flip ONE plaintext bit,
// encrypt both, and watch the difference spread across the block, round by round.
[Trait("Category", "Viewer")]
public class AesAvalanche
{
    private readonly ITestOutputHelper _output;

    public AesAvalanche(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void OneBitFlip_DiffusesAcrossTheWholeBlock()
    {
        byte[] key = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                       0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
        byte[] p1 = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
                      0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
        byte[] p2 = (byte[])p1.Clone();
        p2[0] ^= 0x01;   // flip exactly ONE bit of the plaintext

        // Capture each block's state after every round via the observer hook.
        var a = new Dictionary<int, byte[]>();
        var b = new Dictionary<int, byte[]>();
        Aes128.EncryptBlock(p1, key, onRound: (r, s) => a[r] = s);
        Aes128.EncryptBlock(p2, key, onRound: (r, s) => b[r] = s);

        _output.WriteLine("Two plaintexts differing by 1 bit. Bits that differ after each round:");
        _output.WriteLine($"{"round",6} {"bits",5} / 128   avalanche");
        _output.WriteLine(new string('─', 56));
        foreach (int r in a.Keys.OrderBy(k => k))
        {
            int bits = Hamming.Distance(a[r], b[r]);   // reuse your Challenge 6 primitive
            _output.WriteLine($"{r,6} {bits,5}        {new string('█', bits)}");
        }

        _output.WriteLine("");
        _output.WriteLine("Spatial view — which of the 16 bytes differ (00 = identical):");
        foreach (int r in new[] { 0, 1, 2, 3 })
            RenderByteDiff($"after round {r}", a[r], b[r]);

        _output.WriteLine("");
        _output.WriteLine("Round 0 = 1 bit (AddRoundKey is XOR — no diffusion). Round 1 explodes it");
        _output.WriteLine("(SubBytes→ShiftRows→MixColumns), and by ~round 3 it settles near 64 — HALF");
        _output.WriteLine("the block. A 1-bit input change flips ~half the output bits: that's the");
        _output.WriteLine("strict avalanche criterion, the property that makes AES look random.");
    }

    // Render which of the 16 state bytes differ between two blocks (column-major grid).
    private void RenderByteDiff(string label, byte[] a, byte[] b)
    {
        _output.WriteLine($"  {label}:");
        for (int r = 0; r < 4; r++)
        {
            string row = "";
            for (int c = 0; c < 4; c++)
                row += $"{(byte)(a[c * 4 + r] ^ b[c * 4 + r]):X2} ";
            _output.WriteLine("    " + row);
        }
    }
}
