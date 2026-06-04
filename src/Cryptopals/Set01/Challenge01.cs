using Cryptopals.Codecs;
using Cryptopals.Visualization;

namespace Cryptopals.Set01;

/// <summary>
/// Set 1 · Challenge 1 — Convert hex to base64.
/// https://cryptopals.com/sets/1/challenges/1
/// </summary>
public static class Challenge01
{
    /// <summary>
    /// Decode a hex string to its raw bytes, then re-encode those bytes as base64.
    /// Always operate on the raw bytes — never go hex-string -> base64-string directly.
    /// </summary>
    public static string HexToBase64(string hex, Action<string>? trace = null)
    {
        trace.Line($"hex input ({hex.Length} chars): {hex}");

        trace.Section("Hex → Bytes");
        byte[] bytes = Hex.Decode(hex, trace);
        trace.Line($"decoded ({bytes.Length} bytes): \"{bytes.ToAscii()}\"");

        trace.Section("Bytes → Base64");
        string base64 = Base64.Encode(bytes, trace);
        trace.Line($"result: {base64}");
        return base64;
    }
}
