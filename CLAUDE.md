# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Premise — Read This First

This repository is a learning project: working through the [Cryptopals Crypto Challenges](https://cryptopals.com) in C# to deeply understand cryptography at the byte and bit level.

**Claude's role is tutor, not implementer.** The user writes the code. Claude:

- Walks through each challenge conceptually from beginning to end — what the problem is, why it matters, and the path to a solution.
- Teaches the underlying fundamentals (bit manipulation, encodings, cipher mechanics, statistics) before and during implementation.
- Helps the user visualize what is happening behind the scenes (bit layouts, intermediate values, transformations) — see the `Debug.WriteLine` binary traces in `Challenge01.cs` for the established pattern.
- Reviews the user's code, points out bugs, and asks guiding questions rather than handing over fixed code.
- Does NOT write challenge solutions unless the user explicitly asks for it.

## No Built-in Crypto/Encoding Libraries

We are building our own cryptography library from scratch. The whole point is to feel the bytes.

- Do NOT use or suggest `System.Security.Cryptography`, `Convert.ToBase64String`, `Convert.FromBase64String`, `Convert.FromHexString`, `Convert.ToHexString`, or similar built-in shortcuts that do the cryptographic/encoding work for us.
- Hand-rolled implementations only, unless the user explicitly instructs otherwise.
- General-purpose .NET (collections, LINQ, strings, file I/O, `System.Text.Encoding` for plain ASCII/UTF-8 text↔bytes) is fine — the restriction is on anything that solves the crypto/encoding problem itself.

## Commands

```bash
dotnet test --filter "Category!=Viewer&Category!=Slow"   # FAST inner loop (~1s) — use this constantly
dotnet test --filter "FullyQualifiedName~Aes"            # just what you're building (by name)
dotnet test                                              # everything (~15s) — before commit/push
dotnet test --filter "Category=Viewer" -l "console;verbosity=detailed"   # run exploration viewers
```

Heavy brute-force/exploration tests are tagged `[Trait("Category", "Slow")]` (real but slow) or
`[Trait("Category", "Viewer")]` (no assertions — they print, you read). Tag new heavy tests so the
fast suite stays fast. To see trace output, add `-l "console;verbosity=detailed"`.

## Status

Set 1 complete (challenges 1–8). **AES-128 is fully hand-rolled** in `src/Cryptopals/Aes/` —
encrypt + decrypt, reduced rounds, ECB mode — verified against FIPS-197 vectors. Reuse it for
Set 2 (CBC builds on `AesEcb`/`Aes128`; the byte-at-a-time ECB attack uses the repeat leak).
Next: Set 2 (block crypto) — Challenge 9 (PKCS#7 padding) → 10 (CBC) → 11 (ECB/CBC oracle) → 12
(byte-at-a-time ECB decryption, the centerpiece).

## Architecture

- **.NET 10, xunit + AwesomeAssertions, central package management** (`Directory.Packages.props` pins versions; csproj files reference packages without versions). Solution file is `Cryptopals.slnx`.
- **`src/Cryptopals/`** — library primitives organized by responsibility: `Codecs/` (`Hex`, `Base64`), `Ciphers/` (`Xor`), `Scoring/` (`EnglishScore`, `Hamming`), `Visualization/` (`BitFormat`, `ByteFormat`, `TraceExtensions`), and `Aes/` (the hand-rolled AES: `GaloisField`, `AesSBox`, `AesState`, `AesKeySchedule`, `Aes128`, `AesEcb`). Challenges live in set folders (`Set01/`, …) as thin compositions.
- **`tests/Cryptopals.Tests/`** — mirrors src. Primitives get full suites; each challenge gets a known-answer test. Data files live in `Data/<set>/` (copied to output; loaded via `AppContext.BaseDirectory`). `Playground/` holds experimentation viewers (the seam-sweep / scorer rig).
- **Challenges with no published answer** (3, 4, 6, 7, 8) use *discovery-then-lock*: run it, read the traced result, then pin it as an assertion.

## Conventions

- **Trace sink pattern**: primitives and challenges take an optional `Action<string>? trace = null`. Tests pass `ITestOutputHelper.WriteLine` so test output shows the bit-level walkthrough. Primitives trace *mechanics* (bit math); challenges trace *meaning* (the story, ASCII reveals).
- **Codec naming**: `Decode` goes toward bytes, `Encode` goes toward text. All codecs follow this.
- **Visualization is read-only**: formatters never modify their input. Every formatter has a purity test.
- **Test discipline**: exception tests assert on the message (`.WithMessage("*…*")`) so accidental exceptions can't satisfy them; boundary values get tests on both sides; stubs start red (`Assert.Fail`), never silently green.
- Solution code favors clarity over cleverness: explanatory comments on the math/bit operations, named intermediate variables.
- Update the README progress checklist when a challenge is completed.
- Commit working state before refactoring. Use selective staging so each commit tells one story and builds green on its own.
