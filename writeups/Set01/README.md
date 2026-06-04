# Set 1: Basics

The warm-up set. Eight challenges that build the whole foundation: encodings, XOR, frequency
analysis, and the first real block cipher (AES). Every primitive here is **hand-rolled**: no
`Convert.ToBase64String`, no `System.Security.Cryptography`. The point is to feel the bytes.

Each writeup follows the same shape: the problem, the idea behind it, the actual code that solves
it (linked to source), **real trace output** from the test suite showing the bytes move, and the
reveal.

| # | Challenge | Writeup | The gist |
|---|-----------|---------|----------|
| 1 | Convert hex to base64 | [Challenge 1](Challenge01.md) | Regrouping bits 8→6: three bytes become four base64 symbols. |
| 2 | Fixed XOR | [Challenge 2](Challenge02.md) | XOR two buffers byte by byte, the operation every cipher here is built on. |
| 3 | Single-byte XOR cipher | [Challenge 3](Challenge03.md) | Try all 256 keys, let English letter frequency pick the winner. |
| 4 | Detect single-character XOR | [Challenge 4](Challenge04.md) | Run the Challenge 3 scorer over 300 lines; the cipher-text outs itself. |
| 5 | Implement repeating-key XOR | [Challenge 5](Challenge05.md) | Cycle a multi-byte key across the message. |
| 6 | Break repeating-key XOR | [Challenge 6](Challenge06.md) | Hamming distance finds the key length; then it's 1-byte XOR per column. |
| 7 | AES-128 in ECB mode | [Challenge 7](Challenge07.md) | The hand-rolled AES: GF(2⁸), the S-box, the round function. |
| 8 | Detect AES-128-ECB | [Challenge 8](Challenge08.md) | ECB is deterministic, so repeated plaintext blocks leak as repeated ciphertext. |

> Trace blocks below are captured from the actual tests with
> `dotnet test --filter "FullyQualifiedName~ChallengeNNTests" -l "console;verbosity=detailed"`
> and trimmed to the illustrative lines.
