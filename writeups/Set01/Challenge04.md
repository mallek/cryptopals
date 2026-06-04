# Challenge 4: Detect Single-character XOR

> [cryptopals.com/sets/1/challenges/4](https://cryptopals.com/sets/1/challenges/4)
> Solver: [`Set01/Challenge04.cs`](../../src/Cryptopals/Set01/Challenge04.cs)

## The problem

A file holds ~327 hex strings. Exactly **one** of them was encrypted with single-byte XOR; the rest
are random noise. Find the encrypted line and decrypt it. We are not told which line, and we have no
key.

## The idea

This is [Challenge 3](Challenge03.md) with one more layer of "take the best."

Challenge 3 cracked *one* ciphertext by taking the max score over 256 keys. Challenge 4 cracks
*every* line that way, then takes the max over lines. Two nested searches:

```
for each line:
    best_for_line = max over 256 keys of english_score(decrypt(line, key))   ← Challenge 3
answer = the line whose best_for_line is highest                              ← Challenge 4
```

Why does this work? A random noise line, XORed against its single best key, still decrypts to
garbage, so its best score stays low. The *real* ciphertext, against the *right* key, decrypts to
English and scores high. The encrypted line floats to the top on score alone. The same number that
*cracked* the cipher now also *detects* which line is even worth cracking.

## The build

`Detect` loops over the lines, cracks each one quietly (Challenge 3's `Crack` with the trace sink
withheld, because 327 lines times 256 keys is a lot of scoring), and keeps the best result together
with the line that produced it ([`Set01/Challenge04.cs`](../../src/Cryptopals/Set01/Challenge04.cs)):

```csharp
for (int i = 0; i < hexLines.Count; i++)
{
    if (!Hex.IsValid(hexLines[i])) { trace?.Invoke($"line {i}: invalid hex, skipping"); continue; }
    byte[] ciphertext = Hex.Decode(hexLines[i]);
    CrackResult result = Challenge03.Crack(ciphertext, null);          // the inner max-over-keys
    if (best is null || result.Score > best.Result.Score)             // the outer max-over-lines
        best = new Detection(i, hexLines[i], result);
}
```

`CrackResult` only carries key/plaintext/score, so a small `Detection` record pairs the winning
crack with its line number and ciphertext. Invalid hex lines are skipped rather than allowed to
crash the whole scan.

## Seeing it happen

Most lines report a mediocre best score (their "least-bad" key still yields noise):

```
line 324: score 73.60516252390057 for key 0x69
line 325: score 49.83460803059273 for key 0x74
line 326: score 67.17686424474186 for key 0x78
```

Then one line stands well above the rest, and only it gets the loud per-byte decryption:

```
─── Winner: line 170 with key 0x35 score 203.41 → "Now that the party is jumping·" ───
XOR decryption:
  7B ^ 35 = 4E  'N'
  5A ^ 35 = 6F  'o'
  42 ^ 35 = 77  'w'
  15 ^ 35 = 20  ' '
  41 ^ 35 = 74  't'
  5D ^ 35 = 68  'h'
  54 ^ 35 = 61  'a'
  41 ^ 35 = 74  't'
  ...
```

(The trailing `0A` shown as `·` is the line's newline, kept honest in the trace.)

## The reveal

```
line 170, key 0x35 → "Now that the party is jumping\n"
```

The takeaway: **a good score is a detector, not just a cracker.** Because "looks like English" is
quantified, we can rank not only keys but whole candidate messages, and the signal separates cleanly
from the noise. This ranking-by-score habit carries straight into [Challenge 6](Challenge06.md),
where it solves one column of a repeating-key cipher at a time.

That same per-byte scoring has a breaking point, though: starve it of text and it stops working. We
built an experimentation rig (the `Playground/` viewers) to map exactly where, sweeping a cipher
until the break dies. That boundary is called the **seam**, and it gets the full treatment in
[Challenge 6](Challenge06.md#the-seam-where-the-break-dies).
