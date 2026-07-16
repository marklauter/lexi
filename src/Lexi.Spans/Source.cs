namespace Lexi.Spans;

/// <summary>
/// The source text as a <see cref="ReadOnlySpan{Char}"/>. A <c>ref struct</c> because the span lives
/// <b>only</b> here — never inside a <see cref="Symbol"/> — so only <see cref="Source"/> (and anything
/// that holds it) carries the ref-struct constraint. Tokens leave the lexer as plain structs.
/// </summary>
public readonly ref struct Source(ReadOnlySpan<char> text)
{
    /// <summary>The underlying span.</summary>
    public ReadOnlySpan<char> Span { get; } = text;

    /// <summary>The length of the source.</summary>
    public int Length => Span.Length;

    /// <summary>Creates a source over the given string's span.</summary>
    public static Source FromString(string text) =>
        new((text ?? throw new ArgumentNullException(nameof(text))).AsSpan());

    /// <summary>Returns the span slice a symbol names. Text is extracted on demand, not carried by the token.</summary>
    public ReadOnlySpan<char> Slice(Symbol symbol) =>
        Span.Slice(symbol.Offset, symbol.Length);
}
