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
    /// <param name="rounds">
    /// How many rounds to apply (0–10). 10 = standard AES. Fewer = the same cipher STOPPED
    /// early — a "less encrypted" version, handy for watching diffusion build. rounds=0 is just
    /// the initial whitening (plaintext ⊕ key). MixColumns is skipped only on the true final round.
    /// </param>
    public static byte[] EncryptBlock(byte[] plaintext, byte[] key, int rounds = 10,
                                      Action<string>? trace = null, Action<int, byte[]>? onRound = null)
    {
        if (plaintext.Length != 16) throw new ArgumentException("Block is exactly 16 bytes", nameof(plaintext));
        if (key.Length != 16) throw new ArgumentException("AES-128 key is exactly 16 bytes", nameof(key));
        if (rounds < 0 || rounds > 10) throw new ArgumentOutOfRangeException(nameof(rounds), "AES-128 runs 0–10 rounds");

        byte[][] roundKeys = AesKeySchedule.Expand(key);
        AesState state = AesState.FromBytes(plaintext);

        state.AddRoundKey(roundKeys[0]);                  // initial whitening (this is "round 0")
        state.Render("after round 0", trace);
        onRound?.Invoke(0, state.ToBytes());

        for (int round = 1; round <= rounds; round++)
        {
            state.SubBytes();
            state.ShiftRows();
            if (round < 10) state.MixColumns();           // the REAL final round (10) skips MixColumns
            state.AddRoundKey(roundKeys[round]);
            state.Render($"after round {round}", trace);
            onRound?.Invoke(round, state.ToBytes());
        }

        return state.ToBytes();
    }
}
