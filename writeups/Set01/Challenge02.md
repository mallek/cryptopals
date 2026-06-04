# Challenge 2: Fixed XOR

> [cryptopals.com/sets/1/challenges/2](https://cryptopals.com/sets/1/challenges/2)
> Solver: [`Set01/Challenge02.cs`](../../src/Cryptopals/Set01/Challenge02.cs)

## The problem

Take two equal-length hex buffers, XOR them against each other, and return the result as hex. Small
challenge, enormous payoff: XOR is the operation that every cipher in this library (single-byte,
repeating-key, even the AES round key mixing) is built on.

## The idea

XOR (exclusive-or, `^`) compares two bits and returns `1` only when they differ:

| a | b | a ^ b |
|---|---|-------|
| 0 | 0 |   0   |
| 0 | 1 |   1   |
| 1 | 0 |   1   |
| 1 | 1 |   0   |

Three properties make it the workhorse of symmetric crypto:

- **Self-inverse:** `a ^ a = 0`. A bit XORed with itself cancels.
- **Identity:** `a ^ 0 = a`. XOR with zero changes nothing.
- **Reversible:** if `c = a ^ b`, then `c ^ b = a` and `c ^ a = b`.

That last one is the whole game. XOR a plaintext with a key to encrypt; XOR the ciphertext with the
*same* key to decrypt. Encrypt and decrypt are literally the same operation, which is exactly why
later challenges can attack it. Here in Challenge 2 there's no "key" yet, just two buffers combined,
but the machinery is identical.

## The build

The primitive is a byte-by-byte loop ([`Ciphers/Xor.cs`](../../src/Cryptopals/Ciphers/Xor.cs)):

```csharp
for (int i = 0; i < a.Length; i++)
{
    byte x = (byte)(a[i] ^ b[i]);
    result.Add(x);
}
```

XOR works bit-by-bit, but bytes are just eight bits in parallel, so `a[i] ^ b[i]` does all eight at
once. The solver decodes both hex inputs to bytes, XORs them, and re-encodes
([`Set01/Challenge02.cs`](../../src/Cryptopals/Set01/Challenge02.cs)):

```csharp
byte[] bytesA = Hex.Decode(hexA, trace);
byte[] bytesB = Hex.Decode(hexB, trace);
byte[] xored  = Xor.Apply(bytesA, bytesB, trace);
return Hex.Encode(xored, trace);
```

## Seeing it happen

The two inputs decode to very different-looking buffers: buffer `A` is mostly unprintable control
bytes, buffer `B` is readable text. XORing them column by column produces a *third* readable string:

```
─── A XOR B ───
  1C ^ 68 = 74  't'
  01 ^ 69 = 68  'h'
  11 ^ 74 = 65  'e'
  00 ^ 20 = 20  ' '
  1F ^ 74 = 6B  'k'
  01 ^ 68 = 69  'i'
  01 ^ 65 = 64  'd'
  ...
```

Stacked as text, the relationship is plain: `A` is the "mask" that turns `B` into the result:

```
─── Result ───
a:   "···········KSSP···"
b:   "hit the bull's eye"
xor: "the kid don't play"
```

## The reveal

```
xor: "the kid don't play"
```

Two readable strings related by a hidden third through nothing but `^`. This is the seed of the
attack in **Challenge 3**: if a message is XORed against a single repeated byte, you can guess that
byte, XOR it back out, and check whether what falls out looks like English. XOR's reversibility is
both the cipher and its undoing.
