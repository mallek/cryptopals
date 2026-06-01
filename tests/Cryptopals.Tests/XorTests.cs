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
}
