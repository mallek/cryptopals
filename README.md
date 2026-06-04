# Cryptopals

Working through the [Cryptopals Crypto Challenges](https://cryptopals.com) in C#, to actually *feel* the bytes — no `Convert.ToBase64String` shortcuts where the point is to learn the mechanics.

## Writeups

Narrative walkthroughs with real trace output: the problem, the idea, the code, and the bytes
moving through it:

- [Set 1: Basics](writeups/Set01/README.md)

## Layout

Each challenge is solved as a **test**. The challenge gives a known answer, so the test *is* the deliverable — go green, move on.

Reusable pieces get extracted out of challenges into library primitives as soon as a second challenge needs them. The challenges are excuses; the library is the product.

```
src/Cryptopals/Codecs/         # Hex, Base64
src/Cryptopals/Ciphers/        # Xor
src/Cryptopals/Scoring/        # EnglishScore, Hamming
src/Cryptopals/Visualization/  # BitFormat, ByteFormat, TraceExtensions
src/Cryptopals/Aes/            # hand-rolled AES-128
src/Cryptopals/Set01/          # challenge solvers — thin compositions of primitives
tests/Cryptopals.Tests         # mirrors src: full suites for primitives, one known-answer test per challenge
```

## Run

```bash
dotnet test                                       # everything
dotnet test --filter "FullyQualifiedName~Challenge01"   # one challenge
dotnet test --logger "console;verbosity=detailed"       # see the bit-level trace output
```

## Progress

- [x] Set 1 · Challenge 1 — hex → base64 (hand-rolled)
- [x] Set 1 · Challenge 2 — fixed XOR
- [x] Set 1 · Challenge 3 — single-byte XOR cipher (frequency scoring)
- [x] Set 1 · Challenge 4 — detect single-character XOR
- [x] Set 1 · Challenge 5 — implement repeating-key XOR
- [x] Set 1 · Challenge 6 — break repeating-key XOR
- [x] Set 1 · Challenge 7 — AES-128 in ECB mode (hand-rolled AES, no library)
- [x] Set 1 · Challenge 8 — detect AES-128-ECB (repeated-block fingerprint)
