using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lexi;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "it's a struct")]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct Symbol(
    int offset,
    int length,
    int tokenId)
{
    public readonly int Offset = Math.Clamp(offset, 0, Int32.MaxValue);
    public readonly int Length = Math.Clamp(length, 0, Int32.MaxValue);
    public readonly int TokenId = tokenId;

    public bool IsMatch => Length > 0
        && TokenId is not Pattern.NoMatch or Pattern.EndOfSource or Pattern.LexError;
    public bool IsError => TokenId == Pattern.LexError;
    public bool IsEndOfSource => TokenId == Pattern.EndOfSource;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is(int tokenId) => TokenId == tokenId;
}
