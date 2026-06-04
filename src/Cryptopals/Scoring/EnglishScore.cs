namespace Cryptopals.Scoring;

/// <summary>
/// Scores how much a byte buffer "looks like English text".
/// This is the heart of Challenge 3: it turns your human ability to recognize
/// English into a single comparable number, so a program can rank 256 candidates.
///
/// Design is YOURS to decide (your answer to question 3). Some directions:
///   - count printable / letter / space characters
///   - penalize unprintable bytes
///   - weight by letter-frequency (ETAOIN SHRDLU...) for a sharper signal
/// Whatever you choose, decide the DIRECTION and document it: does a higher score
/// mean "more English" or "less"? The solver depends on that contract.
/// </summary>
public static class EnglishScore
{

    /// <summary>Frequency table measured from <see cref="CorpusText"/> (your corpus).</summary>
    public static readonly Dictionary<char, double> Frequencies;

    /// <summary>
    /// A general English table — standard letter frequencies (ETAOIN SHRDLU…), NOT derived
    /// from any single corpus, plus space weighted as the most common character. Use this to
    /// compare a hand-tuned general model against your corpus-measured one.
    /// </summary>
    public static readonly IReadOnlyDictionary<char, double> StandardFrequencies = new Dictionary<char, double>
    {
        [' '] = 18.0,                                                   // most common character of all
        ['E'] = 12.7, ['T'] = 9.1, ['A'] = 8.2, ['O'] = 7.5, ['I'] = 7.0, ['N'] = 6.7,
        ['S'] = 6.3, ['H'] = 6.1, ['R'] = 6.0, ['D'] = 4.3, ['L'] = 4.0, ['C'] = 2.8,
        ['U'] = 2.8, ['M'] = 2.4, ['W'] = 2.4, ['F'] = 2.2, ['G'] = 2.0, ['Y'] = 2.0,
        ['P'] = 1.9, ['B'] = 1.5, ['V'] = 1.0, ['K'] = 0.8, ['J'] = 0.15, ['X'] = 0.15,
        ['Q'] = 0.10, ['Z'] = 0.07,
    };

    static EnglishScore()
    {
        Frequencies = CorpusAnalysis(CorpusText);
    }

    /// <summary>Score using the default corpus-derived table.</summary>
    public static double Score(byte[] bytes, Action<string>? trace = null)
        => Score(bytes, Frequencies, trace);

    /// <summary>
    /// Score using a SPECIFIC frequency table. Swapping tables tailors the scorer to the
    /// expected plaintext domain — formal prose, slang, a different language — which is the
    /// whole point of letting the corpus be a parameter.
    /// </summary>
    public static double Score(byte[] bytes, IReadOnlyDictionary<char, double> frequencies, Action<string>? trace = null)
    {
        // Per-character breakdown. Fires only when traced — which (because Crack
        // withholds the sink during its 256-key search) means only for the winner.
        trace.Detail($"{"char",-6}{"class",-12}{"value",8}{"running",10}");

        double score = 0;
        foreach (byte b in bytes)
        {
            double charScore;
            string cls;

            if (b < 32 || b > 126)
            {
                charScore = -5;            // Unprintable: strong negative signal.
                cls = "unprintable";
            }
            else if (char.IsLetter((char)b) || b == ' ')
            {
                // Letters and space, weighted by their frequency in the chosen table.
                char upper = char.ToUpper((char)b);
                charScore = frequencies.GetValueOrDefault(upper, 0);
                cls = b == ' ' ? "space" : "letter";
            }
            else
            {
                charScore = 0.5;           // Other printable (punctuation, digits): small positive.
                cls = "other";
            }

            score += charScore;
            trace.Detail($"{"'" + ((byte)b).ToAscii() + "'",-6}{cls,-12}{charScore,8:F2}{score,10:F2}");
        }

        trace.Detail($"{"TOTAL",-26}{score,10:F2}");
        return score;
    }

    // Single-argument handles so a scorer can be passed as Func&lt;byte[], double&gt; into Crack.
    public static double ScoreCorpus(byte[] bytes) => Score(bytes, Frequencies);
    public static double ScoreStandard(byte[] bytes) => Score(bytes, StandardFrequencies);

    public static Dictionary<char, double> CorpusAnalysis(string text, Action<string>? trace = null)
    {
        Dictionary<char, int> frequency = new Dictionary<char, int>();
        int totalChars = 0;

        foreach (char c in text)
        {
            if (char.IsLetter(c) || c == ' ')
            {
                char upperC = char.ToUpper(c);
                if (!frequency.ContainsKey(upperC))
                    frequency[upperC] = 0;
                frequency[upperC]++;
                totalChars++;
            }
        }

        Dictionary<char, double> frequencyPercentages = new Dictionary<char, double>();
        foreach (var key in frequency.Keys.ToList())
        {
            frequencyPercentages[key] = (frequency[key] / (double)totalChars) * 100;
        }

        return frequencyPercentages;
    }

      public static string CorpusText =
    """
    The night that William Jones's world changed began like any other.

    At six o'clock he rose from his bed, made his prayers and his ablutions. At quarter-past six he took tea and toast with his wife, Elanor, in their front parlour. And at half-past six, to the beat of the bell of the grandfather clock, he buttoned up his coat, pulled his hat down upon his head, kissed his wife and lifted the latch of his front door.

    The steady pace of his footsteps marked out the half-hour walk across Oxford. It was a cold February night. The sky was clear and pinpricked with stars. The moon was nothing but a splinter, the curl of a stray feather stuck to the velvet dark of the sky. William pulled up his collar and watched the mists of his breath rope through the air before him.

    He always loved the turning from the lanes of Jericho out on to St Giles. It was an invisible boundary between the quiet domestic world where he was a loving husband and the University where he was a watchman at the college gates. Every time he trod this path he would reflect how the change in the streets echoed the differences between his worlds. The roads of Jericho twisted in upon themselves, and a man could get easily lost. It was sometimes thus when he was sitting by the fireside with his wife. The conversation would ebb and flow between them, full of affection, and talk of the daughter that was blossoming in her belly. But there were times when there were shadowed corners in their speech, when a thing might not mean to Eleanor what it meant to him, and he would feel that he had taken a wrong turning down a dark alley, and was sitting in a room that seemed in outwards appearance to be his home, but was not.

    Whereas when he emerged on to the University streets, there stood the broad walls of the colleges, set shoulder to shoulder, their domes, spires and battlements pointing magnificently towards the heavens. And here William knew exactly who he was: he was Porter Jones, warden of the nights, the man who watched over great minds as they slumbered. Here William had a place and a function, and no one could shift him from it.

    But on this particular evening, the University was retreating from him as he walked through it. It was often thus when the moon waned. The college walls were swallowed by the night, the lamps that hung over the entrances illuminated them in piecemeal: the mouth of a doorway, say, or the curve of a window. As the scholars slept, it was as if the University simply dissolved itself, brick by brick, stone by stone, and drifted off into the night, leaving only a cornice here, a buttress there, and a few curious gargoyles peering down at the shattered world below.
    """;
}
