using Cryptopals.Aes;
using Xunit.Abstractions;

namespace Cryptopals.Tests.Aes;

public class Aes128Tests
{
    private readonly ITestOutputHelper _output;

    public Aes128Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    // THE canonical AES-128 known-answer vector (FIPS-197 Appendix B / NIST). If this passes,
    // every piece you built — the grid, all four transforms, GF(2⁸), the key schedule, the round
    // structure — is correct, together. This is the moment the whole cipher is verified.
    [Fact]
    public void EncryptBlock_MatchesFipsKnownAnswer()
    {
        byte[] plaintext = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
                             0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
        byte[] key = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                       0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
        byte[] expected = { 0x69, 0xC4, 0xE0, 0xD8, 0x6A, 0x7B, 0x04, 0x30,
                            0xD8, 0xCD, 0xB7, 0x80, 0x70, 0xB4, 0xC5, 0x5A };

        byte[] ciphertext = Aes128.EncryptBlock(plaintext, key, _output.WriteLine);

        ciphertext.Should().Equal(expected);
    }
}
