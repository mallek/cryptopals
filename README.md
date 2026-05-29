# Cryptopals

Working through the [Cryptopals Crypto Challenges](https://cryptopals.com) in C#, to actually *feel* the bytes — no `Convert.ToBase64String` shortcuts where the point is to learn the mechanics.

## Layout

Each challenge is solved as a **test**. The challenge gives a known answer, so the test *is* the deliverable — go green, move on.

```
src/Cryptopals/        # solvers + reusable primitives, organized by set (Set01/ …)
tests/Cryptopals.Tests # one test file per challenge, asserts the known answer
```

## Run

```bash
dotnet test                                       # everything
dotnet test --filter "FullyQualifiedName~Challenge01"   # one challenge
```

## Progress

- [x] Set 1 · Challenge 1 — hex → base64 (hand-rolled)
