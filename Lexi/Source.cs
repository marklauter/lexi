using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lexi;

/// <summary>
/// The text and current offset of the source code being analyzed.
/// </summary>
/// <param name="text"><see cref="String"/></param>
/// <param name="offset"><see cref="Int32"/></param>
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "it's a struct")]
[DebuggerDisplay("{Offset}, {text}")]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct Source(
    string text,
    int offset)
{
    /// <summary>
    /// Initializes the source with the text and zero offset.
    /// </summary>
    /// <param name="source"></param>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Source(string source)
        : this(source, 0)
    {
    }

    private readonly string text = text ?? throw new ArgumentNullException(nameof(text));

    /// <summary>
    /// The offset or position in the source code.
    /// </summary>
    public readonly int Offset = Math.Clamp(offset, 0, Int32.MaxValue);

    /// <summary>
    /// Returns true if the offset is at the end of the source.
    /// </summary>
    public bool IsEndOfSource => Offset >= text.Length;

    /// <summary>
    /// Returns the remainding text after the offset.
    /// </summary>
    /// <returns><see cref="String"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Remaining() => text[Offset..];

    /// <summary>
    /// Reads the symbol from the source.
    /// </summary>
    /// <param name="symbol"><see cref="Symbol"/></param>
    /// <returns><see cref="String"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadSymbol(ref readonly Symbol symbol) => symbol.IsEndOfSource
            ? "EOF"
            : !symbol.IsMatch
                ? $"lexer error at offset: {symbol.Offset}"
                : text[symbol.Offset..(symbol.Offset + symbol.Length)];

    /// <summary>
    /// Implicit operator converts source to string.
    /// </summary>
    /// <param name="script"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string(Source script) => script.text;

    /// <summary>
    /// Implicit operator converts string to source with zero offset.
    /// </summary>
    /// <param name="source"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Source(string source) => new(source);
}
