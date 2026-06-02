namespace Cryptopals.Tests;

public class XorTests
{
    // The three XOR properties from the truth table — each gets its own test.
    // These aren't arbitrary cases; they are the algebra every later attack relies on.

    [Fact]
    public void Apply_DifferenceDetector_OutputIsOneWhereInputsDiffer()
    {
        // 0b1100 ^ 0b1010 = 0b0110 — work out one concrete example by hand and pin it.
        Xor.Apply([0b1100], [0b1010]).Should().Equal(0b0110);
    }

    [Fact]
    public void Apply_AnythingXorItself_IsZero()
    {
        // a ^ a = 0    
        Xor.Apply([0b1100], [0b1100]).Should().Equal(0b0000);
    }

    [Fact]
    public void Apply_AnythingXorZero_IsItself()
    {
        // a ^ 0 = a
        Xor.Apply([0b1100], [0b0000]).Should().Equal(0b1100);
    }

    [Fact]
    public void Apply_TwiceWithSameKey_RecoversOriginal()
    {
        // (a ^ k) ^ k = a — THE property. Encryption and decryption are the same operation.
        Xor.Apply(Xor.Apply([0b1100], [0b1010]), [0b1010]).Should().Equal(0b1100);
    }

    [Fact]
    public void Apply_UnequalLengths_Throws()
    {
        // Your design decision — enforce it loudly.
        Action act = () => Xor.Apply([0b1100], [0b1010, 0b0001]);
        act.Should().Throw<ArgumentException>().WithMessage("*equal length*");
    }

    [Fact]
    public void Apply_DoesNotModifyEitherInput()
    {
        // You know why this test exists. (ToAscii remembers.)
        byte[] a = [0b1100];
        byte[] b = [0b1010];
        Xor.Apply(a, b);
        a.Should().Equal(0b1100);
        b.Should().Equal(0b1010);
    }

    // ───────────────────────────── RepeatingKey ─────────────────────────────

    [Fact]
    public void RepeatingKey_CyclesTheKeyAcrossTheData()
    {
        // 5 bytes of data, 2-byte key → key applies as k0 k1 k0 k1 k0.
        // Pick simple values and work the expected result out by hand.
        byte[] data = { 0x10, 0x20, 0x30, 0x40, 0x50 };
        byte[] key = { 0x01, 0x02 };
        // 0x10^0x01, 0x20^0x02, 0x30^0x01, 0x40^0x02, 0x50^0x01
        Xor.RepeatingKey(data, key).Should().Equal(0x11, 0x22, 0x31, 0x42, 0x51);
    }

    [Fact]
    public void RepeatingKey_WithOneByteKey_MatchesSingleByte()
    {
        // The generalization claim: SingleByte is RepeatingKey with a 1-byte key.
        byte[] data = { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        Xor.RepeatingKey(data, new byte[] { 0x42 })
           .Should().Equal(Xor.SingleByte(data, 0x42));
    }

    [Fact]
    public void RepeatingKey_TwiceWithSameKey_RecoversOriginal()
    {
        // Symmetric, like all XOR: encrypt then encrypt again = back to plaintext.
        byte[] data = "Attack at dawn"u8.ToArray();
        byte[] key = "KEY"u8.ToArray();
        byte[] roundTrip = Xor.RepeatingKey(Xor.RepeatingKey(data, key), key);
        roundTrip.Should().Equal(data);
    }

    [Fact]
    public void RepeatingKey_DoesNotModifyInputs()
    {
        byte[] data = { 0x10, 0x20, 0x30 };
        byte[] key = { 0x01, 0x02 };
        Xor.RepeatingKey(data, key);
        data.Should().Equal(0x10, 0x20, 0x30);
        key.Should().Equal(0x01, 0x02);
    }
}
