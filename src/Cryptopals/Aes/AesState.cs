namespace Cryptopals.Aes;

/// <summary>
/// The AES "state": 16 bytes arranged as a 4×4 grid. Every AES transformation operates on
/// this grid. The fill order is COLUMN-MAJOR (FIPS-197 §3.4): the first 4 input bytes are
/// the first column, the next 4 the second column, and so on.
///
///   input b0 b1 b2 b3 b4 ...        grid (row, col):
///                                     b0 b4 b8  b12
///                                     b1 b5 b9  b13
///                                     b2 b6 b10 b14
///                                     b3 b7 b11 b15
/// </summary>
public sealed class AesState
{
    // [row, col]. grid[r, c] holds input byte (c * 4 + r).
    private readonly byte[,] _grid = new byte[4, 4];

    /// <summary>Build a state from 16 bytes, filling COLUMN-MAJOR.</summary>
    public static AesState FromBytes(byte[] bytes)
    {
        if (bytes.Length != 16) throw new ArgumentException("AES state is exactly 16 bytes", nameof(bytes));
        var state = new AesState();
        for (int c = 0; c < 4; c++)
        {
            for (int r = 0; r < 4; r++)
            {
                state._grid[r, c] = bytes[c * 4 + r];
            }
        }
        return state;
    }

    /// <summary>Read the state back out to 16 bytes — the inverse of FromBytes (column-major).</summary>
    public byte[] ToBytes()
    {
        byte[] bytes = new byte[16];
        for (int c = 0; c < 4; c++)
        {
            for (int r = 0; r < 4; r++)            {
                bytes[c * 4 + r] = _grid[r, c];
            }
        }
        return bytes;
    }

    /// <summary>
    /// XOR the state with a 16-byte round key (also taken column-major), in place.
    /// This is the only place the key enters a round — and it's just your XOR.
    /// </summary>
    public void AddRoundKey(byte[] roundKey, Action<string>? trace = null)
    {
        if (roundKey.Length != 16) throw new ArgumentException("Round key is exactly 16 bytes", nameof(roundKey));
        for(int c = 0; c < 4; c++)
        {
            for (int r = 0; r < 4; r++)
            {
                byte before = _grid[r, c];
                byte keyByte = roundKey[c * 4 + r];
                _grid[r, c] ^= keyByte;
                byte after = _grid[r, c];
                trace.Detail($"state[{r}, {c}] {before:X2} ^ {keyByte:X2} = {after:X2}");
            }
        }
    }

    /// <summary>
    /// SubBytes: replace every cell with its S-box substitute — the nonlinear "confusion" step.
    /// Each byte is transformed independently (no neighbours involved).
    /// </summary>
    public void SubBytes(Action<string>? trace = null)
    {
        for(int c = 0; c < 4; c++)
        {
            for (int r = 0; r < 4; r++)
            {
                byte before = _grid[r, c];
                _grid[r, c] = AesSBox.Substitute(before);
                byte after = _grid[r, c];
                trace.Detail($"state[{r}, {c}] {before:X2} → {after:X2}");
            }
        }
    }

    /// <summary>
    /// ShiftRows: cyclically rotate each row LEFT by its row index — the first DIFFUSION step.
    /// Pure permutation: positions change, values don't. (Watch a byte leave its column.)
    /// </summary>
    public void ShiftRows(Action<string>? trace = null)
    {
        // TODO: new grid[r, c] = old grid[r, (c + r) % 4].
        // TRAP: if you write straight into _grid you'll overwrite a byte you still need —
        // snapshot the row (or the whole grid) first. (Same in-place lesson as ToAscii.)
        byte[,] newGrid = new byte[4, 4];
        for (int r = 0; r < 4; r++)        {
            for (int c = 0; c < 4; c++)            {
                newGrid[r, c] = _grid[r, (c + r) % 4];
            }
        }
        for (int r = 0; r < 4; r++)        {
            for (int c = 0; c < 4; c++)            {
                trace.Detail($"state[{r}, {c}] ← state[{r}, {(c + r) % 4}]");
                _grid[r, c] = newGrid[r, c];
            }
        }
    }

    /// <summary>
    /// MixColumns: mix each column via the fixed GF(2⁸) matrix — the second diffusion step.
    /// Every output byte of a column depends on all four input bytes of that column.
    /// </summary>
    public void MixColumns(Action<string>? trace = null)
    {
        // TODO: for each column c, read a0..a3 = grid[0..3, c] into locals FIRST (each output
        // depends on all four inputs — the in-place trap), then write:
        //   grid[0,c] = 2·a0 ⊕ 3·a1 ⊕   a2 ⊕   a3
        //   grid[1,c] =   a0 ⊕ 2·a1 ⊕ 3·a2 ⊕   a3
        //   grid[2,c] =   a0 ⊕   a1 ⊕ 2·a2 ⊕ 3·a3
        //   grid[3,c] = 3·a0 ⊕   a1 ⊕   a2 ⊕ 2·a3
        // using GaloisField.Multiply for ·  and  ^ for ⊕.  (3·x = Multiply(3, x), or XTime(x) ^ x.)
        var localGrid = new byte[4, 4];
            for (int c = 0; c < 4; c++)
            {
                byte a0 = _grid[0, c];
                byte a1 = _grid[1, c];
                byte a2 = _grid[2, c];
                byte a3 = _grid[3, c];

                localGrid[0, c] = (byte)(GaloisField.Multiply(2, a0) ^ GaloisField.Multiply(3, a1) ^ a2 ^ a3);
                localGrid[1, c] = (byte)(a0 ^ GaloisField.Multiply(2, a1) ^ GaloisField.Multiply(3, a2) ^ a3);
                localGrid[2, c] = (byte)(a0 ^ a1 ^ GaloisField.Multiply(2, a2) ^ GaloisField.Multiply(3, a3));
                localGrid[3, c] = (byte)(GaloisField.Multiply(3, a0) ^ a1 ^ a2 ^ GaloisField.Multiply(2, a3));
            }
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                trace.Detail($"state[{r}, {c}] ← mix of column {c}");
                _grid[r, c] = localGrid[r, c];
            }
        }
    }

    /// <summary>Render the 4×4 grid as hex rows (visualization — provided).</summary>
    public void Render(string label, Action<string>? trace)
    {
        trace.Line($"{label}:");
        for (int r = 0; r < 4; r++)
            trace.Detail($"{_grid[r, 0]:X2} {_grid[r, 1]:X2} {_grid[r, 2]:X2} {_grid[r, 3]:X2}");
    }
}
