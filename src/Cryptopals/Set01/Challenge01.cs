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
        trace?.Invoke($"hex input ({hex.Length} chars): {hex}");
        trace?.Invoke("");
        trace?.Invoke("─── Hex → Bytes ───");

        byte[] bytes = Hex.Decode(hex, trace);
        trace?.Invoke($"decoded ({bytes.Length} bytes): \"{bytes.ToAscii()}\"");
        trace?.Invoke("");

        trace?.Invoke("─── Bytes → Base64 ───");

        string base64 = Base64.Encode(bytes, trace);
        trace?.Invoke($"result: {base64}");
        return base64;
    }
}
