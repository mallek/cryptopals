# Challenge 7: AES-128 in ECB Mode

> [cryptopals.com/sets/1/challenges/7](https://cryptopals.com/sets/1/challenges/7)
> Solver: [`Set01/Challenge07.cs`](../../src/Cryptopals/Set01/Challenge07.cs)

## The problem

A base64 file was encrypted with AES-128 in ECB mode under the key `YELLOW SUBMARINE`. Decrypt it.
The catch that makes this the heart of the set: **use the AES we built by hand**, not a library. No
`System.Security.Cryptography`. Every byte substitution and field multiplication is ours.

## The idea

The cipher itself, AES-128, we built by hand in a dedicated
[breakout](Aes128-from-scratch.md): the S-box, the GF(2⁸) arithmetic, the four round transforms,
and the key schedule. This challenge is about the *mode* we run it in.

ECB (Electronic Codebook) is the simplest way to use a 16-byte block cipher on a message longer
than one block: chop the message into 16-byte blocks and encrypt each one independently with the
same key. Decryption is the same in reverse. That "each block independently" is both why ECB is
trivial to implement and why it is fatally weak, as the reveal shows.

## The build

ECB mode is a thin loop over 16-byte blocks, handing each to the hand-rolled block cipher
([`Aes/AesEcb.cs`](../../src/Cryptopals/Aes/AesEcb.cs)):

```csharp
for (int i = 0; i < ciphertext.Length; i += BlockSize)
{
    byte[] block = ciphertext.Skip(i).Take(BlockSize).ToArray();
    byte[] decrypted = Aes128.DecryptBlock(block, key, trace: trace);
    plaintext.AddRange(decrypted);
}
```

And the solver is pure composition, three lines
([`Set01/Challenge07.cs`](../../src/Cryptopals/Set01/Challenge07.cs)):

```csharp
byte[] ciphertext = Base64.Decode(base64Ciphertext);
byte[] keyBytes   = ByteFormat.ToBytes(key);
return AesEcb.Decrypt(ciphertext, keyBytes, trace: trace);
```

## Seeing it happen

`AesEcb.Decrypt` runs every 16-byte block back through the hand-rolled cipher. The round-by-round
diffusion inside one block is visualized in the
[AES breakout](Aes128-from-scratch.md#the-avalanche-visualization); here we just confirm the file
decrypts. All 2880 bytes come back as text:

```
(2880 bytes)
I'm back and I'm ringin' the bell ...
```

## The reveal

The plaintext is, once again, Vanilla Ice's *Play That Funky Music*, the same lyrics
[Challenge 6](Challenge06.md) recovered, now delivered by a real block cipher we wrote from the
field arithmetic up. The lasting point sits in the mode, not the cipher: because ECB encrypts every
block **independently**, two identical plaintext blocks always produce identical ciphertext blocks.
That determinism leaks structure straight through the encryption, and detecting that leak with no
key at all is exactly [Challenge 8](Challenge08.md).
