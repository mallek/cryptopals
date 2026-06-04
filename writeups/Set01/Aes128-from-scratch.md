# Breakout: Building AES-128 from Scratch

> Source: [`src/Cryptopals/Aes/`](../../src/Cryptopals/Aes)
> Sits between [Challenge 6](Challenge06.md) and [Challenge 7](Challenge07.md)

[Challenge 7](Challenge07.md) says to decrypt an AES-128-ECB file "with the code you wrote." Most
people reach for the platform's crypto library. We did the opposite: before touching the challenge,
we **built AES-128 by hand** from the field arithmetic up, and verified it against the FIPS-197 test
vectors. This page is that build. Challenges 7 and 8 are what we do with it afterwards.

## What AES actually is

AES encrypts a fixed **16-byte block** by pushing it through 10 rounds, each round scrambling a 4x4
grid of bytes (the "state"). Two design goals drive everything:

- **Confusion**: make the relationship between key and ciphertext complicated (the S-box).
- **Diffusion**: spread each input bit's influence across the whole block (ShiftRows + MixColumns).

A round is four steps: **SubBytes**, **ShiftRows**, **MixColumns**, **AddRoundKey**. Build those
four plus a key schedule, loop them, and you have AES. Here is each piece, hand-rolled.

## The pieces

### GF(2⁸): the field AES does arithmetic in

AES treats a byte as a polynomial and multiplies bytes in the finite field GF(2⁸). It sounds heavy,
but every operation is shift and XOR. Addition *is* XOR. Multiplication by 2 (`XTime`) is a left
shift, and if the byte overflows past 8 bits you fold it back by XORing `0x1B` (the AES reduction
polynomial) ([`Aes/GaloisField.cs`](../../src/Cryptopals/Aes/GaloisField.cs)):

```csharp
public static byte XTime(byte b)
{
    bool overflow = (b & 0x80) != 0;
    byte shifted = (byte)(b << 1);   // the cast drops the overflowing bit 8
    if (overflow) shifted ^= 0x1B;   // x^8 ≡ x^4+x^3+x+1, folded back into a byte
    return shifted;
}
```

Full multiplication is shift-and-add: walk the bits of one operand, and for each set bit XOR in the
other operand doubled that many times. Binary multiplication, where "+" is XOR and "double" is
`XTime`.

### The S-box: the only nonlinear step

`SubBytes` replaces each byte using a fixed 256-entry table. There is no arithmetic relationship
between input and output, it is a deliberate scramble, and it is the *only* nonlinear component in
all of AES. We store the forward table and **build the inverse from it**, so the two can never
disagree ([`Aes/AesSBox.cs`](../../src/Cryptopals/Aes/AesSBox.cs)):

```csharp
static byte[] BuildInverse()
{
    var inv = new byte[256];
    for (int i = 0; i < 256; i++) inv[Forward[i]] = (byte)i;   // works because Forward is a bijection
    return inv;
}
```

### The state and the four transforms

The 16 input bytes fill a 4x4 grid **column-major** (FIPS-197 §3.4). The four round operations each
act on that grid ([`Aes/AesState.cs`](../../src/Cryptopals/Aes/AesState.cs)):

- **SubBytes**: S-box every cell (confusion). Each byte transforms independently.
- **ShiftRows**: rotate row `r` left by `r` (diffusion, phase 1). Pure permutation: positions move,
  values do not.
- **MixColumns**: replace each column with a fixed GF(2⁸) matrix product (diffusion, phase 2). Every
  output byte of a column depends on all four input bytes of that column.
- **AddRoundKey**: XOR in the round key. The only place the key enters, and it is just our XOR.

Two of these (ShiftRows, MixColumns) read cells they are about to overwrite, so each snapshots its
inputs first. That in-place trap is the same lesson the byte formatters taught earlier in the set.

### The key schedule

`AddRoundKey` needs 11 round keys (one for the initial whitening, one per round). The schedule grows
the 16-byte key into 44 four-byte words, reusing pieces we already had: a byte rotation (`RotWord`,
like ShiftRows), the S-box (`SubWord`), a round constant from `XTime` (`Rcon`), and XOR
([`Aes/AesKeySchedule.cs`](../../src/Cryptopals/Aes/AesKeySchedule.cs)). No new math, just
recombination.

### The round loop

`Aes128` wires the four transforms into the FIPS-197 round structure. Note the two quirks: round 0
is just key whitening, and the *real* final round (10) skips MixColumns
([`Aes/Aes128.cs`](../../src/Cryptopals/Aes/Aes128.cs)):

```csharp
state.AddRoundKey(roundKeys[0]);                  // round 0: initial whitening
for (int round = 1; round <= rounds; round++)
{
    state.SubBytes();
    state.ShiftRows();
    if (round < 10) state.MixColumns();           // the true final round skips MixColumns
    state.AddRoundKey(roundKeys[round]);
}
```

The `rounds` parameter (0 to 10) is a deliberate knob: running fewer rounds gives a "less encrypted"
block, which is exactly what makes the next section possible. Decryption is this loop in reverse,
using the inverse transforms, and `DecryptBlock(EncryptBlock(p, k, N), k, N) == p` for every round
count `N`.

## The avalanche visualization

Here is the property that makes AES *look* random: the **strict avalanche criterion**. Flip a single
bit of the plaintext, and about half the output bits should flip. The
[`AesAvalanche`](../../tests/Cryptopals.Tests/Aes/AesAvalanche.cs) viewer makes it visible. It
encrypts two plaintexts that differ by one bit, capturing the state after each round, and measures
how many bits differ using the **exact Hamming distance primitive from [Challenge 6](Challenge06.md)**:

```
Two plaintexts differing by 1 bit. Bits that differ after each round:
 round  bits / 128   avalanche
 ────────────────────────────────────────────────────────
     0     1        █
     1    17        █████████████████
     2    64        ████████████████████████████████████████████████████████████████
     3    65        █████████████████████████████████████████████████████████████████
     4    64        ████████████████████████████████████████████████████████████████
```

One bit becomes 17 after round 1, then settles near 64, half the 128-bit block, and stays there. A
spatial view of *which* of the 16 bytes differ tells the same story: the difference starts as one
byte and floods the grid within a couple of rounds (`00` means that byte is identical in both):

```
after round 0:        after round 1:        after round 2:
  01 00 00 00           3E 00 00 00           1C 46 BC F2
  00 00 00 00           1F 00 00 00           0E 46 DF 55
  00 00 00 00           1F 00 00 00           0E CA 63 A7
  00 00 00 00           21 00 00 00           12 8C BC A7
```

Round 0 leaves the lone flipped byte alone, because `AddRoundKey` is just XOR and does not diffuse.
Round 1 (`SubBytes` then `ShiftRows` then `MixColumns`) explodes it down the first column, and by
round 2 it has spread across all 16 bytes. That spreading is diffusion working exactly as designed,
and it is why a one-bit change produces a completely different-looking ciphertext.

## Where this goes

This is a complete, FIPS-197-verified AES-128: encrypt and decrypt, full or reduced rounds, every
piece hand-rolled. With the cipher built, the challenges become thin:
[Challenge 7](Challenge07.md) wraps it in ECB mode to decrypt a file, and
[Challenge 8](Challenge08.md) detects ECB's fatal determinism without using the key at all. The
reduced-rounds knob and the avalanche rig also set up Set 2, where chosen-plaintext attacks probe
the cipher's structure directly.
