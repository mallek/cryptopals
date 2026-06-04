# Design — Library Cleanup + Set 1 Writeup

**Date:** 2026-06-04
**Status:** Approved (pending spec review)

## Goal

Two sequenced pieces of work on the cryptopals repo now that Set 1 is complete:

1. **Cleanup** — organize the eight loose utility files (and their tests) currently sitting
   flat in the root of `src/Cryptopals/` into category folders with matching namespaces,
   consistent with the existing `Aes/` and `Set01/` convention (folder == namespace segment).
2. **Writeup** — an Orelbenr-style narrative walkthrough of Set 1 that shows off the repo's
   real trace/visualizer output (the thing a normal writeup can't do).

**Sequence:** cleanup first, then the writeup. The writeup references the new namespaces and
links to source files, so the reorg must land first.

Reference for the writeup style:
<https://github.com/Orelbenr/cryptopals-writeups/blob/main/Set1_Basics/README.md>

---

## Part 1 — Library cleanup

### Layout (Option A: category folders + matching namespaces)

| New folder | Files | New namespace |
|---|---|---|
| `Codecs/` | `Hex.cs`, `Base64.cs` | `Cryptopals.Codecs` |
| `Ciphers/` | `Xor.cs` | `Cryptopals.Ciphers` |
| `Scoring/` | `EnglishScore.cs`, `Hamming.cs` | `Cryptopals.Scoring` |
| `Visualization/` | `BitFormat.cs`, `ByteFormat.cs`, `TraceExtensions.cs` | `Cryptopals.Visualization` |

- `Aes/` and `Set01/` are unchanged — they already follow folder == namespace.
- `Hamming` goes under `Scoring/` (it is the other statistical/analysis helper alongside
  `EnglishScore`).
- Tests mirror the same four folders under `tests/Cryptopals.Tests/` with matching
  `Cryptopals.Tests.<Category>` namespaces (e.g. `tests/Cryptopals.Tests/Codecs/HexTests.cs`).

### Mechanics

- Move files with `git mv` to preserve history.
- Update the `namespace` declaration in each moved file.
- Update consumers (Set 1 challenge solvers + all tests) by adding the needed
  `using Cryptopals.Codecs;` / `Ciphers` / `Scoring` / `Visualization` lines.
- No `.csproj` edits — the project globs `**/*.cs` automatically.
- Data-file copy config is unaffected.

### Safety net & commit strategy

- The fast test suite (`dotnet test --filter "Category!=Viewer&Category!=Slow"`) is the
  regression net; run it after each category move.
- One commit per category folder (Codecs → Ciphers → Scoring → Visualization), each green on
  its own, matching the repo's "one story per commit, builds green" convention.
- Run the full suite (`dotnet test`) before the final commit.

---

## Part 2 — Set 1 writeup

### Tree

```
README.md                           # repo root: master TOC linking to each set
writeups/
  Set01/
    README.md                       # Set 1 index: one blurb + link per challenge (not a link wall)
    Challenge01.md … Challenge08.md  # the actual writeups
src/Cryptopals/Set01/
  README.md                         # short signpost pointing to /writeups/Set01/
```

- `writeups/` (top level) holds all writeup content, organized by set.
- `writeups/Set01/README.md` is the curated index — short blurb + link per challenge.
- `src/Cryptopals/Set01/README.md` is a small pointer for someone browsing the source.
- Repo-root `README.md` gains a master TOC link to the writeups.

### Per-challenge file template

Each `ChallengeNN.md` follows a consistent shape:

1. **The problem** — brief restatement of the challenge + link to cryptopals.com.
2. **The idea** — the fundamental being taught (bit math, frequency analysis, Hamming
   distance, ECB determinism, …).
3. **The build** — key code snippets pulled from the *actual* repo, with links to the real
   source file (e.g. `src/Cryptopals/Codecs/Hex.cs`).
4. **Seeing it happen** — curated/trimmed real trace output in fenced blocks. Full bit-layouts
   where they are the star (Ch1 hex→base64, Ch8 ECB repeat), trimmed elsewhere.
5. **The reveal** — the answer / decrypted text and a one-line "why this matters."

- Light math notation (GitHub renders LaTeX) where it earns its place — XOR truth table,
  Hamming distance, frequency/χ² scoring.
- Trace output is captured by actually running the tests with detailed verbosity
  (`dotnet test --filter "FullyQualifiedName~ChallengeNN" -l "console;verbosity=detailed"`)
  and trimming to the most illustrative lines.

### Process

- Claude drafts, user reviews/edits. The writeup is documentation, not a challenge solution,
  so this does not conflict with the tutor premise.
- Drafted in small batches (1–2 challenges at a time) so review stays manageable rather than
  dumping eight files at once.

---

## Out of scope

- No new library features or challenge solutions.
- No changes to `Aes/` or the existing challenge `.cs` solvers beyond `using` updates.
- Sets 2+ writeups (the structure is built to extend, but only Set 1 is written now).
