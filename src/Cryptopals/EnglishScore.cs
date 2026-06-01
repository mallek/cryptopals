namespace Cryptopals;

/// <summary>
/// Scores how much a byte buffer "looks like English text".
/// This is the heart of Challenge 3: it turns your human ability to recognize
/// English into a single comparable number, so a program can rank 256 candidates.
///
/// Design is YOURS to decide (your answer to question 3). Some directions:
///   - count printable / letter / space characters
///   - penalize unprintable bytes
///   - weight by letter-frequency (ETAOIN SHRDLU...) for a sharper signal
/// Whatever you choose, decide the DIRECTION and document it: does a higher score
/// mean "more English" or "less"? The solver depends on that contract.
/// </summary>
public static class EnglishScore
{
    /// <summary>
    /// Return a number representing how English-like the bytes are.
    /// </summary>
    public static double Score(byte[] bytes, Action<string>? trace = null)
    {
        // TODO: this is question 3. Start as simple as you can get away with —
        // you can always sharpen it later if it can't tell two candidates apart.
        throw new NotImplementedException();
    }
}
