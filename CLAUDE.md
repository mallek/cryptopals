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

- **.NET 10, xunit, central package management** (`Directory.Packages.props` pins versions; csproj files reference packages without versions). Solution file is `Cryptopals.slnx`.
- **`src/Cryptopals/`** — the crypto library. One folder per set (`Set01/`, `Set02/`, …), one static class per challenge (`Challenge01.cs`). As challenges build on each other, reusable primitives (hex codec, base64 codec, XOR, scoring, etc.) get extracted here for later challenges to reuse.
- **`tests/Cryptopals.Tests/`** — mirrors the set/challenge structure. Each challenge is solved as a test: cryptopals provides a known input/output pair, so the test asserting that pair *is* the deliverable. Green test = challenge complete.

## Conventions

- Solution code favors clarity over cleverness: explanatory comments on the math/bit operations, named intermediate variables, and debug output that shows binary representations of each transformation step.
- Update the README progress checklist when a challenge is completed.
