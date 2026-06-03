namespace Cryptopals.Set01;

/// <summary>
/// Set 1 · Challenge 6 — Break repeating-key XOR.
/// https://cryptopals.com/sets/1/challenges/6
///
/// Recover the key (length unknown) and plaintext from ciphertext alone:
///   Phase 1  guess the key length   — Hamming distance between ciphertext chunks, normalized
///   Phase 2  recover the key        — transpose into N single-byte-XOR buckets, Crack each
///   Phase 3  decrypt                — RepeatingKey-decrypt with the recovered key
/// </summary>
public static class Challenge06
{
    public static Challenge06Result Break(byte[] ciphertext, int minKeyLen = 2, int maxKeyLen = 40,
                                          Action<string>? trace = null)
    {
        // Phase 1 — find the key length.
        int keyLength = GuessKeyLength(ciphertext, minKeyLen, maxKeyLen, trace);

        // Phase 2 — recover one key byte per transposed bucket.
        byte[][] buckets = Transpose(ciphertext, keyLength, trace);
        byte[] key = new byte[keyLength];

        for (int b = 0; b < buckets.Length; b++)
        {
            key[b] = Challenge03.Crack(buckets[b], null).Key;
            trace.Line($"recovered key byte {b}: 0x{key[b]:X2} ('{key[b].ToAscii()}')");
        }

        // Phase 3 — decrypt the whole thing with the recovered key.
        byte[] plaintext = Xor.RepeatingKey(ciphertext, key, trace);
        return new Challenge06Result(key, plaintext);
    }

    /// <summary>
    /// Phase 1: the key length whose ciphertext chunks have the lowest NORMALIZED Hamming
    /// distance (bits per byte). Average several chunk-pairs per candidate to cut noise.
    /// </summary>
    static int GuessKeyLength(byte[] ciphertext, int minKeyLen, int maxKeyLen, Action<string>? trace)
   {
        int bestKeyLen = minKeyLen;
        double bestDistance = double.PositiveInfinity;

        foreach (int keyLen in Enumerable.Range(minKeyLen, maxKeyLen - minKeyLen + 1))
        {
            // Guard against key lengths longer than the ciphertext:
            if (keyLen * 2 > ciphertext.Length)
            {
                trace.Line($"key length {keyLen} is too long for the ciphertext, skipping");
                continue;
            }

            // Take several adjacent chunks of size keyLen:
            var chunks = Enumerable.Range(0, ciphertext.Length / keyLen)
                                   .Select(i => ciphertext.Skip(i * keyLen).Take(keyLen).ToArray())
                                   .ToArray();

            // Hamming distance between pairs, normalized by keyLen:
            var pairs = chunks.Zip(chunks.Skip(1), (a, b) => (a, b));
            double averageDistance = pairs.Average(pair => Hamming.Distance(pair.a, pair.b) / (double)keyLen);

            trace.Line($"key length {keyLen}: average normalized Hamming distance = {averageDistance:F4}");
            if (averageDistance < bestDistance)
            {
                bestDistance = averageDistance;
                bestKeyLen = keyLen;
            }
        }

        trace.Line($"best key length: {bestKeyLen} (distance: {bestDistance:F4})");
        return bestKeyLen;
    }

    /// <summary>
    /// Phase 2 setup: split data into <paramref name="keyLength"/> buckets, where bucket i
    /// holds every byte at an index where index % keyLength == i. Each bucket was encrypted
    /// with a single key byte.
    /// </summary>
    public static byte[][] Transpose(byte[] data, int keyLength, Action<string>? trace)
    {
        var buckets = new byte[keyLength][];
        for (int i = 0; i < keyLength; i++)
        {
            buckets[i] = data.Where((_, index) => index % keyLength == i).ToArray();
            trace.Line($"bucket {i} has {buckets[i].Length} bytes");
            trace.Detail($"bucket {i} bytes: {string.Join(", ", buckets[i].Select(b => $"0x{b:X2}"))}");
        }
        return buckets;
    }
}

/// <summary>The recovered key and the decrypted plaintext.</summary>
public record Challenge06Result(byte[] Key, byte[] Plaintext);
