using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lexi;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "it's a struct")]
public readonly ref struct Source(
    string text,
    int offset)
{
    public Source(string source)
        : this(source, 0)
    {
    }

    private readonly string text = text ?? throw new ArgumentNullException(nameof(text));
    public readonly int Offset = Math.Clamp(offset, 0, Int32.MaxValue);
    public bool IsEndOfSource => Offset >= text.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadSymbol(ref readonly Symbol symbol) => symbol.IsEndOfSource
            ? "EOF"
            : symbol.IsError
                ? $"lexer error at offset: {symbol.Offset}"
                : text[symbol.Offset..(symbol.Offset + symbol.Length)];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string(Source script) => script.text;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Source(string source) => new(source);
}
