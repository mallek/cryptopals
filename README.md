# Cryptopals

Working through the [Cryptopals Crypto Challenges](https://cryptopals.com) in C#, to actually *feel* the bytes — no `Convert.ToBase64String` shortcuts where the point is to learn the mechanics.

## Layout

Each challenge is solved as a **test**. The challenge gives a known answer, so the test *is* the deliverable — go green, move on.

Reusable pieces get extracted out of challenges into library primitives as soon as a second challenge needs them. The challenges are excuses; the library is the product.

```
src/Cryptopals/        # library primitives — codecs (Hex, Base64), ciphers (Xor), formatters
src/Cryptopals/Set01/  # challenge solvers — thin compositions of primitives
tests/Cryptopals.Tests # mirrors src: full suites for primitives, one known-answer test per challenge
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
