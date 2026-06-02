namespace Cryptopals;

/// <summary>
/// One visual grammar for all trace output. These are extension methods on
/// <c>Action&lt;string&gt;?</c> — and because extension methods bind statically,
/// they can be called even when the sink is null (each just no-ops). That removes
/// the <c>trace?.Invoke(...)</c> ceremony from every primitive and gives the whole
/// library a single, consistent look: sections, indented detail, blank lines.
///
/// Division of labour stays the same: compositions call <see cref="Section"/> to tell
/// the story; primitives call <see cref="Detail"/> to show the mechanics.
/// </summary>
public static class TraceExtensions
{
    /// <summary>A plain line. No argument = a blank line, for vertical spacing.</summary>
    public static void Line(this Action<string>? trace, string text = "") => trace?.Invoke(text);

    /// <summary>A section header: a blank line, then <c>─── title ───</c>.</summary>
    public static void Section(this Action<string>? trace, string title)
    {
        if (trace is null) return;
        trace("");
        trace($"─── {title} ───");
    }

    /// <summary>An indented detail line, sitting visually under a section.</summary>
    public static void Detail(this Action<string>? trace, string text) => trace?.Invoke("  " + text);
}
