using Cryptopals.Aes;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Aes;

public class AesStateTests
{
    private readonly ITestOutputHelper _output;

    public AesStateTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // The 16 input bytes 00 11 22 .. ff (the FIPS-197 known-answer plaintext) fill the grid
    // COLUMN-MAJOR, so reading it back out must return the exact same bytes (round-trip).
    [Fact]
    public void FromBytes_ToBytes_RoundTrips()
    {
        byte[] input = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
                         0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };

        AesState state = AesState.FromBytes(input);
        state.Render("state", _output.WriteLine);   // eyeball the column-major layout

        state.ToBytes().Should().Equal(input);
    }

    // AddRoundKey is byte-wise XOR. Key = 000102..0f, so 00^00, 11^01, 22^02, ... = 00 10 20 .. f0.
    [Fact]
    public void AddRoundKey_XorsStateWithKey()
    {
        byte[] input = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
                         0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
        byte[] key = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                       0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
        byte[] expected = { 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70,
                            0x80, 0x90, 0xA0, 0xB0, 0xC0, 0xD0, 0xE0, 0xF0 };

        AesState state = AesState.FromBytes(input);
        state.AddRoundKey(key, _output.WriteLine);
        state.Render("after AddRoundKey", _output.WriteLine);

        state.ToBytes().Should().Equal(expected);
    }

    // SubBytes replaces each byte via the S-box. Input 00 10 20 .. f0 (the AddRoundKey result
    // above) → each is an x0 byte, so all come from S-box column 0: 63 ca b7 04 09 53 d0 51 ...
    [Fact]
    public void SubBytes_SubstitutesEveryByteThroughSBox()
    {
        byte[] input = { 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70,
                         0x80, 0x90, 0xA0, 0xB0, 0xC0, 0xD0, 0xE0, 0xF0 };
        byte[] expected = { 0x63, 0xCA, 0xB7, 0x04, 0x09, 0x53, 0xD0, 0x51,
                            0xCD, 0x60, 0xE0, 0xE7, 0xBA, 0x70, 0xE1, 0x8C };

        AesState state = AesState.FromBytes(input);
        state.SubBytes(_output.WriteLine);
        state.Render("after SubBytes", _output.WriteLine);

        state.ToBytes().Should().Equal(expected);
    }

    // ShiftRows rotates each row left by its index. Input 00..0f (grid columns = 0123/4567/...)
    // → the result's first column is the original DIAGONAL 00 05 0a 0f.
    [Fact]
    public void ShiftRows_RotatesEachRowLeftByItsIndex()
    {
        byte[] input = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                         0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
        byte[] expected = { 0x00, 0x05, 0x0A, 0x0F, 0x04, 0x09, 0x0E, 0x03,
                            0x08, 0x0D, 0x02, 0x07, 0x0C, 0x01, 0x06, 0x0B };

        AesState state = AesState.FromBytes(input);
        state.Render("before ShiftRows", _output.WriteLine);
        state.ShiftRows(_output.WriteLine);
        state.Render("after ShiftRows", _output.WriteLine);

        state.ToBytes().Should().Equal(expected);
    }

    // FIPS-197 round-1 worked example: the state AFTER ShiftRows, run through MixColumns.
    // The first column [d4 bf 5d 30] → [04 66 81 e5] is the one every textbook traces.
    [Fact]
    public void MixColumns_MixesEachColumnViaTheMatrix()
    {
        byte[] input = { 0xD4, 0xBF, 0x5D, 0x30, 0xE0, 0xB4, 0x52, 0xAE,
                         0xB8, 0x41, 0x11, 0xF1, 0x1E, 0x27, 0x98, 0xE5 };
        byte[] expected = { 0x04, 0x66, 0x81, 0xE5, 0xE0, 0xCB, 0x19, 0x9A,
                            0x48, 0xF8, 0xD3, 0x7A, 0x28, 0x06, 0x26, 0x4C };

        AesState state = AesState.FromBytes(input);
        state.Render("before MixColumns", _output.WriteLine);
        state.MixColumns(_output.WriteLine);
        state.Render("after MixColumns", _output.WriteLine);

        state.ToBytes().Should().Equal(expected);
    }
}
