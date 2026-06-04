using Cryptopals.Aes;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Aes;

public class AesKeyScheduleTests
{
    private readonly ITestOutputHelper _output;

    public AesKeyScheduleTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // The FIPS-197 cipher-example key 000102..0f. Its round keys are the canonical worked example.
    private static readonly byte[] Key =
        { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
          0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

    [Fact]
    public void Expand_ProducesElevenRoundKeys()
    {
        AesKeySchedule.Expand(Key).Should().HaveCount(11);
    }

    [Fact]
    public void Expand_RoundKey0_IsTheCipherKey()
    {
        // The first round key is the cipher key itself (the base case of the recurrence).
        AesKeySchedule.Expand(Key)[0].Should().Equal(Key);
    }

    [Fact]
    public void Expand_RoundKey1_MatchesFips()
    {
        // Round key 1 is the FIRST time the nonlinear kick fires — it exercises RotWord,
        // SubWord, Rcon[1], and the recurrence all at once. FIPS-197 value:
        byte[] expected = { 0xD6, 0xAA, 0x74, 0xFD, 0xD2, 0xAF, 0x72, 0xFA,
                            0xDA, 0xA6, 0x78, 0xF1, 0xD6, 0xAB, 0x76, 0xFE };

        AesKeySchedule.Expand(Key, _output.WriteLine)[1].Should().Equal(expected);
    }

    [Fact]
    public void Expand_RoundKey2_MatchesFips()
    {
        // Round 2 forces Rcon to ADVANCE (0x01 → 0x02 via XTime), so this catches a broken Rcon.
        byte[] expected = { 0xB6, 0x92, 0xCF, 0x0B, 0x64, 0x3D, 0xBD, 0xF1,
                            0xBE, 0x9B, 0xC5, 0x00, 0x68, 0x30, 0xB3, 0xFE };

        AesKeySchedule.Expand(Key)[2].Should().Equal(expected);
    }
}
