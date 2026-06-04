namespace Cryptopals.Aes;

/// <summary>
/// AES-128 key expansion (FIPS-197 §5.2): turns the 16-byte cipher key into 11 round keys
/// (one for the initial AddRoundKey + one per round). Introduces no new math — it reuses
/// SubBytes (via SubWord), a byte rotation (RotWord, like ShiftRows), XTime (for Rcon), and XOR.
///
/// It works in 4-byte WORDS — each word is one column of a round key. 44 words total:
/// words 0–3 are the cipher key; words 4–43 are derived. Every group of 4 words = one round key.
/// </summary>
public static class AesKeySchedule
{
    /// <summary>Expand a 16-byte key into 11 round keys of 16 bytes each (column-major).</summary>
    public static byte[][] Expand(byte[] key, Action<string>? trace = null)
    {
        if (key.Length != 16) throw new ArgumentException("AES-128 key is exactly 16 bytes", nameof(key));

        var words = new byte[44][];
        for (int i = 0; i < 4; i++)
        {
            words[i] = new byte[4];
            Array.Copy(key, i * 4, words[i], 0, 4);
        }
        for (int i = 4; i < 44; i++)
        {
            byte[] temp = (byte[])words[i - 1].Clone();
            if (i % 4 == 0)
            {
                temp = Xor4(SubWord(RotWord(temp)), Rcon(i / 4));
            }
            words[i] = Xor4(words[i - 4], temp);
            trace.Detail($"w[{i}] = w[{i - 4}] XOR {(i % 4 == 0 ? $"SubWord(RotWord(w[{i - 1}])) XOR Rcon({i / 4})" : $"w[{i - 1}]")} = {string.Join(' ', words[i].Select(b => b.ToString("X2")))}");
        }
        var roundKeys = new byte[11][];
        for (int i = 0; i < 11; i++)
        {
            roundKeys[i] = new byte[16];
            for (int j = 0; j < 4; j++)
            {
                Array.Copy(words[i * 4 + j], 0, roundKeys[i], j * 4, 4);
            }
        }
        return roundKeys;
    }

    // RotWord: cyclic left-rotate a 4-byte word by one — [a,b,c,d] → [b,c,d,a]. (Like ShiftRows row 1.)
    static byte[] RotWord(byte[] w)
    {
        var rotArray = new byte[] { w[1], w[2], w[3], w[0] };
        return rotArray;
    }

    // SubWord: S-box each of the 4 bytes (reuses AesSBox). The schedule's only nonlinearity.
    static byte[] SubWord(byte[] w)
    {
        var subArray = new byte[4];
        subArray[0] = AesSBox.Substitute(w[0]);
        subArray[1] = AesSBox.Substitute(w[1]);
        subArray[2] = AesSBox.Substitute(w[2]);
        subArray[3] = AesSBox.Substitute(w[3]);
        return subArray;
    }

    // Rcon(round): the round-constant word [rc, 0, 0, 0], where rc = x^(round-1) in GF(2^8).
    // rc(1) = 0x01, and rc(n+1) = XTime(rc(n)) → 01 02 04 08 10 20 40 80 1B 36 for rounds 1..10.
    static byte[] Rcon(int round)
    {
        byte rc = 0x01;
        for (int i = 1; i < round; i++)
        {
            rc = GaloisField.XTime(rc);
        }
        return [rc, 0, 0, 0];
    }

    // XOR two 4-byte words.
    static byte[] Xor4(byte[] a, byte[] b)
    {
        var xorArray = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            xorArray[i] = (byte)(a[i] ^ b[i]);
        }
        return xorArray;
    }
}
