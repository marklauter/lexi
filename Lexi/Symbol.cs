using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lexi;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "it's a struct")]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct Symbol(
    int offset,
    int length,
    uint tokenId)
{
    public readonly int Offset = offset < 0
        ? throw new ArgumentOutOfRangeException(nameof(offset))
        : offset;
    public readonly int Length = length < 0
        ? throw new ArgumentOutOfRangeException(nameof(length))
        : length;
    public readonly uint TokenId = tokenId;

    public bool IsMatch => Length > 0;
    public bool IsError => TokenId == Pattern.LexError;
    public bool IsEndOfSource => TokenId == Pattern.EndOfSource;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is(uint tokenId) => TokenId == tokenId;
}
