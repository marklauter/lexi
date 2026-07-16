using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lexi;

/// <summary>
/// The source text and current offset of the code being analyzed. A <c>ref struct</c> over a
/// <see cref="ReadOnlySpan{Char}"/>: the span lives <b>only</b> here — never inside a <see cref="Symbol"/> —
/// so text is extracted on demand via <see cref="ReadSymbol"/> (a span, no copy) rather than carried by the
/// token. Tokens leave the lexer as plain, collectible <see cref="Symbol"/> values.
/// </summary>
[DebuggerDisplay("Offset = {Offset}")]
public readonly ref struct Source
{
    private readonly ReadOnlySpan<char> text;

    /// <summary>
    /// Initializes the source with the span and offset.
    /// </summary>
    /// <param name="text"><see cref="ReadOnlySpan{Char}"/></param>
    /// <param name="offset"><see cref="int"/></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Source(ReadOnlySpan<char> text, int offset)
    {
        this.text = text;
        Offset = Math.Clamp(offset, 0, int.MaxValue);
    }

    /// <summary>
    /// Initializes the source with the span and zero offset.
    /// </summary>
    /// <param name="text"><see cref="ReadOnlySpan{Char}"/></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Source(ReadOnlySpan<char> text)
        : this(text, 0)
    {
    }

    /// <summary>
    /// Initializes the source with the text and offset.
    /// </summary>
    /// <param name="text"><see cref="string"/></param>
    /// <param name="offset"><see cref="int"/></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Source(string text, int offset)
        : this((text ?? throw new ArgumentNullException(nameof(text))).AsSpan(), offset)
    {
    }

    /// <summary>
    /// Initializes the source with the text and zero offset.
    /// </summary>
    /// <param name="text"><see cref="string"/></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Source(string text)
        : this(text, 0)
    {
    }

    /// <summary>
    /// The offset or position in the source code.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// The underlying source span.
    /// </summary>
    public ReadOnlySpan<char> Span => text;

    /// <summary>
    /// Returns true if the offset is at the end of the source.
    /// </summary>
    public bool IsEndOfSource => Offset >= text.Length;

    /// <summary>
    /// Returns the remaining text after the offset.
    /// </summary>
    /// <returns><see cref="string"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Remaining() => text[Offset..].ToString();

    /// <summary>
    /// Reads the symbol's text from the source as a span — no substring is allocated.
    /// </summary>
    /// <param name="symbol"><see cref="Symbol"/></param>
    /// <returns><see cref="ReadOnlySpan{Char}"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> ReadSymbol(ref readonly Symbol symbol) => symbol.IsEndOfSource
        ? "EOF"
        : !symbol.IsMatch
            ? symbol.Offset < text.Length
                ? $"lexer error at offset {symbol.Offset}: unexpected '{text[symbol.Offset]}'"
                : $"lexer error at offset: {symbol.Offset}"
            : text.Slice(symbol.Offset, symbol.Length);

    /// <summary>
    /// Returns the full source text.
    /// </summary>
    /// <returns><see cref="string"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => text.ToString();

    /// <summary>
    /// Creates a source from the text with zero offset.
    /// </summary>
    /// <param name="source"><see cref="string"/></param>
    /// <returns><see cref="Source"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Source FromString(string source) => new(source);
}
