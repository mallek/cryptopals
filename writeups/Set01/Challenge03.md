# Challenge 3: Single-byte XOR Cipher

> [cryptopals.com/sets/1/challenges/3](https://cryptopals.com/sets/1/challenges/3)
> Solver: [`Set01/Challenge03.cs`](../../src/Cryptopals/Set01/Challenge03.cs)

## The problem

A message was XORed against a single secret byte, the same byte repeated across the whole message,
and handed to us as hex. Recover the byte and the message. This is the first actual *break*: no key
is given, we have to find it.

## The idea

Two observations make this easy.

First, the keyspace is tiny. A single byte has only **256 possible values**, so we can simply try
all of them. Brute force is not a clever shortcut here, it is the whole strategy.

Second, and this is the real lesson: trying all 256 keys is useless unless we can *recognize the
right answer*. A human glancing at 256 candidate decryptions spots the English one instantly, but a
program needs that judgment turned into a number. So we score each candidate by how much it "looks
like English" and keep the highest. The cipher is broken not by math but by **measuring text**.

The scorer leans on letter frequency. In English, some characters are wildly more common than
others (space most of all, then `E`, `T`, `A`, `O`...). Give each byte a weight equal to its
frequency, penalize bytes that could not appear in real text, and sum it up. Gibberish (full of
unprintable bytes) scores deeply negative; real prose scores high.

## The build

The scorer ([`Scoring/EnglishScore.cs`](../../src/Cryptopals/Scoring/EnglishScore.cs)) walks each
byte and adds a weight depending on its class:

```csharp
if (b < 32 || b > 126)        { charScore = -5;  cls = "unprintable"; }   // can't be English
else if (char.IsLetter((char)b) || b == ' ')
{
    char upper = char.ToUpper((char)b);
    charScore = frequencies.GetValueOrDefault(upper, 0);                  // weight by frequency
    cls = b == ' ' ? "space" : "letter";
}
else                          { charScore = 0.5; cls = "other"; }         // punctuation/digits
score += charScore;
```

Higher means more English. The cracker is the brute-force loop: try every key, score the
decryption, keep the best ([`Set01/Challenge03.cs`](../../src/Cryptopals/Set01/Challenge03.cs)):

```csharp
for (int key = 0; key < 256; key++)
{
    byte[] decrypted = Xor.SingleByte(ciphertext, (byte)key, null);
    double score = score01(decrypted);
    if (score > bestScore) { bestScore = score; bestKey = (byte)key; bestPlaintext = decrypted; }
}
```

## Our own corpus, and overloading the scorer

The weights are not a stock table copied off the internet. We **wrote our own corpus**: a passage
of original prose baked into [`Scoring/EnglishScore.cs`](../../src/Cryptopals/Scoring/EnglishScore.cs)
as `CorpusText`, and `CorpusAnalysis` measures the actual letter frequencies from it when the type
is first loaded. The numbers you see in the trace (`'e' → 10.44`, `' ' → 18.51`) are counted from
*our* text, not handed to us.

And because the corpus is just data, the scorer is **swappable at two levels**:

```csharp
// Level 1: swap the frequency table. Any table works; two ship in the box.
public static double Score(byte[] bytes, IReadOnlyDictionary<char, double> frequencies, ...);
public static double ScoreCorpus(byte[] bytes)   => Score(bytes, Frequencies);          // measured from CorpusText
public static double ScoreStandard(byte[] bytes) => Score(bytes, StandardFrequencies);  // classic ETAOIN SHRDLU table

// Level 2: swap the WHOLE scorer. Crack takes a pluggable function (defaults to the corpus score).
public static CrackResult Crack(byte[] ciphertext, Action<string>? trace = null, Func<byte[], double>? scorer = null);
```

So the same brute-force loop can score against our corpus-measured table, the classic ETAOIN SHRDLU
table, a table built from a *different* corpus (formal prose, slang, another language), or a
completely different scoring function (a bigram model, say), all without touching the cracker. The
corpus is a knob, not a constant.

## Seeing it happen

The 256-key sweep is quiet (one line per key). The wrong keys cluster at a floor score because
their decryptions are mostly unprintable bytes:

```
Key 0xFD ('·') → score  -170.00
Key 0xFE ('·') → score  -170.00
Key 0xFF ('·') → score  -170.00

─── Winner: key 0x58 ('X') score 217.33 ───
```

Only the winner gets the loud, per-byte treatment. The decryption resolves to letters, and the
scorer tallies each one (notice how every space adds a fat `18.51` and common letters like `e`
add `10.44`):

```
XOR decryption:
  1B ^ 58 = 43  'C'
  37 ^ 58 = 6F  'o'
  37 ^ 58 = 6F  'o'
  33 ^ 58 = 6B  'k'
  ...
English scoring:
  char  class          value   running
  'C'   letter          1.87      1.87
  'o'   letter          5.62      7.50
  'o'   letter          5.62     13.12
  ' '   space          18.51     43.94
  'e'   letter         10.44     91.97
  ...
  TOTAL                         217.33
```

## The reveal

```
key  = 0x58 ('X')
text = "Cooking MC's like a pound of bacon"
```

The breakthrough idea is that **a scoring function turns guessing into searching**: once "looks
like English" is a number, breaking the cipher is just `argmax` over the keyspace. That exact
scorer becomes a *detector* in [Challenge 4](Challenge04.md), where it has to find the one encrypted
line hiding among hundreds of random ones.
