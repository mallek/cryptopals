# Challenge 6: Break Repeating-key XOR

> [cryptopals.com/sets/1/challenges/6](https://cryptopals.com/sets/1/challenges/6)
> Solver: [`Set01/Challenge06.cs`](../../src/Cryptopals/Set01/Challenge06.cs)

## The problem

Here is a file of base64 ciphertext. It was encrypted with repeating-key XOR
([Challenge 5](Challenge05.md)), but we are given **neither the key nor its length**. Recover both
the key and the plaintext from the ciphertext alone. This is the centerpiece of Set 1: the first
break that needs a real idea, not just brute force.

## The idea

The whole attack rests on one observation from [Challenge 5](Challenge05.md): a repeating key of
length `N` turns the message into `N` interleaved single-byte ciphers. **If we knew `N`**, we could
split the ciphertext into `N` buckets (every `N`th byte), and each bucket would be a plain
single-byte XOR that [Challenge 3](Challenge03.md) already cracks.

So the only genuinely new problem is finding `N`. The trick is **Hamming distance**, the number of
differing bits between two buffers. XOR two English letters and they differ in only a couple of bits
(ASCII letters cluster together); XOR two random bytes and about half their bits differ. Now look at
what happens when you compare two chunks of ciphertext of size `N`:

```
same key byte:  (P1^K) ^ (P2^K)  = P1^P2           ← key CANCELS (a^a=0) → English^English → LOW
diff key bytes: (P1^Ka)^(P2^Kb)  = P1^P2^(Ka^Kb)   ← key survives → random-ish        → HIGH
```

When the chunk size equals the key length, each position in chunk 1 lines up with the *same* key
byte in chunk 2, so the key cancels and you are left comparing English to English: a **low**
Hamming distance. Any wrong chunk size leaves key material in the comparison, pushing the distance
up. So the right key length is the chunk size whose chunks are closest together.

## The build

Three phases ([`Set01/Challenge06.cs`](../../src/Cryptopals/Set01/Challenge06.cs)):

**Phase 1, guess the key length.** For each candidate length, take adjacent chunks, average their
Hamming distance, and normalize by the length (so long keys are not unfairly penalized). Lowest
wins:

```csharp
var pairs = chunks.Zip(chunks.Skip(1), (a, b) => (a, b));
double averageDistance = pairs.Average(pair => Hamming.Distance(pair.a, pair.b) / (double)keyLen);
```

Hamming distance itself is popcount of the XOR ([`Scoring/Hamming.cs`](../../src/Cryptopals/Scoring/Hamming.cs)):

```csharp
byte x = (byte)(a[i] ^ b[i]);
int bitCount = CountSetBits(x);     // count the 1 bits
totalDistance += bitCount;
```

**Phase 2, recover the key.** `Transpose` splits the ciphertext into `keyLength` buckets (bucket `i`
holds every byte where `index % keyLength == i`), then each bucket is cracked as single-byte XOR
with the exact [Challenge 3](Challenge03.md) cracker:

```csharp
byte[][] buckets = Transpose(ciphertext, keyLength, null);
for (int b = 0; b < buckets.Length; b++)
    key[b] = Challenge03.Crack(buckets[b], null).Key;   // one key byte per bucket
```

**Phase 3, decrypt.** Run repeating-key XOR with the recovered key (encrypt and decrypt are the same
operation).

## Seeing it happen

A tiny viewer test (`DemonstrateHammingIntuition`) makes the signal concrete: English-vs-English
clusters low, random-vs-random spreads high.

```
  'e' 01100101   't' 01110100   xor 00010001  → 2 bits
  'h' 01101000   'e' 01100101   xor 00001101  → 3 bits
  0xA3 10100011   0x6E 01101110   xor 11001101  → 5 bits
  0x5C 01011100   0xB2 10110010   xor 11101110  → 6 bits

average English-vs-English:  2.63 bits/byte   ← the SIGNAL
average random-vs-random:    4.75 bits/byte   ← the NOISE
```

That gap is what Phase 1 detects. Sweeping the candidate lengths, most sit around 3.2 bits/byte, but
**key length 29 dips to 2.76**, right toward the English-vs-English signal:

```
key length 28: average normalized Hamming distance = 3.2893
key length 29: average normalized Hamming distance = 2.7593
key length 30: average normalized Hamming distance = 3.2578
best key length: 29 (distance: 2.7593)
```

Phase 2 then cracks all 29 buckets, and the key spells itself out one byte at a time:

```
bucket  0 → 0x54 'T'  (score  637.26)   key so far: "T"
bucket  1 → 0x65 'e'  (score  716.59)   key so far: "Te"
bucket  2 → 0x72 'r'  (score  612.51)   key so far: "Ter"
  ...
bucket 27 → 0x73 's'  (score  816.72)   key so far: "Terminator X: Bring the nois"
bucket 28 → 0x65 'e'  (score  631.39)   key so far: "Terminator X: Bring the noise"
recovered key (29 bytes): "Terminator X: Bring the noise"
```

## The reveal

```
key (29 bytes): "Terminator X: Bring the noise"
plaintext:      "I'm back and I'm ringin' the bell ..."
```

The plaintext is the full lyrics to Vanilla Ice's *Play That Funky Music*. We recovered a 29-byte
key from ciphertext alone, with no brute force over the key (2^232 possibilities), just two cheap
statistical signals stacked: Hamming distance for the length, letter frequency for each byte.

## The seam: where the break dies

This attack works here, but it has a breaking point, and finding it is worth more than the win.
Phase 2 cracks each bucket with the [Challenge 3](Challenge03.md) frequency scorer, and that scorer
needs **enough text to count letters in**. The make-or-break number is **bytes per bucket**:

```
bytesPerBucket = ciphertextLength / keyLength
```

A long message under a short key gives fat buckets (easy). As the key length grows, the buckets
starve, and at `bytesPerBucket ≈ 1` (key length equal to message length) it is a one-time pad: each
key byte touches exactly one plaintext byte, there is no distribution to measure, and the cipher is
unbreakable in principle.

The repo's [`Playground/`](../../tests/Cryptopals.Tests/Playground) viewers exist to *map* that
boundary. They are not tests (no assertions); they print and you read. We built them to answer
questions the pass/fail tests cannot:

- **`SeamSweep`** fixes one long message, sweeps the key length, and shows the `✓ → ✗` flip as
  buckets starve. That flip is the seam. (It uses `CrackKnownLength`, which is *told* the key length,
  to isolate the bucket seam from the separate problem of detecting the length.)
- **`ScorerShowdown`** pits the [home-grown corpus table](Challenge03.md#our-own-corpus-and-overloading-the-scorer)
  against the standard ETAOIN table on *unseen* text, compared at the margin by key-recovery
  percentage. It answers "which table generalizes better."
- **`PerfectCorpusCeiling` / `TrueCeiling`** score against the plaintext's own letter distribution,
  the theoretical best any single-letter scorer can do. The finding: even a perfect-match table
  cannot crack a near-1-byte bucket. **The floor is set by bucket size, not table quality.** Going
  lower needs a smarter algorithm (bigrams, word structure), not better letter counts.

So the Playground is the lab bench for this whole family of attacks: bring your own message and key
(`Lab.Run` / `MyExperiment`), watch where the break dies, and compare scorers honestly. It is the
natural home for the experiment promised back in [Challenge 4](Challenge04.md): the point where the
single-byte scoring technique finally runs out of data.
