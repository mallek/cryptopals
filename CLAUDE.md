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
dotnet test                                              # run all challenge tests
dotnet test --filter "FullyQualifiedName~Challenge01"    # run one challenge
dotnet build                                             # build everything
```

## Architecture

- **.NET 10, xunit + AwesomeAssertions, central package management** (`Directory.Packages.props` pins versions; csproj files reference packages without versions). Solution file is `Cryptopals.slnx`.
- **`src/Cryptopals/`** — the library primitives: codecs (`Hex`, `Base64`), ciphers (`Xor`), visualization (`BitFormat`, `ByteFormat`). Challenges live in set folders (`Set01/`, `Set02/`, …) as thin compositions of primitives — if a challenge method grows past a few lines, something in it wants to become a primitive.
- **`tests/Cryptopals.Tests/`** — mirrors src. Primitives get full test suites; each challenge gets a test asserting the known cryptopals answer. Green test = challenge complete.

## Conventions

- **Trace sink pattern**: primitives and challenges take an optional `Action<string>? trace = null`. Tests pass `ITestOutputHelper.WriteLine` so test output shows the bit-level walkthrough. Primitives trace *mechanics* (bit math); challenges trace *meaning* (the story, ASCII reveals).
- **Codec naming**: `Decode` goes toward bytes, `Encode` goes toward text. All codecs follow this.
- **Visualization is read-only**: formatters never modify their input. Every formatter has a purity test.
- **Test discipline**: exception tests assert on the message (`.WithMessage("*…*")`) so accidental exceptions can't satisfy them; boundary values get tests on both sides; stubs start red (`Assert.Fail`), never silently green.
- Solution code favors clarity over cleverness: explanatory comments on the math/bit operations, named intermediate variables.
- Update the README progress checklist when a challenge is completed.
- Commit working state before refactoring. Use selective staging so each commit tells one story and builds green on its own.
