# Challenge 1: Convert Hex to Base64

> [cryptopals.com/sets/1/challenges/1](https://cryptopals.com/sets/1/challenges/1)
> Solver: [`Set01/Challenge01.cs`](../../src/Cryptopals/Set01/Challenge01.cs)

## The problem

Given a hex string, produce its base64 encoding. The challenge spells out the one rule that
matters: **always operate on the raw bytes.** Don't translate hex-text straight to base64-text:
decode hex to bytes, then encode those bytes as base64. Hex and base64 are just two different ways
of *spelling* the same underlying bytes.

## The idea

Both hex and base64 are ways to write binary in printable characters. They differ only in how many
bits each character carries:

- **Hex** packs **4 bits** per character (`0`–`f` = 16 values = 2⁴). Two hex chars = one 8-bit byte.
- **Base64** packs **6 bits** per character (`A`–`Z`, `a`–`z`, `0`–`9`, `+`, `/` = 64 values = 2⁶).

The friction is that a byte is 8 bits and a base64 symbol wants 6. They only line up at the least
common multiple: **lcm(8, 6) = 24 bits = 3 bytes = 4 base64 characters.** So the whole algorithm is
"take bytes three at a time, and re-slice those 24 bits into four 6-bit groups."

```
bytes:   |  byte 0  |  byte 1  |  byte 2  |   (3 × 8 = 24 bits)
         01001001   00100111   01101101
base64:  | c0 | c1 | c2 | c3 |              (4 × 6 = 24 bits)
         010010 010010 011101 101101
```

Same 24 bits, regrouped. That's the entire trick.

## The build

**Step 1: hex → bytes.** Each pair of hex characters is a high nibble and a low nibble; the byte
is `high * 16 + low` ([`Codecs/Hex.cs`](../../src/Cryptopals/Codecs/Hex.cs)):

```csharp
int highVal = HexCharToInt(highChar);
int lowVal  = HexCharToInt(lowChar);
int finalDecimal = highVal * 16 + lowVal;   // pure-math view of (high << 4) | low
return (byte)finalDecimal;
```

**Step 2: bytes → base64.** Pack three bytes into one 24-bit integer, then peel off four 6-bit
values with shift-and-mask (`0x3F` is `00111111`) ([`Codecs/Base64.cs`](../../src/Cryptopals/Codecs/Base64.cs)):

```csharp
int block24Bits = (paddedBytes[i] << 16) + (paddedBytes[i + 1] << 8) + paddedBytes[i + 2];

int c1 = (block24Bits >> 18) & 0x3F;   // top 6 bits
int c2 = (block24Bits >> 12) & 0x3F;
int c3 = (block24Bits >>  6) & 0x3F;
int c4 =  block24Bits        & 0x3F;   // bottom 6 bits
```

Each 6-bit value (0–63) maps to a base64 character via `IntToBase64Char`. The solver itself is just
the two codecs wired together:

```csharp
byte[] bytes  = Hex.Decode(hex, trace);
string base64 = Base64.Encode(bytes, trace);
```

## Seeing it happen

The trace walks every transformation. First the hex string becomes bytes, each pair shown as
`hex → bits → decimal → ASCII`:

```
─── Hex → Bytes ───
  49 → 0100 1001 →  73  'I'
  27 → 0010 0111 →  39  '''
  6d → 0110 1101 → 109  'm'
  20 → 0010 0000 →  32  ' '
  ...
decoded (48 bytes): "I'm killing your brain like a poisonous mushroom"
```

Then the bytes are re-sliced into base64, three-at-a-time. Watch the 24 bits get repacked: the
byte boundaries (`01001001 00100111 01101101`) and the base64 boundaries (`010010 | 010010 |
011101 | 101101`) cut the *same* bitstream in different places:

```
─── Bytes → Base64 ───
Block 0  "I'm"
  bytes    01001001 00100111 01101101   0x49 0x27 0x6D
  packed   010010010010011101101101   (4,794,221)
  sliced   010010 | 010010 | 011101 | 101101
  values       18 |     18 |     29 |     45
  chars         S |      S |      d |      t

Block 4  "you"
  bytes    01111001 01101111 01110101   0x79 0x6F 0x75
  packed   011110010110111101110101   (7,958,389)
  sliced   011110 | 010110 | 111101 | 110101
  values       30 |     22 |     61 |     53
  chars         e |      W |      9 |      1
```

## The reveal

```
result: SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t
```

Decoded along the way, the hex was the ASCII *"I'm killing your brain like a poisonous mushroom"*,
a line that recurs through the challenges. The lasting lesson is the one the challenge insisted on:
**bytes are the substrate; encodings are just clothing.** Every later challenge decodes to bytes
first and does the real work there.
