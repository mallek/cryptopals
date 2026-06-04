using Cryptopals;
using Cryptopals.Aes;

namespace Cryptopals.Tests.Aes;

public class AesEcbTests
{
    [Fact]
    public void EncryptThenDecrypt_RoundTrips()
    {
        byte[] plaintext = "Two blocks here Two blocks here!"u8.ToArray();   // 32 bytes = 2 blocks
        byte[] key = "YELLOW SUBMARINE"u8.ToArray();

        AesEcb.Decrypt(AesEcb.Encrypt(plaintext, key), key).Should().Equal(plaintext);
    }

    // THE flaw, made a test: two identical plaintext blocks → two identical ciphertext blocks.
    // This is the leak Challenge 8 detects, and the reason ECB is unsafe.
    [Fact]
    public void IdenticalPlaintextBlocks_ProduceIdenticalCiphertextBlocks()
    {
        byte[] plaintext = "YELLOW SUBMARINEYELLOW SUBMARINE"u8.ToArray();   // same 16 bytes, twice
        byte[] key = "1234567890ABCDEF"u8.ToArray();

        byte[] cipher = AesEcb.Encrypt(plaintext, key);

        cipher[0..16].Should().Equal(cipher[16..32]);   // same input block → same output block
    }

    [Fact]
    public void Encrypt_NonBlockMultipleLength_Throws()
    {
        Action act = () => AesEcb.Encrypt(new byte[20], "YELLOW SUBMARINE"u8.ToArray());
        act.Should().Throw<ArgumentException>();
    }
}
