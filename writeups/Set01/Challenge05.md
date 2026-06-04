# Challenge 5: Implement Repeating-key XOR

> [cryptopals.com/sets/1/challenges/5](https://cryptopals.com/sets/1/challenges/5)
> Solver: [`Set01/Challenge05.cs`](../../src/Cryptopals/Set01/Challenge05.cs)

## The problem

Encrypt a message under a repeating multi-byte key: the first plaintext byte XORs with the first
key byte, the second with the second, and when the key runs out it wraps back to the start. Return
the ciphertext as hex. This is the Vigenère cipher, done in binary.

## The idea

[Challenge 3](Challenge03.md) used a one-byte key applied to every byte. A repeating key is the
same operation with the key **cycling** across the message. For position `i`, the key byte is
`key[i mod keyLength]`:

```
plaintext:  B  u  r  n  i  n  g  ' 'e  m  ...
key:        I  C  E  I  C  E  I  C  E  I  ...   (the 3-byte key "ICE", wrapping)
            ^^^^^^^^                            position 0,1,2 then back to 0
```

The modulo is the entire trick. Single-byte XOR is just this with a 1-byte key, so the same
primitive covers both.

## The build

The primitive builds a full-length key buffer by cycling the key, then hands off to the same
byte-by-byte `Apply` from [Challenge 2](Challenge02.md)
([`Ciphers/Xor.cs`](../../src/Cryptopals/Ciphers/Xor.cs)):

```csharp
public static byte[] RepeatingKey(byte[] data, byte[] key, Action<string>? trace = null)
{
    if (key.Length == 0) throw new ArgumentException("Key cannot be empty");
    byte[] keyBuffer = new byte[data.Length];
    for (int i = 0; i < data.Length; i++)
        keyBuffer[i] = key[i % key.Length];   // the key cycles
    return Apply(data, keyBuffer, trace);
}
```

The solver is pure composition, text to bytes, repeating-key XOR, hex out
([`Set01/Challenge05.cs`](../../src/Cryptopals/Set01/Challenge05.cs)):

```csharp
byte[] plainTextBytes = ByteFormat.ToBytes(plaintext);
byte[] keyBytes       = ByteFormat.ToBytes(key);
byte[] ciphertextBytes = Xor.RepeatingKey(plainTextBytes, keyBytes, trace);
return Hex.Encode(ciphertextBytes);
```

## Seeing it happen

The trace shows the key `ICE` (`0x49 0x43 0x45`) cycling under the plaintext. Watch the right-hand
key column repeat `49, 43, 45, 49, 43, 45, ...`:

```
  42 ^ 49 = 0B   ('B' ^ 'I')
  75 ^ 43 = 36   ('u' ^ 'C')
  72 ^ 45 = 37   ('r' ^ 'E')
  6E ^ 49 = 27   ('n' ^ 'I')
  69 ^ 43 = 2A   ('i' ^ 'C')
  6E ^ 45 = 2B   ('n' ^ 'E')
  67 ^ 49 = 2E   ('g' ^ 'I')
  20 ^ 43 = 63   (' ' ^ 'C')
  27 ^ 45 = 62   ('\'' ^ 'E')
```

Reading the result bytes off in order (`0B 36 37 27 2A 2B 2E 63 62 ...`) gives the hex ciphertext.

## The reveal

```
0b3637272a2b2e63622c2e69692a23693a2a3c6324202d623d63343c2a262263...
```

That matches the published Challenge 5 answer. The detail that matters for what comes next: a
repeating key turns the message into **`keyLength` interleaved single-byte ciphers**. Every byte at
positions `0, keyLength, 2·keyLength, ...` was encrypted with the same key byte. That regular
structure is exactly the weakness [Challenge 6](Challenge06.md) exploits to break it without the
key.
