# Library Cleanup + Set 1 Writeup — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reorganize the eight loose root utilities into category folders/namespaces (Codecs/Ciphers/Scoring/Visualization), then write an Orelbenr-style Set 1 walkthrough that embeds the repo's real trace output.

**Architecture:** Part 1 moves files with `git mv` (preserving history), updates each file's `namespace`, and adds explicit `using` lines to every consumer that loses implicit parent-namespace access. One green commit per category. Part 2 scaffolds a top-level `writeups/` tree and drafts one writeup per challenge, embedding curated real trace output captured from the test suite.

**Tech Stack:** .NET 10, xunit, PowerShell shell on Windows. No `.csproj` edits needed (globs `**/*.cs`). No warnings-as-errors, so the build/test suite is the regression net.

---

## Background: the namespace rule (read once)

C# child namespaces implicitly see ancestor namespaces. Today every primitive is in the bare
`Cryptopals` namespace, so `Cryptopals.Set01`, `Cryptopals.Aes`, and `Cryptopals.Tests` reach
them with no `using`. After a primitive moves to e.g. `Cryptopals.Codecs` (a *sibling* subtree),
those consumers must add `using Cryptopals.Codecs;`. Moved **test** files also need the `using`
for the category they test, because `Cryptopals.Tests.Codecs` does not implicitly see
`Cryptopals.Codecs`.

**Verified consumer map** (which files reference which category):

| Category → `using` | Source consumers | Test consumers |
|---|---|---|
| `Cryptopals.Codecs` (Hex, Base64) | Set01: Challenge 01, 02, 04, 05, 07, 08 | Set01 tests: 03, 04, 06, 08; `ByteFormatTests`; moved `HexTests`, `Base64Tests` |
| `Cryptopals.Ciphers` (Xor) | Set01: Challenge 02, 03, 04, 05, 06 | Set01 tests: 03; `Playground/Lab`; moved `XorTests` |
| `Cryptopals.Scoring` (EnglishScore, Hamming) | Set01: Challenge 03, 04, 06 | Set01 tests: 06; `Aes/AesAvalanche`; `Playground/Playground`; moved `EnglishScoreTests`, `HammingTests` |
| `Cryptopals.Visualization` (BitFormat, ByteFormat, TraceExtensions) | Set01: Challenge 01–08 (all); `Aes/AesKeySchedule`, `Aes/AesState`, `Aes/GaloisField` | moved `BitFormatTests`, `ByteFormatTests` |

`ByteFormatTests` needs **both** `Cryptopals.Codecs` and `Cryptopals.Visualization`.

**Fast test command (use after every category):**
`dotnet test --filter "Category!=Viewer&Category!=Slow"`

---

# Part 1 — Library Cleanup

## File Structure (Part 1 end state)

```
src/Cryptopals/
  Codecs/         Hex.cs, Base64.cs                              (namespace Cryptopals.Codecs)
  Ciphers/        Xor.cs                                         (namespace Cryptopals.Ciphers)
  Scoring/        EnglishScore.cs, Hamming.cs                    (namespace Cryptopals.Scoring)
  Visualization/  BitFormat.cs, ByteFormat.cs, TraceExtensions.cs (namespace Cryptopals.Visualization)
  Aes/            (unchanged)
  Set01/          (Challenge*.cs gain usings)
tests/Cryptopals.Tests/
  Codecs/         HexTests.cs, Base64Tests.cs                    (namespace Cryptopals.Tests.Codecs)
  Ciphers/        XorTests.cs                                    (namespace Cryptopals.Tests.Ciphers)
  Scoring/        EnglishScoreTests.cs, HammingTests.cs          (namespace Cryptopals.Tests.Scoring)
  Visualization/  BitFormatTests.cs, ByteFormatTests.cs          (namespace Cryptopals.Tests.Visualization)
  (root keeps only Cryptopals.Tests.csproj, xunit.runner.json)
```

---

## Task 1: Codecs folder (Hex, Base64)

**Files:**
- Move: `src/Cryptopals/Hex.cs` → `src/Cryptopals/Codecs/Hex.cs`
- Move: `src/Cryptopals/Base64.cs` → `src/Cryptopals/Codecs/Base64.cs`
- Move: `tests/Cryptopals.Tests/HexTests.cs` → `tests/Cryptopals.Tests/Codecs/HexTests.cs`
- Move: `tests/Cryptopals.Tests/Base64Tests.cs` → `tests/Cryptopals.Tests/Codecs/Base64Tests.cs`
- Modify (add `using Cryptopals.Codecs;`): `src/Cryptopals/Set01/Challenge01.cs`, `Challenge02.cs`, `Challenge04.cs`, `Challenge05.cs`, `Challenge07.cs`, `Challenge08.cs`; `tests/Cryptopals.Tests/Set01/Challenge03Tests.cs`, `Challenge04Tests.cs`, `Challenge06Tests.cs`, `Challenge08Tests.cs`; `tests/Cryptopals.Tests/ByteFormatTests.cs`

- [ ] **Step 1: Move the source files with git**

```powershell
git -C "D:/Skunkworks/cryptopals" mv src/Cryptopals/Hex.cs src/Cryptopals/Codecs/Hex.cs
git -C "D:/Skunkworks/cryptopals" mv src/Cryptopals/Base64.cs src/Cryptopals/Codecs/Base64.cs
git -C "D:/Skunkworks/cryptopals" mv tests/Cryptopals.Tests/HexTests.cs tests/Cryptopals.Tests/Codecs/HexTests.cs
git -C "D:/Skunkworks/cryptopals" mv tests/Cryptopals.Tests/Base64Tests.cs tests/Cryptopals.Tests/Codecs/Base64Tests.cs
```

- [ ] **Step 2: Update the source namespaces**

In `src/Cryptopals/Codecs/Hex.cs` and `src/Cryptopals/Codecs/Base64.cs`, change:
```csharp
namespace Cryptopals;
```
to:
```csharp
namespace Cryptopals.Codecs;
```

- [ ] **Step 3: Update the moved test files**

In `tests/Cryptopals.Tests/Codecs/HexTests.cs` and `Base64Tests.cs`:
- change `namespace Cryptopals.Tests;` to `namespace Cryptopals.Tests.Codecs;`
- ensure the file has `using Cryptopals.Codecs;` near the top (add it if absent).

- [ ] **Step 4: Add `using Cryptopals.Codecs;` to every other Codecs consumer**

Add this line to the top using-block (before the `namespace` line) of each file. If a file has no
usings yet, add it as the first line:
```csharp
using Cryptopals.Codecs;
```
Files: `src/Cryptopals/Set01/Challenge01.cs`, `Challenge02.cs`, `Challenge04.cs`, `Challenge05.cs`,
`Challenge07.cs`, `Challenge08.cs`; `tests/Cryptopals.Tests/Set01/Challenge03Tests.cs`,
`Challenge04Tests.cs`, `Challenge06Tests.cs`, `Challenge08Tests.cs`;
`tests/Cryptopals.Tests/ByteFormatTests.cs`.

- [ ] **Step 5: Build and run the fast suite**

Run:
```powershell
dotnet test --filter "Category!=Viewer&Category!=Slow"
```
Expected: build succeeds, all fast tests PASS. If a file errors with "The type or namespace name
'Hex'/'Base64' could not be found," it is a Codecs consumer that was missed in Step 4 — add the
`using` there and re-run.

- [ ] **Step 6: Commit**

```powershell
git -C "D:/Skunkworks/cryptopals" add -A
git -C "D:/Skunkworks/cryptopals" commit -m "refactor: move codecs (Hex, Base64) into Codecs/ namespace"
```

---

## Task 2: Ciphers folder (Xor)

**Files:**
- Move: `src/Cryptopals/Xor.cs` → `src/Cryptopals/Ciphers/Xor.cs`
- Move: `tests/Cryptopals.Tests/XorTests.cs` → `tests/Cryptopals.Tests/Ciphers/XorTests.cs`
- Modify (add `using Cryptopals.Ciphers;`): `src/Cryptopals/Set01/Challenge02.cs`, `Challenge03.cs`, `Challenge04.cs`, `Challenge05.cs`, `Challenge06.cs`; `tests/Cryptopals.Tests/Set01/Challenge03Tests.cs`; `tests/Cryptopals.Tests/Playground/Lab.cs`

- [ ] **Step 1: Move the files with git**

```powershell
git -C "D:/Skunkworks/cryptopals" mv src/Cryptopals/Xor.cs src/Cryptopals/Ciphers/Xor.cs
git -C "D:/Skunkworks/cryptopals" mv tests/Cryptopals.Tests/XorTests.cs tests/Cryptopals.Tests/Ciphers/XorTests.cs
```

- [ ] **Step 2: Update the source namespace**

In `src/Cryptopals/Ciphers/Xor.cs`, change `namespace Cryptopals;` to `namespace Cryptopals.Ciphers;`.

- [ ] **Step 3: Update the moved test file**

In `tests/Cryptopals.Tests/Ciphers/XorTests.cs`:
- change `namespace Cryptopals.Tests;` to `namespace Cryptopals.Tests.Ciphers;`
- ensure `using Cryptopals.Ciphers;` is present near the top.

- [ ] **Step 4: Add `using Cryptopals.Ciphers;` to every other Ciphers consumer**

Add `using Cryptopals.Ciphers;` to: `src/Cryptopals/Set01/Challenge02.cs`, `Challenge03.cs`,
`Challenge04.cs`, `Challenge05.cs`, `Challenge06.cs`;
`tests/Cryptopals.Tests/Set01/Challenge03Tests.cs`; `tests/Cryptopals.Tests/Playground/Lab.cs`.

- [ ] **Step 5: Build and run the fast suite**

Run:
```powershell
dotnet test --filter "Category!=Viewer&Category!=Slow"
```
Expected: PASS. A "'Xor' could not be found" error means a missed consumer — add the `using` and re-run.

- [ ] **Step 6: Commit**

```powershell
git -C "D:/Skunkworks/cryptopals" add -A
git -C "D:/Skunkworks/cryptopals" commit -m "refactor: move Xor into Ciphers/ namespace"
```

---

## Task 3: Scoring folder (EnglishScore, Hamming)

**Files:**
- Move: `src/Cryptopals/EnglishScore.cs` → `src/Cryptopals/Scoring/EnglishScore.cs`
- Move: `src/Cryptopals/Hamming.cs` → `src/Cryptopals/Scoring/Hamming.cs`
- Move: `tests/Cryptopals.Tests/EnglishScoreTests.cs` → `tests/Cryptopals.Tests/Scoring/EnglishScoreTests.cs`
- Move: `tests/Cryptopals.Tests/HammingTests.cs` → `tests/Cryptopals.Tests/Scoring/HammingTests.cs`
- Modify (add `using Cryptopals.Scoring;`): `src/Cryptopals/Set01/Challenge03.cs`, `Challenge04.cs`, `Challenge06.cs`; `tests/Cryptopals.Tests/Set01/Challenge06Tests.cs`; `tests/Cryptopals.Tests/Aes/AesAvalanche.cs`; `tests/Cryptopals.Tests/Playground/Playground.cs`

- [ ] **Step 1: Move the files with git**

```powershell
git -C "D:/Skunkworks/cryptopals" mv src/Cryptopals/EnglishScore.cs src/Cryptopals/Scoring/EnglishScore.cs
git -C "D:/Skunkworks/cryptopals" mv src/Cryptopals/Hamming.cs src/Cryptopals/Scoring/Hamming.cs
git -C "D:/Skunkworks/cryptopals" mv tests/Cryptopals.Tests/EnglishScoreTests.cs tests/Cryptopals.Tests/Scoring/EnglishScoreTests.cs
git -C "D:/Skunkworks/cryptopals" mv tests/Cryptopals.Tests/HammingTests.cs tests/Cryptopals.Tests/Scoring/HammingTests.cs
```

- [ ] **Step 2: Update the source namespaces**

In `src/Cryptopals/Scoring/EnglishScore.cs` and `Hamming.cs`, change `namespace Cryptopals;` to
`namespace Cryptopals.Scoring;`.

- [ ] **Step 3: Update the moved test files**

In `tests/Cryptopals.Tests/Scoring/EnglishScoreTests.cs` and `HammingTests.cs`:
- change `namespace Cryptopals.Tests;` to `namespace Cryptopals.Tests.Scoring;`
- ensure `using Cryptopals.Scoring;` is present near the top.

- [ ] **Step 4: Add `using Cryptopals.Scoring;` to every other Scoring consumer**

Add `using Cryptopals.Scoring;` to: `src/Cryptopals/Set01/Challenge03.cs`, `Challenge04.cs`,
`Challenge06.cs`; `tests/Cryptopals.Tests/Set01/Challenge06Tests.cs`;
`tests/Cryptopals.Tests/Aes/AesAvalanche.cs`; `tests/Cryptopals.Tests/Playground/Playground.cs`.

- [ ] **Step 5: Build and run the fast suite**

Run:
```powershell
dotnet test --filter "Category!=Viewer&Category!=Slow"
```
Expected: PASS. A "'EnglishScore'/'Hamming' could not be found" error means a missed consumer.

- [ ] **Step 6: Commit**

```powershell
git -C "D:/Skunkworks/cryptopals" add -A
git -C "D:/Skunkworks/cryptopals" commit -m "refactor: move EnglishScore + Hamming into Scoring/ namespace"
```

---

## Task 4: Visualization folder (BitFormat, ByteFormat, TraceExtensions)

**Files:**
- Move: `src/Cryptopals/BitFormat.cs` → `src/Cryptopals/Visualization/BitFormat.cs`
- Move: `src/Cryptopals/ByteFormat.cs` → `src/Cryptopals/Visualization/ByteFormat.cs`
- Move: `src/Cryptopals/TraceExtensions.cs` → `src/Cryptopals/Visualization/TraceExtensions.cs`
- Move: `tests/Cryptopals.Tests/BitFormatTests.cs` → `tests/Cryptopals.Tests/Visualization/BitFormatTests.cs`
- Move: `tests/Cryptopals.Tests/ByteFormatTests.cs` → `tests/Cryptopals.Tests/Visualization/ByteFormatTests.cs`
- Modify (add `using Cryptopals.Visualization;`): all 8 `src/Cryptopals/Set01/Challenge0N.cs`; `src/Cryptopals/Aes/AesKeySchedule.cs`, `AesState.cs`, `GaloisField.cs`

- [ ] **Step 1: Move the files with git**

```powershell
git -C "D:/Skunkworks/cryptopals" mv src/Cryptopals/BitFormat.cs src/Cryptopals/Visualization/BitFormat.cs
git -C "D:/Skunkworks/cryptopals" mv src/Cryptopals/ByteFormat.cs src/Cryptopals/Visualization/ByteFormat.cs
git -C "D:/Skunkworks/cryptopals" mv src/Cryptopals/TraceExtensions.cs src/Cryptopals/Visualization/TraceExtensions.cs
git -C "D:/Skunkworks/cryptopals" mv tests/Cryptopals.Tests/BitFormatTests.cs tests/Cryptopals.Tests/Visualization/BitFormatTests.cs
git -C "D:/Skunkworks/cryptopals" mv tests/Cryptopals.Tests/ByteFormatTests.cs tests/Cryptopals.Tests/Visualization/ByteFormatTests.cs
```

- [ ] **Step 2: Update the source namespaces**

In `BitFormat.cs`, `ByteFormat.cs`, `TraceExtensions.cs` (now under `Visualization/`), change
`namespace Cryptopals;` to `namespace Cryptopals.Visualization;`.

- [ ] **Step 3: Update the moved test files**

In `tests/Cryptopals.Tests/Visualization/BitFormatTests.cs` and `ByteFormatTests.cs`:
- change `namespace Cryptopals.Tests;` to `namespace Cryptopals.Tests.Visualization;`
- ensure `using Cryptopals.Visualization;` is present.
- `ByteFormatTests.cs` must keep its `using Cryptopals.Codecs;` (added in Task 1) as well.

- [ ] **Step 4: Add `using Cryptopals.Visualization;` to every other Visualization consumer**

Add `using Cryptopals.Visualization;` to: all eight `src/Cryptopals/Set01/Challenge01.cs` …
`Challenge08.cs`; and `src/Cryptopals/Aes/AesKeySchedule.cs`, `src/Cryptopals/Aes/AesState.cs`,
`src/Cryptopals/Aes/GaloisField.cs`.

- [ ] **Step 5: Build and run the fast suite**

Run:
```powershell
dotnet test --filter "Category!=Viewer&Category!=Slow"
```
Expected: PASS. Errors naming `BitFormat`, `ByteFormat`, or the `.Line`/`.Section`/`.Detail`
extension methods indicate a missed Visualization consumer.

- [ ] **Step 6: Commit**

```powershell
git -C "D:/Skunkworks/cryptopals" add -A
git -C "D:/Skunkworks/cryptopals" commit -m "refactor: move formatters + trace into Visualization/ namespace"
```

---

## Task 5: Tidy stray usings, docs, and full-suite verification

**Files:**
- Modify: any file still carrying a now-unused `using Cryptopals;` (root namespace is empty after the moves): `tests/Cryptopals.Tests/Visualization/BitFormatTests.cs`, `ByteFormatTests.cs`, `tests/Cryptopals.Tests/Aes/AesAvalanche.cs`, `tests/Cryptopals.Tests/Aes/AesEcbTests.cs`, `tests/Cryptopals.Tests/Playground/Lab.cs`, `tests/Cryptopals.Tests/Playground/Playground.cs`
- Modify: `CLAUDE.md` (Architecture section), `README.md` (Layout section)

- [ ] **Step 1: Remove unused `using Cryptopals;` lines**

The bare `Cryptopals` namespace now holds no types (everything lives in subnamespaces). Remove any
remaining standalone `using Cryptopals;` line from the files listed above. Leave `using
Cryptopals.Aes;` / `.Set01;` / category usings intact. (These are warnings, not errors, so this is
cosmetic — but it keeps the tree honest.)

- [ ] **Step 2: Update the Architecture note in `CLAUDE.md`**

In the `## Architecture` section, replace the sentence describing primitives so it names the new
folders. Change the phrase listing root primitives to read:
```
**`src/Cryptopals/`** — library primitives organized by responsibility: `Codecs/` (`Hex`,
`Base64`), `Ciphers/` (`Xor`), `Scoring/` (`EnglishScore`, `Hamming`), `Visualization/`
(`BitFormat`, `ByteFormat`, `TraceExtensions`), and `Aes/` (the hand-rolled AES). Challenges live
in set folders (`Set01/`, …) as thin compositions.
```

- [ ] **Step 3: Update the Layout block in `README.md`**

In the `## Layout` fenced block, replace the `src/Cryptopals/` line with folder-aware lines:
```
src/Cryptopals/Codecs/         # Hex, Base64
src/Cryptopals/Ciphers/        # Xor
src/Cryptopals/Scoring/        # EnglishScore, Hamming
src/Cryptopals/Visualization/  # BitFormat, ByteFormat, TraceExtensions
src/Cryptopals/Aes/            # hand-rolled AES-128
src/Cryptopals/Set01/          # challenge solvers — thin compositions of primitives
```

- [ ] **Step 4: Guard — no built-in crypto/encoding crept in**

This refactor must add ZERO new dependencies and ZERO built-in crypto/encoding APIs — the whole
library is hand-rolled. Verify:
```powershell
git -C "D:/Skunkworks/cryptopals" diff --stat HEAD~4 -- '*.csproj' Directory.Packages.props
git -C "D:/Skunkworks/cryptopals" grep -nE "System\.Security\.Cryptography|Convert\.(To|From)Base64String|Convert\.(To|From)HexString" -- 'src/**/*.cs' 'tests/**/*.cs'
```
Expected: the first command prints NOTHING (no project/package files changed by the refactor). The
second command prints NOTHING (no forbidden APIs anywhere). If either prints output, stop and remove
the offending change before continuing.

- [ ] **Step 5: Run the FULL suite**

Run:
```powershell
dotnet test
```
Expected: every test PASSES (slow + viewer included). This is the pre-writeup green checkpoint.

- [ ] **Step 6: Commit**

```powershell
git -C "D:/Skunkworks/cryptopals" add -A
git -C "D:/Skunkworks/cryptopals" commit -m "chore: drop dead usings, document new folder layout"
```

---

# Part 2 — Set 1 Writeup

## File Structure (Part 2 end state)

```
README.md                              # repo root: gains a "Writeups" TOC section
writeups/
  Set01/
    README.md                          # Set 1 index: one blurb + link per challenge
    Challenge01.md … Challenge08.md    # the writeups
src/Cryptopals/Set01/
  README.md                            # signpost → ../../../writeups/Set01/
```

Each `ChallengeNN.md` follows this fixed five-section template:
1. **The problem** — one-paragraph restatement + link to `https://cryptopals.com/sets/1/challenges/NN`.
2. **The idea** — the fundamental being taught.
3. **The build** — code snippet(s) pulled from the actual repo, each with a relative link to the source file.
4. **Seeing it happen** — a curated/trimmed fenced block of real trace output.
5. **The reveal** — the answer/decrypted text + one line on why it matters.

---

## Task 6: Scaffold the writeups tree

**Files:**
- Create: `writeups/Set01/README.md`
- Create: `src/Cryptopals/Set01/README.md`
- Modify: `README.md` (add Writeups section)

- [ ] **Step 1: Create the Set 1 index `writeups/Set01/README.md`**

Write a header, a one-sentence description of the set, and a table with one row per challenge:
columns = Challenge | Writeup link | one-line blurb. Link each to `ChallengeNN.md` (files created in
later tasks). Example row:
```markdown
| 1 | [Hex → Base64](Challenge01.md) | Hand-rolling base64 by regrouping bits 8→6. |
```
Include all 8 rows (blurbs may be refined in Task 11).

- [ ] **Step 2: Create the source signpost `src/Cryptopals/Set01/README.md`**

```markdown
# Set 1 — Challenge Solvers

These are thin compositions of the library primitives. The narrative walkthroughs —
problem, concept, code, and **real trace output** — live in
[`/writeups/Set01/`](../../../writeups/Set01/README.md).
```

- [ ] **Step 3: Add a Writeups section to the repo-root `README.md`**

After the `## Progress` section, add:
```markdown
## Writeups

Narrative walkthroughs with real trace output:

- [Set 1 — Basics](writeups/Set01/README.md)
```

- [ ] **Step 4: Commit**

```powershell
git -C "D:/Skunkworks/cryptopals" add -A
git -C "D:/Skunkworks/cryptopals" commit -m "docs: scaffold writeups tree + indexes"
```

---

## Task 7: Capture real trace output for all 8 challenges

**Files:**
- Create (scratch, not committed): `writeups/Set01/_traces/ChallengeNN.txt` for N = 01..08

- [ ] **Step 1: Run each challenge test with detailed verbosity and save the output**

For each challenge, run (example for 01):
```powershell
dotnet test --filter "FullyQualifiedName~Cryptopals.Tests.Set01.Challenge01Tests" -l "console;verbosity=detailed" 2>&1 | Tee-Object -FilePath writeups/Set01/_traces/Challenge01.txt
```
Repeat for `Challenge02Tests` … `Challenge08Tests`. These raw captures are the trim source for each
writeup's "Seeing it happen" block. (The `_traces/` folder is scratch; do not commit it — it is
trimmed into the `.md` files.)

- [ ] **Step 2: Add `_traces/` to `.gitignore`**

Append to `.gitignore`:
```
writeups/Set01/_traces/
```

- [ ] **Step 3: Confirm captures are non-empty**

Run:
```powershell
Get-ChildItem writeups/Set01/_traces/*.txt | Select-Object Name, Length
```
Expected: eight files, each Length > 0. If a capture is empty, the trace sink may not be wired in
that test — note it; that challenge's "Seeing it happen" block will quote the solver's inline trace
instead.

---

## Task 8: Draft writeups for Challenges 1–2 (review batch 1)

**Files:**
- Create: `writeups/Set01/Challenge01.md`, `writeups/Set01/Challenge02.md`
- Reference (link + snippet source): `src/Cryptopals/Set01/Challenge01.cs`, `src/Cryptopals/Codecs/Hex.cs`, `src/Cryptopals/Codecs/Base64.cs`, `src/Cryptopals/Set01/Challenge02.cs`, `src/Cryptopals/Ciphers/Xor.cs`

- [ ] **Step 1: Draft `Challenge01.md` (hex → base64)** using the five-section template.
  - Section 3 snippets quote the real bit-regrouping code from `Codecs/Hex.cs` and `Codecs/Base64.cs`
    with relative links (e.g. `[Base64.cs](../../src/Cryptopals/Codecs/Base64.cs)`).
  - Section 4 embeds the **full** bit-layout trace from `_traces/Challenge01.txt` (this is the star —
    8→6 bit regrouping is the whole point), trimmed only of xunit boilerplate lines.

- [ ] **Step 2: Draft `Challenge02.md` (fixed XOR)** using the template.
  - Section 2 includes the XOR truth table (LaTeX or a small markdown table).
  - Section 4 embeds curated trace showing the byte-by-byte XOR.

- [ ] **Step 3: Present both drafts to the user for review.** Apply requested edits before committing.

- [ ] **Step 4: Commit**

```powershell
git -C "D:/Skunkworks/cryptopals" add writeups/Set01/Challenge01.md writeups/Set01/Challenge02.md
git -C "D:/Skunkworks/cryptopals" commit -m "docs: Set 1 writeups for Challenges 1-2"
```

---

## Task 9: Draft writeups for Challenges 3–4 (review batch 2)

**Files:**
- Create: `writeups/Set01/Challenge03.md`, `writeups/Set01/Challenge04.md`
- Reference: `src/Cryptopals/Set01/Challenge03.cs`, `src/Cryptopals/Set01/Challenge04.cs`, `src/Cryptopals/Scoring/EnglishScore.cs`, `src/Cryptopals/Ciphers/Xor.cs`

- [ ] **Step 1: Draft `Challenge03.md` (single-byte XOR + frequency scoring).**
  - Section 2 explains frequency/χ² scoring; quote the scoring approach from `Scoring/EnglishScore.cs`.
  - Section 4 shows the trimmed score-sweep trace and the winning key/plaintext.

- [ ] **Step 2: Draft `Challenge04.md` (detect single-char XOR).**
  - Emphasize reusing Challenge 3's scorer across many lines; Section 5 reveals the found line.

- [ ] **Step 3: Present both drafts for review.** Apply edits.

- [ ] **Step 4: Commit**

```powershell
git -C "D:/Skunkworks/cryptopals" add writeups/Set01/Challenge03.md writeups/Set01/Challenge04.md
git -C "D:/Skunkworks/cryptopals" commit -m "docs: Set 1 writeups for Challenges 3-4"
```

---

## Task 10: Draft writeups for Challenges 5–6 (review batch 3)

**Files:**
- Create: `writeups/Set01/Challenge05.md`, `writeups/Set01/Challenge06.md`
- Reference: `src/Cryptopals/Set01/Challenge05.cs`, `src/Cryptopals/Set01/Challenge06.cs`, `src/Cryptopals/Scoring/Hamming.cs`, `src/Cryptopals/Ciphers/Xor.cs`

- [ ] **Step 1: Draft `Challenge05.md` (repeating-key XOR).** Quote the key-cycling code; Section 4
  shows the trace of the key repeating across the plaintext.

- [ ] **Step 2: Draft `Challenge06.md` (break repeating-key XOR).**
  - Section 2 explains Hamming distance for keysize detection (include the formula); quote
    `Scoring/Hamming.cs`.
  - Section 4 shows curated traces: keysize ranking, then per-column single-byte solve. This is a
    flagship walkthrough — give it room.

- [ ] **Step 3: Present both drafts for review.** Apply edits.

- [ ] **Step 4: Commit**

```powershell
git -C "D:/Skunkworks/cryptopals" add writeups/Set01/Challenge05.md writeups/Set01/Challenge06.md
git -C "D:/Skunkworks/cryptopals" commit -m "docs: Set 1 writeups for Challenges 5-6"
```

---

## Task 11: Draft writeups for Challenges 7–8 + finalize indexes (review batch 4)

**Files:**
- Create: `writeups/Set01/Challenge07.md`, `writeups/Set01/Challenge08.md`
- Modify: `writeups/Set01/README.md` (finalize blurbs)
- Reference: `src/Cryptopals/Set01/Challenge07.cs`, `src/Cryptopals/Set01/Challenge08.cs`, `src/Cryptopals/Aes/*.cs`

- [ ] **Step 1: Draft `Challenge07.md` (AES-128-ECB).** Link the hand-rolled AES files; Section 4
  embeds a curated `AesState`/`GaloisField` round trace (trimmed to one illustrative round).

- [ ] **Step 2: Draft `Challenge08.md` (detect ECB).**
  - Section 4 embeds the **full** repeated-block visualization (the ECB fingerprint is the star).
  - Section 5 ties back to ECB's determinism and forward to Set 2.

- [ ] **Step 3: Finalize `writeups/Set01/README.md`** — refine each blurb so it accurately summarizes
  the now-written challenge file.

- [ ] **Step 4: Present drafts + final index for review.** Apply edits.

- [ ] **Step 5: Verify all writeup links resolve**

Run:
```powershell
Get-ChildItem writeups/Set01/Challenge0*.md | Select-Object Name, Length
```
Expected: eight files, each Length > 0. Manually confirm each relative source link in the `.md`
files points to an existing file under `src/Cryptopals/`.

- [ ] **Step 6: Commit**

```powershell
git -C "D:/Skunkworks/cryptopals" add writeups/Set01/Challenge07.md writeups/Set01/Challenge08.md writeups/Set01/README.md
git -C "D:/Skunkworks/cryptopals" commit -m "docs: Set 1 writeups for Challenges 7-8 + finalize index"
```

---

## Done criteria

- `src/Cryptopals/` root holds no loose `.cs` primitives — all under Codecs/Ciphers/Scoring/Visualization/Aes.
- `tests/Cryptopals.Tests/` root holds no loose `*Tests.cs` — all mirrored into category folders.
- `dotnet test` (full suite) is green.
- `writeups/Set01/` has an index + 8 challenge writeups, each with real trace output and working
  source links; repo-root README and `src/Cryptopals/Set01/README.md` both link in.
