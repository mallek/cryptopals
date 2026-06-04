namespace Cryptopals.Aes;

/// <summary>
/// AES-128 block cipher — the four transforms wired into the FIPS-197 round structure.
/// No new math here: this is pure assembly of AesState, AesSBox, GaloisField, and AesKeySchedule.
/// </summary>
public static class Aes128
{
    /// <summary>
    /// Encrypt one 16-byte block under a 16-byte key. Returns the 16-byte ciphertext.
    /// </summary>
    public static byte[] EncryptBlock(byte[] plaintext, byte[] key, Action<string>? trace = null)
    {
        if (plaintext.Length != 16) throw new ArgumentException("Block is exactly 16 bytes", nameof(plaintext));
        if (key.Length != 16) throw new ArgumentException("AES-128 key is exactly 16 bytes", nameof(key));

        byte[][] roundKeys = AesKeySchedule.Expand(key);
        AesState state = AesState.FromBytes(plaintext);

        state.AddRoundKey(roundKeys[0]);                  // initial whitening
        state.Render("after round 0", trace);

        for (int round = 1; round <= 9; round++)
        {
            state.SubBytes();
            state.ShiftRows();
            state.MixColumns();
            state.AddRoundKey(roundKeys[round]);
            state.Render($"after round {round}", trace);
        }

        state.SubBytes();                                 // final round — NO MixColumns
        state.ShiftRows();
        state.AddRoundKey(roundKeys[10]);
        state.Render("after round 10 (final)", trace);

        return state.ToBytes();
    }
}
