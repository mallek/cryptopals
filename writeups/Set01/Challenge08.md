# Challenge 8: Detect AES-128-ECB

> [cryptopals.com/sets/1/challenges/8](https://cryptopals.com/sets/1/challenges/8)
> Solver: [`Set01/Challenge08.cs`](../../src/Cryptopals/Set01/Challenge08.cs)

## The problem

A file holds ~200 hex-encoded ciphertexts. Exactly one was encrypted with
[our hand-rolled AES-128](Aes128-from-scratch.md) in **ECB** mode; the rest are something else. Find
the ECB line. We get **no key** and we do **no decryption**. The ciphertext alone has to give it
away.

## The idea

This challenge is the payoff of the warning at the end of [Challenge 7](Challenge07.md): ECB
encrypts each 16-byte block independently, so it behaves like a fixed **codebook**. The same 16
plaintext bytes always encrypt to the same 16 ciphertext bytes, every time, anywhere in the message.

So if the underlying plaintext has any repeated 16-byte block (and real data often does: runs of
spaces, repeated structure, padding), those repeats survive into the ciphertext as identical
ciphertext blocks. And the odds of a 128-bit block repeating *by chance* are about 1 in 2¹²⁸, which
is to say never. Therefore **any** repeated 16-byte block is a structural fingerprint of ECB. The
line with repeated blocks is the one.

## The build

For each line: decode the hex, chop into 16-byte blocks, and count repeats as `total blocks −
distinct blocks` (hashing each block by its hex string). Keep the line with the most repeats
([`Set01/Challenge08.cs`](../../src/Cryptopals/Set01/Challenge08.cs)):

```csharp
var blocks = Hex.Decode(hex).Chunk(16).Select(b => Hex.Encode(b)).ToList();
int repeatedBlocks = blocks.Count - blocks.Distinct().Count();
```

No cipher, no key, no field math. Just an equality check on 16-byte chunks. The detection is almost
embarrassingly cheap, which is the whole indictment of ECB.

## Seeing it happen

Only one line has any repeats at all, and the trace lays its blocks out so the fingerprint is
impossible to miss: a single 16-byte value appearing four times in a field of otherwise unique
blocks.

```
─── ECB found: line 132 ───
  block  0: d880619740a8a19b7840a8a31c810a3d
  block  1: 08649af70dc06f4fd5d2d69c744cd283   ← repeats ×4
  block  2: e2dd052f6b641dbf9d11b0348542bb57
  block  3: 08649af70dc06f4fd5d2d69c744cd283   ← repeats ×4
  block  4: 9475c9dfdbc1d46597949d9c7e82bf5a
  block  5: 08649af70dc06f4fd5d2d69c744cd283   ← repeats ×4
  block  6: 97a93eab8d6aecd566489154789a6b03
  block  7: 08649af70dc06f4fd5d2d69c744cd283   ← repeats ×4
  block  8: d403180c98c8f6db1f2a3f9c4040deb0
  block  9: ab51b29933f2c123c58386b06fba186a
```

A companion viewer (`TryYellowSubmarineKey`) drives the point home from the other side: decrypting
line 132 with the wrong key (`YELLOW SUBMARINE`) yields garbage, *but* the repeated ciphertext block
still decrypts to a repeated plaintext block. The block-for-block determinism is baked into ECB
itself, independent of which key was used.

## The reveal

```
ECB line: 132  (block 08649af7... repeats ×4)
```

We pinned the ECB ciphertext without ever decrypting it, on nothing but the visible repetition of a
single block. That is the famous "ECB penguin" reduced to its essence: structure passes straight
through the cipher. And that same determinism is not just a detector, it is an *attack surface*. Set
2 turns this leak into [the byte-at-a-time ECB decryption](https://cryptopals.com/sets/2/challenges/12),
recovering an unknown plaintext one byte at a time by feeding chosen input through an ECB oracle and
watching which blocks collide. Set 1 ends where the real fun begins.
